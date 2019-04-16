using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveLoadManager : MonoBehaviour {

    public static SaveLoadManager instance;

    public NavigationData CurrentData;

    public string Folder;


	public void Awake()
	{
        instance = this;
	}

	public void Start()
	{
        Folder = Application.persistentDataPath + "/saves";
	}

	public void Save()
    {
        if (CurrentData != null)
        {
            if (!Directory.Exists(Folder))
            {
                Directory.CreateDirectory(Folder);
            }

            FileStream fs = new FileStream(Folder + "/data.hdt", FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(fs, CurrentData);
            fs.Close();
        }
    }

    public void AddField(string Name, List<Transform> WorkField, List<TargetScript> WorkTargets)
    {
        if(CurrentData == null)
        {
            CurrentData = new NavigationData();
        }

        NavigationFiled field = new NavigationFiled();

        field.Name = Name;

        field.Parts = new List<FieldPart>();

        for (int i = 0; i < WorkField.Count; i++)
        {
            field.Parts.Add(new FieldPart(WorkField[i]));
        }


        field.Targets = new List<NavigationTarget>();
        for (int i = 0; i < WorkTargets.Count; i++)
        {
            field.Targets.Add(WorkTargets[i].GetNavi());
        }
        CurrentData.NavFields.Add(field);
    }

    public void Load()
    {
        if(File.Exists(Folder + "/data.hdt"))
        {
            FileStream fs = new FileStream(Folder + "/data.hdt", FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();
            CurrentData = null;
            CurrentData = new NavigationData();
            try
            {
                CurrentData = (NavigationData)formatter.Deserialize(fs);
            }
            catch(System.Exception e)
            {
                Debug.Log(e.Message);
            }
            finally
            {
                fs.Close();
            }
        }
    }


}
