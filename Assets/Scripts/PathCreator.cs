using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour {

    public GameObject PointPrefab;

    public Transform Root;

    public List<Vector3> Points = new List<Vector3>();

    public Vector3 CamPos;
    public Vector3 LastPoint;

    public bool CanStart = false;

	// Use this for initialization
	void Start () 
    {
		
	}

    public void StartCreating()
    {
        
        CanStart = !CanStart;
        if(CanStart)
            LastPoint = CamPos;
    }
	
	// Update is called once per frame
	void Update () 
    {
        


        if(Root)
        {
            CamPos = Camera.main.transform.position;
            if (CanStart)
            {
                Creating();
            }
        }
        else
        {
            Root = GameObject.Find("RandomCube").transform;
        }
	}

    public void Creating()
    {
        if(Vector3.Distance(CamPos, LastPoint) > 1f)
        {
            Debug.Log("Creating");
            Vector3 pos = new Vector3(CamPos.x, 1, CamPos.z);
            GameObject clone = Instantiate<GameObject>(PointPrefab, Root);

            clone.transform.rotation = Quaternion.Euler(Vector3.zero);
            clone.transform.position = pos;
            Points.Add(clone.transform.position);
            LastPoint = clone.transform.position;
        }
    }
}
