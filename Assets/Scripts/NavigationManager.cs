using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NavigationManager : MonoBehaviour
{

    #region Singleton
    public static NavigationManager instance;

    public void Awake()
    {
        instance = this;
    }

    #endregion

    public NavigationFiled LoadedField;

    public List<Transform> WorkField;

    public GameObject FieldPartPrefab;
    public Transform Anchor;
    public Transform Agent;

    public bool CanRecord;

    public float DistanceOfSetup = 0.2f;
    public float MaxDistance = 2f;

    public GameObject GetFieldPart()
    {
        return FieldPartPrefab;
    }
    public Transform GetAnchor()
    {
        return Anchor;
    }

    public void Update()
    {
        if(CanRecord)
            Recording();

        if (GameObject.FindGameObjectWithTag("Anchor"))
            GameObject.Find("Text").GetComponent<Text>().text = "Anchor found!";
        else
            GameObject.Find("Text").GetComponent<Text>().text = "Anchor not found!";
    }

    public void SetupField()
    {
        LoadedField.SetupField();
    }

    public void Recording()
    {
        if (WorkField.Count != 0)
        {
            Transform NearestPart = GetNearestPart();
            float dist = Vector3.Distance(NearestPart.position, Agent.position);
            if (dist >= DistanceOfSetup)
            {
                SetPartOn(Agent.position, NearestPart.position);
            }
        }
        else
        {
            SetPartOn(Agent.position, Agent.position - Vector3.forward/2);
        }
    }

    public Transform GetNearestPart()
    {
        int Nearest = 0;
        float minDist = float.MaxValue;
        for (int i = 1; i < WorkField.Count; i++)
        {
            float dist = Vector3.Distance(WorkField[i].position, Agent.position);
            if (dist < minDist)
            {
                minDist = dist;
                Nearest = i;
            }
        }

        return WorkField[Nearest];
    }

    public void SetPartOn(Vector3 position, Vector3 to)
    {
        GameObject clone = Instantiate<GameObject>(FieldPartPrefab, position, Quaternion.identity);
        clone.transform.LookAt(to);
        clone.transform.localScale = new Vector3(1, 1, Vector3.Distance(position, to));
        WorkField.Add(clone.transform);
    }
}
