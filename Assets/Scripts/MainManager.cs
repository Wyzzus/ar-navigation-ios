using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{

    public List<Vector3> Points = new List<Vector3>();
    public Transform RootPoint;

    [Header("Messages")]
    public GameObject[] Messages;

    [Header("Admin Panel")]
    public bool CanCreatePath = false;
    public bool Debugging;
    public InputField PathName;

    [Header("Path")]
    public GameObject PointPrefab;
    public GameObject Buttons;

    [Header("UIs")]
    public GameObject[] Layers;
    public RectTransform Content;

    public Data CurrentSession;
    // Use this for initialization
    void Start()
    {
        System.Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
        LoadPaths();
        ShowPaths();
    }

    // Update is called once per frame
    void Update()
    {
        GameObject tmp = GameObject.FindGameObjectWithTag("SafeNet");
        if(tmp )
        {
            if(tmp.activeSelf && RootPoint == null)
                RootPoint = tmp.transform;
        }

        if (CanCreatePath)
            StartCreatePath();
        if (Debugging)
            Camera.main.transform.position += new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (RootPoint == null)
            Content.gameObject.SetActive(false);
        else
            Content.gameObject.SetActive(true);


    }

    public void StartCreate()
    {
        CanCreatePath = true;
    }

    public void StartCreatePath()
    {
        if (!RootIsFound())
            SetActiveMessage(0);
        else
        {
            if (Points.Count == 0)
                SetPoint();
            SetActiveMessage(1);
            CreatingPath();
        }
    }

    public void SetPoint()
    {
        GameObject Point = Instantiate<GameObject>(PointPrefab, RootPoint);
        Point.transform.position = Camera.main.transform.position - Vector3.up;
        Points.Add(Point.transform.position);
        if (Points.Count > 1)
        {
            Vector3 lastPoint = Points[Points.Count - 2];

            Point.transform.LookAt(lastPoint);
            Point.transform.GetChild(0).localScale = new Vector3(1, 1, Vector3.Distance(Point.transform.position, lastPoint) / 2);
        }
        else
        {
            Point.transform.GetChild(0).localScale = new Vector3(1, 1, 0);
        }
    }

    public bool RootIsFound()
    {
        GameObject tmp = GameObject.FindGameObjectWithTag("SafeNet");
        if (RootPoint == null && tmp)
        {
            RootPoint = tmp.transform;
            return true;
        }
        else
            return false;
    }

    public void SetActiveMessage(int id)
    {
        if (id == -1)
            foreach (GameObject go in Messages)
                go.SetActive(false);
        else
        {
            if (!Messages[id].activeSelf)
            {
                for (int i = 0; i < Messages.Length; i++)
                {
                    Messages[i].SetActive(false);
                }
                Messages[id].SetActive(true);
            }
        }
    }

    public void CreatingPath()
    {
        Vector3 lastPoin = Points[Points.Count - 1];
        if (Vector3.Distance(lastPoin, Camera.main.transform.position) > 2)
            SetPoint();
    }

    public void SavePoints()
    {
        CanCreatePath = false;
        MyPath newPath = new MyPath();
        newPath.Points = new List<CustomVector>();
        SphereCollider[] tmp = RootPoint.GetComponentsInChildren<SphereCollider>();
        foreach (SphereCollider sph in tmp)
        {
            CustomVector Point = new CustomVector(sph.transform.localPosition);
            newPath.Points.Add(Point);
        }

        newPath.Name = PathName.text;

        if (!CurrentSession.Paths.Contains(newPath))
            CurrentSession.Paths.Add(newPath);


        BinaryFormatter formatter = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/paths.save");
        formatter.Serialize(file, CurrentSession);
        file.Close();
        SetActiveMessage(-1);
        ShowPaths();

    }

    public void LoadPaths()
    {
        if (File.Exists(Application.persistentDataPath + "/paths.save"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/paths.save", FileMode.Open);
            CurrentSession = (Data)bf.Deserialize(file);
            file.Close();
        }
        ShowPaths();
    }

    /*
     * 
     * 
            if (RootIsFound())
                CreatePath(0);
            else
                StartCoroutine(CloseMessage(Messages[0],2));
     * 
    */

    public void ShowPaths()
    {
        PathHolder[] phs = Content.GetComponentsInChildren<PathHolder>();
        foreach (PathHolder ph in phs)
        {
            Destroy(ph.gameObject);
        }
        for (int i = 0; i < CurrentSession.Paths.Count; i++)
        {
            GameObject k = Instantiate<GameObject>(Buttons, Content);
            k.GetComponent<PathHolder>().Setup(CurrentSession.Paths[i].Name, i, this);
        }
        Content.sizeDelta = new Vector2(Content.sizeDelta.x, CurrentSession.Paths.Count * 50f);
    }

    public void BuildP(int id)
    {
        if(RootIsFound())
            CreatePath(id);
    }

    public void SetActiveLayer(int id)
    {
        for (int i = 0; i < Layers.Length; i++)
        {
            Layers[i].SetActive(false);
        }

        Layers[id].SetActive(true);
    }

    public void Clear()
    {
        SphereCollider[] oldPoints = RootPoint.GetComponentsInChildren<SphereCollider>();
        foreach (SphereCollider sph in oldPoints)
            Destroy(sph.gameObject);
    }
    public void CreatePath(int id)
    {
        Clear();
        MyPath currentPath = CurrentSession.Paths[id];
        Transform lastPoint = null;
        for (int i = 0; i < currentPath.Points.Count; i++)
        {
            GameObject point = Instantiate(PointPrefab, RootPoint);
            point.transform.parent = RootPoint;
            point.transform.localPosition = currentPath.Points[i].ToVector3();
            if (i > 0)
            {
                point.transform.LookAt(lastPoint);
                point.transform.GetChild(0).localScale = new Vector3(1, 1, Vector3.Distance(point.transform.position, lastPoint.position) / 2);
            }
            lastPoint = point.transform;
        }
    }

    public IEnumerator CloseMessage(GameObject om, float delay)
    {
        om.SetActive(true);
        yield return new WaitForSeconds(delay);
        om.SetActive(false);
    }
}

#region Data
[System.Serializable]
public class Data
{
    public List<MyPath> Paths;
}

[System.Serializable]
public class MyPath
{
    public string Name;
    public List<CustomVector> Points;
}

[System.Serializable]
public class CustomVector
{
    public float x;
    public float y;
    public float z;

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

    public CustomVector(Vector3 from)
    {
        x = from.x; y = from.y; z = from.z;
    }
}
#endregion
