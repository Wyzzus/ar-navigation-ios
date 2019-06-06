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

    public GameObject[] Models;
    public GameObject Left;
    public GameObject Right;

    public GameObject[] Banners;

    public float DistanceToHide = 4f;

    public Transform Body;
    public Transform Canv;

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
        if(ImageUrl.Length > 0)
        {
            LoadImage();
        }
        NameDisplay.text = Name;
        DescriptionDisplay.text = Description;

    }

	public void Update()
	{
        if (Vector3.Distance(Camera.main.transform.position, transform.position) < DistanceToHide)
        {
            Body.transform.localScale = Vector3.one;
        }
        else
            Body.transform.localScale = Vector3.zero;

        if (ImageUrl.Length == 0)
            ImageDisplay.rectTransform.sizeDelta = Vector2.zero;
        else
            ImageDisplay.rectTransform.sizeDelta = Vector2.one;

        Canv.transform.rotation = Camera.main.transform.rotation;
        SetupModel();
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

    public void SetupModel()
    {
        switch(Name)
        {
            case "Винный отдел":
                ActivateModel(-1);
                Right.SetActive(false);
                Left.SetActive(true);
                break;
            case "Рыбный отдел":
                ActivateModel(-1);
                Right.SetActive(true);
                Left.SetActive(false);
                break;
            case "Фрукты/Овощи":
                ActivateModel(-1);
                Right.SetActive(true);
                Left.SetActive(false);
                break;
            case "1":
                ActivateModel(0);
                Right.SetActive(false);
                Left.SetActive(false);
                DescriptionDisplay.transform.parent.gameObject.SetActive(false);
                Banners[0].SetActive(true);
                Banners[1].SetActive(false);
                break;
            case "2":
                ActivateModel(1);
                Right.SetActive(false);
                Left.SetActive(false);
                DescriptionDisplay.transform.parent.gameObject.SetActive(false);
                Banners[0].SetActive(false);
                Banners[1].SetActive(true);
                break;
            case "3":
                ActivateModel(2);
                Right.SetActive(false);
                Left.SetActive(false);
                DescriptionDisplay.transform.parent.gameObject.SetActive(false);
                break;
            default:
                ActivateModel(-1);
                Right.SetActive(false);
                Left.SetActive(false);
                Banners[0].SetActive(false);
                Banners[1].SetActive(false);
                break;
        }
    }

    public void ActivateModel(int n)
    {
        for (int i = 0; i < Models.Length; i++)
        {
            if (i == n)
                Models[n].SetActive(true);
            else
                Models[i].SetActive(false);
        }
    }

}
