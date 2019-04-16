using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class TargetScript : MonoBehaviour
{
    public Vector3 position;
    public string Name;
    public string ImageUrl;
    public string Description;

    public Text NameDisplay;
    public Text DescriptionDisplay;
    public Image ImageDisplay;

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

    public void SetupTarget()
    {
        LoadImage();
        NameDisplay.text = Name;
        DescriptionDisplay.text = Description;
    }

	public void Update()
	{
		
	}

    public void LoadImage()
    {
        StartCoroutine(SetImage(ImageUrl)); //balanced parens CAS
    }

    IEnumerator SetImage(string url)
    {
        WWW www = new WWW(url);
        yield return www;
        ImageDisplay.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));


        if (www.texture.width < www.texture.height)
        {
            float ratio = www.texture.width * 1.0f / www.texture.height * 1.0f;
            ImageDisplay.rectTransform.sizeDelta = new Vector2(ratio, 1);
        }
        else
        {
            float ratio = www.texture.height * 1.0f / www.texture.width * 1.0f;
            ImageDisplay.rectTransform.sizeDelta = new Vector2(1, ratio);
        }



    }

}
