using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point : MonoBehaviour {

    public Transform Leg;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetPoint(Vector3 to)
    {
        Leg.LookAt(to);
        Leg.localScale = new Vector3(1, 1, Vector3.Distance(transform.position, to));
    }
}
