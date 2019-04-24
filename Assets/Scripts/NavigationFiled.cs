using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerVector3
{
    public float x;
    public float y;
    public float z;

    public SerVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public SerVector3(Vector3 vec)
    {
        this.x = vec.x;
        this.y = vec.y;
        this.z = vec.z;
    }

    public Vector3 GetRegularVector3()
    {
        return new Vector3(this.x, this.y, this.z);
    }
}

[System.Serializable]
public class FieldPart
{
    

    public SerVector3 position;
    public SerVector3 rotation;
    public float shoulderScale;

    public FieldPart(Transform tr, float size)
    {
        this.position = new SerVector3(tr.localPosition);
        this.rotation = new SerVector3(tr.localEulerAngles);
        this.shoulderScale = size;
    }

    public Quaternion GetRotation()
    {
        Quaternion rot = new Quaternion();

        rot = Quaternion.Euler(rotation.GetRegularVector3());

        return rot;
    }

}

[System.Serializable]
public class NavigationTarget
{
    public string Name;
    public string ImageUrl;
    public string Description;
    public SerVector3 position;
}

[System.Serializable]
public class NavigationFiled
{
    public string Name;
    public List<FieldPart> Parts;
    public List<NavigationTarget> Targets;

    public void InitializeField()
    {
        Parts = new List<FieldPart>();
    }

    /*public void SetupField()
    {
        for (int i = 0; i < Parts.Count; i++)
        {
            GameObject part = Instantiate<GameObject>(NavigationManager.instance.GetFieldPart(), NavigationManager.instance.GetAnchor());
            part.transform.localPosition = Parts[i].position.GetRegularVector3();
            part.transform.localRotation = Quaternion.Euler(Parts[i].rotation.GetRegularVector3());
            part.transform.localScale = Vector3.forward * Parts[i].shoulderScale;
        }
    }*/
}

[System.Serializable]
public class NavigationData
{
    public List<NavigationFiled> NavFields; 
}
