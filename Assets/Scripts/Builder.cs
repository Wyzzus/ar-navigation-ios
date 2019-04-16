using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Builder : MonoBehaviour
{

    public NavMeshAgent Me;
    public Vector3 destination;
    public Vector3 lastPoint;
    public GameObject PointerPrefab;
    public GameObject FinishPrefab;
    public GameObject FinishInstance;
    public float delta;

    // Use this for initialization
    void Start()
    {
        lastPoint = Camera.main.transform.position;
        transform.parent = NavigationManager.instance.Path.transform;
        FinishInstance = Instantiate<GameObject>(FinishPrefab, NavigationManager.instance.Path.transform);
        FinishInstance.transform.position = destination;
    }

    // Update is called once per frame
    void Update()
    {
        //if (Vector3.Distance(transform.position, Camera.main.transform.position) < 4f)
        {
            Me.destination = destination;
        }
        /*else
        {
            Me.ResetPath();
        }*/

        if(Vector3.Distance(transform.position, lastPoint) > delta)
        {
            GameObject clone = Instantiate<GameObject>(PointerPrefab, transform.position, Quaternion.identity);
            clone.transform.parent = NavigationManager.instance.Path.transform;
            clone.transform.LookAt(lastPoint);
            lastPoint = transform.position;
        }

        if(FinishInstance)
        {
            FinishInstance.transform.rotation = Camera.main.transform.rotation;
        }
		
	}
}
