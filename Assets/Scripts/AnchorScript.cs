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
        Model.localRotation = Quaternion.Slerp(Model.localRotation, Camera.main.transform.rotation, .2f);
	}
}
