using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoutePart : MonoBehaviour {

    public Transform Shoulder;

    public void SetupPart(float size)
    {
        Shoulder.localScale = new Vector3(Shoulder.localScale.x, Shoulder.localScale.y, size);
    }
}
