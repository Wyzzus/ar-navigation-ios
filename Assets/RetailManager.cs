using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RetailManager : MonoBehaviour {

    public Transform Agent;
    public GameObject RoutePart;
	
    public void SetPart(Vector3 from, Vector3 to)
    {
        GameObject part = Instantiate<GameObject>(RoutePart, from, Quaternion.identity);
    }

}
