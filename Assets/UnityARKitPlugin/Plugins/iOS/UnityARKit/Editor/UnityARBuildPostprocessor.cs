using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using UnityEngine.XR.iOS;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System;


public class UnityARBuildPostprocessor
{
    static List<ARReferenceImagesSet> imageSets = new List<ARReferenceImagesSet>();
    // Build postprocessor. Currently only needed on:
    // - iOS: no dynamic libraries, so plugin source files have to be copied into Xcode project
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.iOS)
            OnPostprocessBuildIOS(pathToBuiltProject);
    }

    [PostProcessScene]
    public static void OnPostProcessScene()
    {
        if (!BuildPipeline.isBuildingPlayer)
            return;

        foreach (ARReferenceImagesSet ar in UnityEngine.Resources.FindObjectsOfTypeAll<ARReferenceImagesSet>())
        {
            if (!imageSets.Contains(ar))
            {
                imageSets.Add(ar);
            }
        }

    }

    private static UnityARKitPluginSettings LoadSettings()
    {
        UnityARKitPluginSettings loadedSettings = Resources.Load<UnityARKitPluginSettings>("UnityARKitPlugin/ARKitSettings");
        if (loadedSettings == null)
        {
            loadedSettings = ScriptableObject.CreateInstance<UnityARKitPluginSettings>();
        }
        return loadedSettings;
    }

    // Replaces the first C++ macro with the given name in the source file. Only changes
    // single-line macro declarations, if multi-line macro declaration is detected, the
    // function returns without changing it. Macro name must be a valid C++ identifier.
    internal static bool ReplaceCppMacro(string[] lines, string name, string newValue)
    {
        bool replaced = false;
        Regex matchRegex = new Regex(@"^.*#\s*define\s+" + name);
        Regex replaceRegex = new Regex(@"^.*#\s*define\s+" + name + @"(:?|\s|\s.*[^\\])$");
        for (int i = 0; i < lines.Count(); i++)
        {
            if (matchRegex.Match(lines[i]).Success)
            {
                lines[i] = replaceRegex.Replace(lines[i], "#define " + name + " " + newValue);
                replaced = true;
            }
        }
        return replaced;
    }

    internal static void AddOrReplaceCppMacro(ref string[] lines, string name, string newValue)
    {
        if (ReplaceCppMacro(lines, name, newValue) == false)
        {
            Array.Resize(ref lines, lines.Length + 1);
            lines[lines.Length - 1] = "#define " + name + " " + newValue;
        }
    }

    static void UpdateDefinesInFile(string file, Dictionary<string, bool> valuesToUpdate)
    {
        string[] src = File.ReadAllLines(file);
        var copy = (string[])src.Clone();

        foreach (var kvp in valuesToUpdate)
            AddOrReplaceCppMacro(ref copy, kvp.Key, kvp.Value ? "1" : "0");

        if (!copy.SequenceEqual(src))
            File.WriteAllLines(file, copy);
    }

#if UNITY_IOS
    static void AddReferenceImageToResourceGroup(ARReferenceImage arri, string parentFolderFullPath, string projectRelativePath, PBXProject project)
	{

		ARResourceContents resourceContents = new ARResourceContents ();
		resourceContents.info = new ARResourceInfo ();
		resourceContents.info.author = "xcode";
		resourceContents.info.version = 1;

		resourceContents.images = new ARResourceImage[1];
		resourceContents.images [0] = new ARResourceImage ();
		resourceContents.images [0].idiom = "universal";

		resourceContents.properties = new ARResourceProperties ();
		resourceContents.properties.width = arri.physicalSize;

		//add folder for reference image
		string folderToCreate = arri.imageName + ".arreferenceimage";
		string folderFullPath = Path.Combine (parentFolderFullPath, folderToCreate);
		string projectRelativeFolder = Path.Combine (projectRelativePath, folderToCreate);
		Directory.CreateDirectory (folderFullPath);
		project.AddFolderReference (folderFullPath, projectRelativeFolder);

		//copy file from texture asset
		string imagePath = AssetDatabase.GetAssetPath(arri.imageTexture);
		string imageFilename = Path.GetFileName (imagePath);
		var dstPath = Path.Combine(folderFullPath, imageFilename);
		File.Copy(imagePath, dstPath, true);
		project.AddFile (dstPath, Path.Combine (projectRelativeFolder, imageFilename));
		resourceContents.images [0].filename = imageFilename;

		//add contents.json file
		string contentsJsonPath = Path.Combine(folderFullPath, "Contents.json");
		File.WriteAllText (contentsJsonPath, JsonUtility.ToJson (resourceContents, true));
		project.AddFile (contentsJsonPath, Path.Combine (projectRelativeFolder, "Contents.json"));

	}

	static void AddReferenceImagesSetToAssetCatalog(ARReferenceImagesSet aris, string pathToBuiltProject, PBXProject project)
	{
		List<ARReferenceImage> processedImages = new List<ARReferenceImage> ();
		ARResourceGroupContents groupContents = new ARResourceGroupContents();
		groupContents.info = new ARResourceGroupInfo ();
		groupContents.info.author = "xcode";
		groupContents.info.version = 1;
		string folderToCreate = "Unity-iPhone/Images.xcassets/" + aris.resourceGroupName + ".arresourcegroup";
		string folderFullPath = Path.Combine (pathToBuiltProject, folderToCreate);
		Directory.CreateDirectory (folderFullPath);
		project.AddFolderReference (folderFullPath, folderToCreate);
		foreach (ARReferenceImage arri in aris.referenceImages) {
			if (!processedImages.Contains (arri)) {
				processedImages.Add (arri); //get rid of dupes
				AddReferenceImageToResourceGroup(arri, folderFullPath, folderToCreate, project);
			}
		}

		groupContents.resources = new ARResourceGroupResource[processedImages.Count];
		int index = 0;
		foreach (ARReferenceImage arri in processedImages) {
			groupContents.resources [index] = new ARResourceGroupResource ();
			groupContents.resources [index].filename = arri.imageName + ".arreferenceimage";
			index++;
		}
		string contentsJsonPath = Path.Combine(folderFullPath, "Contents.json");
		File.WriteAllText (contentsJsonPath, JsonUtility.ToJson (groupContents, true));
		project.AddFile (contentsJsonPath, Path.Combine (folderToCreate, "Contents.json"));
	}
