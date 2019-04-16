using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetScript : MonoBehaviour
{
    public Vector3 position;
    public string Name;
    public string ImageUrl;
    public string Description;

    public NavigationTarget GetNavi()
    {
        NavigationTarget navi = new NavigationTarget();

        navi.Name = this.Name;
        navi.Description = this.Description;
        navi.ImageUrl = this.ImageUrl;
        navi.position = new SerVector3(this.position);

        return navi;
    }

    public void FromNavi(NavigationTarget target)
    {
        this.Name = target.Name;
        this.Description = target.Description;
        this.ImageUrl = target.ImageUrl;
        this.position = target.position.GetRegularVector3();
    }

}
