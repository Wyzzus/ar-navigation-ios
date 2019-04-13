using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FieldPart
{
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

    public SerVector3 position;
    public SerVector3 rotation;
    public float shoulderScale;

    public void SetPart()
    {

    }

}

[System.Serializable]
public class NavigationFiled : MonoBehaviour
{
    public List<FieldPart> Parts;

    public void InitializeField()
    {
        Parts = new List<FieldPart>();
    }

    public void SetupField()
    {
        for (int i = 0; i < Parts.Count; i++)
        {
            GameObject part = Instantiate<GameObject>(NavigationManager.instance.GetFieldPart(), NavigationManager.instance.GetAnchor());
            part.transform.localPosition = Parts[i].position.GetRegularVector3();
            part.transform.localRotation = Quaternion.Euler(Parts[i].rotation.GetRegularVector3());
            //part.transform.localScale = Parts[i].scale.GetRegularVector3();
        }
    }
}
