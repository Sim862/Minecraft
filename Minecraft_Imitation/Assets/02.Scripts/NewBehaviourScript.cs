using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;


public class NewBehaviourScript : MonoBehaviour
{


    // Start is called before the first frame update
    void Start()
    {
        Vector3 a = new Vector3(1, 1, 1);
        Vector3 b = new Vector3(1, 1, 1);

        print(a.Equals(b));
    }

  
}
