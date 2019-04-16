using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RouteUI : MonoBehaviour {

    public TargetScript target;
    public Text Name;

	// Use this for initialization
	void Start () {
        Name.text = target.Name;
	}
	
	// Update is called once per frame
	void Update () {
        if(NavigationManager.instance.Recording)
        {
            GetComponent<Button>().interactable = false;
        }
        else
        {
            GetComponent<Button>().interactable = true;
        }
	}

    public void SetPath()
    {
        NavigationManager.instance.GetPath(target);
    }
}
