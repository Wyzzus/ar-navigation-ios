using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopObject : MonoBehaviour {
    public RetailObject retailObject;
    public string description;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Setup()
    {
        if(retailObject != null)
        {
            transform.position = retailObject.GetPosition();
            description = retailObject.GetDescription();
        }
    }

    public void Create()
    {
        retailObject = new RetailObject(this);
    }
}
