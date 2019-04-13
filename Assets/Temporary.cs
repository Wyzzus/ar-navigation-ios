using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class Temporary : MonoBehaviour {

    public GameObject body;
    public NavMeshPath path;
    public GameObject Point;
    public GameObject RoutePart;
    public List<Vector3> Points;
    public List<NavMeshSurface> Surfs;
    public GameObject empty;
    public GameObject currentPath;

    public float DistanceBetweenPoints = 1.5f;

    public bool CanRecord = false;

	// Use this for initialization
	void Start () 
    {
        path = new NavMeshPath();
	}
	
	// Update is called once per frame
	void Update () 
    {
        if(CanRecord)
        {
            Recording();
        }
            
	}

    public void Recording()
    {
        if (Points.Count == 0)
        {
            Points.Add(SetPoint().transform.position);
        }
        else
        {
            int lastIndex = Points.Count - 1;
            if (CanSetPoint())
            {
                Points.Add(SetPoint().transform.position);
            }
        }
    }

    public void StartStopRecord(Text BtnTxt)
    {
        CanRecord = !CanRecord;
        if (CanRecord)
            BtnTxt.text = "Recording";
        else
        {
            BtnTxt.text = "Record";
            ActivateField();
        }
    }

    public GameObject SetPoint()
    {
        GameObject clone = Instantiate<GameObject>(Point, Camera.main.transform.position, Quaternion.identity);
        Vector3 tmp = clone.transform.position;
        clone.transform.position = new Vector3(tmp.x, -0.25f, tmp.z);
        //clone.GetComponent<NavMeshSurface>().BuildNavMesh();
        Surfs.Add(clone.GetComponent<NavMeshSurface>());
        /*foreach(Renderer r in clone.GetComponentsInChildren<Renderer>())
        {
            if(r.gameObject != clone)
            r.enabled = false;
        }*/
        return clone;
    }

    public bool CanSetPoint()
    {
        for (int i = 0; i < Points.Count; i++)
        {
            Vector2 one = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z);
            Vector2 two = new Vector2(Points[i].x, Points[i].z);
            if (Vector2.Distance(one, two) <= DistanceBetweenPoints)
            {
                return false;
            }
        }
        return true;
    }

    public void GetPathTo(Vector3 point)
    {
        NavMesh.CalculatePath(Camera.main.transform.position, point, NavMesh.AllAreas, path);
        Destroy(currentPath);
        currentPath = Instantiate<GameObject>(empty);
        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            SetRoutePart(path.corners[i], path.corners[i + 1]);
        }
        SetRoutePart(path.corners[path.corners.Length - 1], point);
    }

    public void SetRoutePart(Vector3 point, Vector3 next)
    {
        GameObject clone = Instantiate<GameObject>(RoutePart, point, Quaternion.identity);
        clone.transform.parent = currentPath.transform;
        clone.GetComponent<Point>().SetPoint(next);
    }

    public void tmp()
    {
        GetPathTo(new Vector3(0, 0, 0));
    }

    public void ActivateField()
    {
        for (int i = 0; i < Surfs.Count; i++)
        {
            
            foreach (Renderer r in Surfs[i].GetComponentsInChildren<Renderer>())
            {
                if (r.gameObject != Surfs[i].gameObject)
                    r.enabled = true;
            }
            Surfs[i].RemoveData();
        }

        for (int i = 0; i < Surfs.Count; i++)
        {
            Surfs[i].BuildNavMesh();

            foreach (Renderer r in Surfs[i].GetComponentsInChildren<Renderer>())
            {
                if (r.gameObject != Surfs[i].gameObject)
                    r.enabled = false;
            }
        }
    }
}