#endif //UNITY_IOS

	private static void OnPostprocessBuildIOS(string pathToBuiltProject)
	{
		// We use UnityEditor.iOS.Xcode API which only exists in iOS editor module
#if UNITY_IOS
		string projPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";

		UnityEditor.iOS.Xcode.PBXProject proj = new UnityEditor.iOS.Xcode.PBXProject();
		proj.ReadFromString(File.ReadAllText(projPath));
		proj.AddFrameworkToProject(proj.TargetGuidByName("Unity-iPhone"), "ARKit.framework", false);
		string target = proj.TargetGuidByName("Unity-iPhone");
		Directory.CreateDirectory(Path.Combine(pathToBuiltProject, "Libraries/Unity"));

		// Check UnityARKitPluginSettings
		UnityARKitPluginSettings ps = LoadSettings();
		string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
		PlistDocument plist = new PlistDocument();
		plist.ReadFromString(File.ReadAllText(plistPath));
		PlistElementDict rootDict = plist.root;

		// Get or create array to manage device capabilities
		const string capsKey = "UIRequiredDeviceCapabilities";
		PlistElementArray capsArray;
		PlistElement pel;
		if (rootDict.values.TryGetValue(capsKey, out pel)) {
			capsArray = pel.AsArray();
		}
		else {
			capsArray = rootDict.CreateArray(capsKey);
		}
		// Remove any existing "arkit" plist entries
		const string arkitStr = "arkit";
		capsArray.values.RemoveAll(x => arkitStr.Equals(x.AsString()));
		if (ps.AppRequiresARKit) {
			// Add "arkit" plist entry
			capsArray.AddString(arkitStr);
		}
		File.WriteAllText(plistPath, plist.WriteToString());

		foreach(ARReferenceImagesSet ar in imageSets)
		{
			AddReferenceImagesSetToAssetCatalog(ar, pathToBuiltProject, proj);
		}

		//TODO: remove this when XCode actool is able to handles ARResources despite deployment target
		if (imageSets.Count > 0)
		{
			proj.SetBuildProperty(target, "IPHONEOS_DEPLOYMENT_TARGET", "11.3");
		}

		// Add or replace define for facetracking
		UpdateDefinesInFile(pathToBuiltProject + "/Classes/Preprocessor.h", new Dictionary<string, bool>() {
			{ "ARKIT_USES_FACETRACKING", ps.m_ARKitUsesFacetracking }
		});

		string[] filesToCopy = new string[]
		{
			
		};

		for(int i = 0 ; i < filesToCopy.Length ; ++i)
		{
			var srcPath = Path.Combine("../PluginSource/source", filesToCopy[i]);
			var dstLocalPath = "Libraries/" + filesToCopy[i];
			var dstPath = Path.Combine(pathToBuiltProject, dstLocalPath);
			File.Copy(srcPath, dstPath, true);
			proj.AddFileToBuild(target, proj.AddFile(dstLocalPath, dstLocalPath));
		}

		File.WriteAllText(projPath, proj.WriteToString());
#endif // #if UNITY_IOS
	}
}


/*
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
        //LoadPaths();
    }

    // Update is called once per frame
    void Update()
    {
        if (CanCreatePath)
            StartCreatePath();
        if (Debugging)
            Camera.main.transform.position += new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
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
        if (tmp)
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

    }

    public void LoadPaths()
    {
        if(File.Exists(Application.persistentDataPath + "/paths.save"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/paths.save", FileMode.Open);
            CurrentSession = (Data)bf.Deserialize(file);
            file.Close();
        }
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
/*
public void ShowPaths()
{
    f
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


*/
