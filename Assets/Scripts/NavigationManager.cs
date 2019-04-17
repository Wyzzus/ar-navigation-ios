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

    public int BuildOption = 0;

    public RectTransform ServicePanelContent;
    public GameObject PanelPartPrefab;
    public NavigationFiled LoadedField;
    public List<Transform> WorkField;
    public List<TargetScript> WorkTargets;

    public NavMeshPath CurrentPath;
    public TargetScript CurrentTarget;
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
    public GameObject CrossPrefab;
    public MeshRenderer AnchorModel;

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
    public GameObject BuilderPrefab;
    public Button CrossButton;

    public GameObject LeftPointer;
    public GameObject RightPointer;

    public float AngleBetween;

    public float Timer = 0;

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
        LoadField();
        //GetPath(new Vector3(100, 10, 0));
	}




    public void ShowService()
    {
        foreach (RectTransform child in ServicePanelContent)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < WorkTargets.Count; i++)
        {
            GameObject clone = Instantiate<GameObject>(PanelPartPrefab, ServicePanelContent);
            clone.GetComponent<ServicePanelPart>().targetScript = WorkTargets[i];
            clone.GetComponent<ServicePanelPart>().Load();
        }
        ServicePanelContent.sizeDelta = new Vector2(content.sizeDelta.x, WorkTargets.Count * 100);
    }












    public void TestSave()
    {
        //SaveLoadManager.instance.AddField("Test", WorkField, WorkTargets);
        SaveLoadManager.instance.Load();
    }

    public void SaveField()
    {
        SaveLoadManager.instance.AddField("Test", WorkField, WorkTargets);
        SaveLoadManager.instance.Save();
    }

    public void DeleteField()
    {
        SaveLoadManager.instance.DeleteField();
        WorkField.Clear();
        WorkTargets.Clear();

        foreach (Transform child in Anchor.PointsRoot)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in Anchor.TargetsRoot)
        {
            Destroy(child.gameObject);
        }
    }

    public void LoadField()
    {
        SaveLoadManager.instance.Load();
        WorkField.Clear();
        WorkTargets.Clear();

        foreach (Transform child in Anchor.PointsRoot)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in Anchor.TargetsRoot)
        {
            Destroy(child.gameObject);
        }

        if(SaveLoadManager.instance.CurrentData != null)
        {
            LoadedField = SaveLoadManager.instance.CurrentData.NavFields[0];
            Debug.Log(LoadedField.Name);
            for (int i = 0; i < LoadedField.Parts.Count; i++)
            {
                GameObject part = Instantiate<GameObject>(NavigationManager.instance.GetFieldPart(), Anchor.PointsRoot);
                part.transform.localPosition = LoadedField.Parts[i].position.GetRegularVector3();
                part.transform.localRotation = Quaternion.Euler(LoadedField.Parts[i].rotation.GetRegularVector3());
                part.transform.localScale = Vector3.one * LoadedField.Parts[i].shoulderScale;
                WorkField.Add(part.transform);
            }

            Anchor.GenerateNavMesh();

            for (int i = 0; i < LoadedField.Targets.Count; i++)
            {
                GameObject part = Instantiate<GameObject>(TargetPrefab, Anchor.TargetsRoot);
                part.transform.localPosition = LoadedField.Targets[i].position.GetRegularVector3();
                TargetScript ts = part.GetComponent<TargetScript>();
                ts.position = ts.transform.position;
                ts.Name = LoadedField.Targets[i].Name;
                ts.Description = LoadedField.Targets[i].Description;
                ts.ImageUrl = LoadedField.Targets[i].ImageUrl;
                ts.SetupTarget();
                WorkTargets.Add(ts);
            }
            ShowTargets();
        }
    }

	public void Update()
    {
        ButtonManagement();
        TrackedImage = GameObject.FindGameObjectWithTag("ImageAnchor");

        if (TrackedImage.GetComponent<MeshRenderer>().enabled && !AnchorFound)
        {
            //Debug.Log("Found!");
            Timer += Time.deltaTime;
            if(Timer < 1f)
            {
                Anchor.transform.position = TrackedImage.transform.position;
                Anchor.PointsRoot.rotation = TrackedImage.transform.rotation;
                Anchor.TargetsRoot.rotation = TrackedImage.transform.rotation;

            }
            else
            {
                AnchorPlaced = true;
                AnchorFound = true;
                Anchor.GenerateNavMesh();
                Timer = 0;
            }
        }

        if (!TrackedImage.GetComponent<MeshRenderer>().enabled)
            AnchorFound = false;
            

        if (AnchorFound)
        {
            Error.text = "";
        }
        else if (!AnchorPlaced)
        {
            Error.text = "Найдите метку";
            AnchorFound = false;
        }

        if (AnchorPlaced)
            CanRecord = true;

        if (CanRecord)
        {
            if (Recording)
                Record();
        }

        if(Path && CurrentTarget)
        {
            Vector3 targetDir = CurrentTarget.transform.position - Agent.transform.position;
            AngleBetween = Vector3.SignedAngle(targetDir, Agent.transform.forward, Vector3.up);

            if(Mathf.Abs(AngleBetween) > 20)
            {
                if(AngleBetween > 0)
                {
                    LeftPointer.SetActive(true);
                    RightPointer.SetActive(false);
                }
                else
                {
                    LeftPointer.SetActive(false);
                    RightPointer.SetActive(true);
                }
            }
            else
            {
                LeftPointer.SetActive(false);
                RightPointer.SetActive(false);
            }
            //Debug.Log(AngleBetween);
        }
        else
        {
            LeftPointer.SetActive(false);
            RightPointer.SetActive(false);
        }

    }

    public void GetPath(TargetScript target)
    {
        CurrentTarget = target;
        CurrentPath = new NavMeshPath();
        NavMeshHit hitAgent;
        NavMesh.SamplePosition(Agent.position, out hitAgent, 1000, -1);
        NavMeshHit hitPoint;
        NavMesh.SamplePosition(target.position, out hitPoint, 1000, -1);
        bool Success = NavMesh.CalculatePath(hitAgent.position, hitPoint.position, NavMesh.AllAreas, CurrentPath);
        if(Success)
        {
            Debug.Log(CurrentPath.corners);
            if (BuildOption == 0)
                CreatePath();
            else
                BuildPath(target.position);
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
        CurrentTarget = null;
    }


    public void BuildPath(Vector3 pos)
    {
        if (Path)
            Destroy(Path);

        Path = Instantiate<GameObject>(EmptyObject);
        GameObject clone = Instantiate<GameObject>(BuilderPrefab, Agent.position, Quaternion.identity);
        clone.GetComponent<Builder>().destination = pos; 
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

    public void AddCross()
    {
        SetCross(Agent.position);
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
            target.SetupTarget();
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
            CrossButton.interactable = true;
        }
        else
        {
            RecordButton.interactable = false;
            AddButton.interactable = false;
            CrossButton.interactable = false;
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
        content.sizeDelta = new Vector2(content.sizeDelta.x, WorkTargets.Count * 30);
                                   
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
                SetPartOn(Agent.position, NearestPart.localPosition);
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

    public void SetCross(Vector3 position)
    {
        GameObject clone = Instantiate<GameObject>(CrossPrefab, position, Quaternion.identity);
        clone.transform.position = new Vector3(position.x, -0.5f, position.z);
        WorkField.Add(clone.transform);
        clone.transform.parent = Anchor.PointsRoot;
    }
}
