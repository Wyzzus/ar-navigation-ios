using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class ServicePanelPart : MonoBehaviour {

    public InputField Name;
    public InputField Description;
    public InputField URL;

    public TargetScript targetScript;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Load()
    {
        this.Name.text = targetScript.Name;
        this.Description.text = targetScript.Description;
        this.URL.text = targetScript.ImageUrl;
    }

    public void Delete()
    {
        NavigationManager.instance.WorkTargets.Remove(targetScript);
        Destroy(targetScript.gameObject);
        Destroy(gameObject);

    }

    public void Save()
    {
        targetScript.Name = this.Name.text;
        targetScript.Description = this.Description.text;
        targetScript.ImageUrl = this.URL.text;
        targetScript.SetupTarget();
    }
}
