using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartScript : MonoBehaviour
{
    public Transform Shoulder;

	public void SetupPart(float size)
    {
        Shoulder.localScale = new Vector3(1, 1, size / NavigationManager.instance.ScaleKoeff );
        /*foreach(MeshRenderer mt in GetComponentsInChildren<MeshRenderer>())
        {
            mt.enabled = true;
        }*/
    }

}
