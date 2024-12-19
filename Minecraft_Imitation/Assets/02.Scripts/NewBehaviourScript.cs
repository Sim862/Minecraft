using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;


public class NewBehaviourScript : MonoBehaviour
{

    public Transform target;
    public float y  = 30;
    private void Update()
    {
        transform.position = new Vector3(target.position.x, y, target.position.z);
    }



}
