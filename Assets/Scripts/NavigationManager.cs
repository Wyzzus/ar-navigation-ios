using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class NavigationManager : MonoBehaviour
{

    #region Singleton
    public static NavigationManager instance;

    public void Awake()
    {
        instance = this;
    }

    #endregion

    //public NavigationFiled LoadedField;
    public List<Transform> WorkField;
    public List<TargetScript> WorkTargets;

    public NavMeshPath CurrentPath;
    public LayerMask NormalMask;
    public LayerMask DebugMask;

    public GameObject FieldPartPrefab;
    public GameObject TargetPrefab;
    public bool AnchorFound;
    public bool AnchorPlaced;
    public bool Recording;
    public AnchorScript Anchor;
    public GameObject TrackedImage;
    public Transform Agent;
    public GameObject EmptyObject;
    public GameObject Path;
    public GameObject RoutePartPrefab;

    public bool CanRecord;

    public float DistanceOfSetup = 0.2f;
    public float MaxDistance = 2f;

    [Header("UI")]
    public Button RecordButton;
    public Button AddButton;
    public Text Error;
    public GameObject ErrorDialog;
    public GameObject TargetDialog;
    public InputField TargetName;

    public RectTransform content;
    public GameObject RoutesDialog;
    public GameObject RouteButtonPrefab;

    public GameObject GetFieldPart()
    {
        return FieldPartPrefab;
    }
    public Transform GetAnchor()
    {
        return Anchor.transform;
    }

	public void Start()
	{
        Camera.main.cullingMask = NormalMask;
        //GetPath(new Vector3(100, 10, 0));
	}

    public void TestSave()
    {
        //SaveLoadManager.instance.AddField("Test", WorkField, WorkTargets);
        SaveLoadManager.instance.Load();
    }

	public void Update()
    {
        ButtonManagement();
        TrackedImage = GameObject.FindGameObjectWithTag("ImageAnchor");

        if (TrackedImage.GetComponent<MeshRenderer>().enabled)
        {
            Error.text = "";
            AnchorFound = true;
        }
        else if (!Recording)
        {
            Error.text = "Найтите метку";
            AnchorFound = false;
            AnchorPlaced = false;
        }

        if(AnchorFound)
        {
            if(!AnchorPlaced)
            {
                Anchor.transform.position = TrackedImage.transform.position;
                AnchorPlaced = true;
            }
            CanRecord = true;
        }

        if (CanRecord)
        {
            if (Recording)
                Record();
        }

    }

    public void GetPath(TargetScript target)
    {
        CurrentPath = new NavMeshPath();
        NavMeshHit hitAgent;
        NavMesh.SamplePosition(Agent.position, out hitAgent, 1000, -1);
        NavMeshHit hitPoint;
        NavMesh.SamplePosition(target.position, out hitPoint, 1000, -1);
        bool Success = NavMesh.CalculatePath(hitAgent.position, hitPoint.position, NavMesh.AllAreas, CurrentPath);
        if(Success)
        {
            Debug.Log(CurrentPath.corners);
            CreatePath();
        }
        else
        {
            ErrorDialog.SetActive(true);
            ErrorDialog.GetComponentInChildren<Text>().text = "Невозможно построить маршрут";
        }
    }

    public void ClearPath()
    {
        if (Path)
            Destroy(Path);
    }

    public void CreatePath()
    {
        ClearPath();

        Path = Instantiate<GameObject>(EmptyObject);

        for (int i = 0; i < CurrentPath.corners.Length - 1; i++)
        {

            SetRoutePart(CurrentPath.corners[i], CurrentPath.corners[i + 1]);
        }
        Vector3 last = CurrentPath.corners[CurrentPath.corners.Length - 1];
        Vector3 preLast = CurrentPath.corners[CurrentPath.corners.Length - 2];
        SetRoutePart(last, preLast);
    }

    public void SetRoutePart(Vector3 from, Vector3 to)
    {
        GameObject clone = Instantiate<GameObject>(RoutePartPrefab, from, Quaternion.identity);
        clone.transform.LookAt(to);
        float dist = Vector3.Distance(from, to);
        clone.GetComponent<RoutePart>().SetupPart(dist);
        clone.transform.parent = Path.transform;
    }

    public void ShowHideObject(GameObject obj)
    {
        obj.SetActive(!obj.activeSelf);
    }


    public void StartTargetDialog()
    {
        TargetDialog.SetActive(true);
    }

    public void AddTarget()
    {
        if(TargetName.text.Length != 0)
        {
            GameObject clone = Instantiate<GameObject>(TargetPrefab, Agent.position, Quaternion.identity);
            clone.transform.position = new Vector3(Agent.position.x, -0.5f, Agent.position.z);
            clone.transform.parent = Anchor.TargetsRoot;
            TargetScript target = clone.GetComponent<TargetScript>();
            target.Name = TargetName.text;
            TargetName.text = "";
            target.position = clone.transform.localPosition;
            WorkTargets.Add(target);
            TargetDialog.SetActive(false);
            ShowTargets();
        }
    }

    public void ButtonManagement()
    {
        if (CanRecord)
        {
            RecordButton.interactable = true;
            AddButton.interactable = true;
        }
        else
        {
            RecordButton.interactable = false;
            AddButton.interactable = false;
        }
    }

    public void StartStopRecording()
    {
        Recording = !Recording;
        if (!Recording)
        {
            RecordButton.GetComponentInChildren<Text>().text = "Начать запись";
            SetPartOn(Agent.position, GetNearestPart().position);
            Anchor.GenerateNavMesh();
        }
        else
        {
            RecordButton.GetComponentInChildren<Text>().text = "Прекратить запись";

        }
    }

    public void ShowTargets()
    {
        foreach (RectTransform child in content)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < WorkTargets.Count; i++)
        {
            GameObject clone = Instantiate<GameObject>(RouteButtonPrefab, content);
            clone.GetComponent<RouteUI>().target = WorkTargets[i];
        }
    }

    public void SetupField()
    {
        //LoadedField.SetupField();
    }

    public void ChangeMask()
    {
        if (Camera.main.cullingMask == NormalMask)
            Camera.main.cullingMask = DebugMask;
        else
            Camera.main.cullingMask = NormalMask;
    }

    public void Record()
    {
        if (WorkField.Count != 0)
        {
            Transform NearestPart = GetNearestPart();
            float dist = Vector3.Distance(NearestPart.position, Agent.position);
            if (dist >= DistanceOfSetup)
            {
                SetPartOn(Agent.position, NearestPart.position);
            }
        }
        else
        {
            SetPartOn(Agent.position, Agent.position - Vector3.forward/2 - Vector3.up/2);
        }
    }

    public Transform GetNearestPart()
    {
        int Nearest = 0;
        float minDist = float.MaxValue;
        for (int i = 1; i < WorkField.Count; i++)
        {
            float dist = Vector3.Distance(WorkField[i].position, Agent.position);
            if (dist < minDist)
            {
                minDist = dist;
                Nearest = i;
            }
        }

        return WorkField[Nearest];
    }

    public void SetPartOn(Vector3 position, Vector3 to)
    {
        GameObject clone = Instantiate<GameObject>(FieldPartPrefab, position, Quaternion.identity);
        clone.transform.position = new Vector3(position.x, -0.5f, position.z);
        clone.transform.LookAt(to);
        //clone.transform.localScale = new Vector3(1, 1, Vector3.Distance(position, to));
        clone.GetComponent<PartScript>().SetupPart(Vector3.Distance(position, to));
        WorkField.Add(clone.transform);
        clone.transform.parent = Anchor.PointsRoot;
    }
}
