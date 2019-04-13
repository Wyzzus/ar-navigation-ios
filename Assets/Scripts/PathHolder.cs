using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathHolder : MonoBehaviour {

    public int ID;
    public string Name;
    public Text PathName;
    public MainManager MM;

    public void Setup(string n, int id, MainManager m)
    {
        ID = id;
        Name = n;
        PathName.text = Name;
        MM = m;
    }

    public void Build()
    {
        MM.BuildP(ID);
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () 
    {
		
	}
}
