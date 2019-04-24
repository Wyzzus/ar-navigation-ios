using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AnchorScript : MonoBehaviour {

    public Transform PointsRoot;
    public Transform TargetsRoot;
    public Transform Model;
    public NavMeshSurface NavMeshGen;

	// Use this for initialization
	void Start () {
		
	}

    public void GenerateNavMesh()
    {
        NavMeshGen.BuildNavMesh();
    }
	
	// Update is called once per frame
	void Update () 
    {
        Model.rotation = Quaternion.Slerp(Model.rotation, Camera.main.transform.rotation, .2f);
        Model.localPosition = Vector3.up / NavigationManager.instance.ScaleKoeff;
	}
}
