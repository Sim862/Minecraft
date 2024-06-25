using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;

class Test
{
   public Vector3 b;
}
public class NewBehaviourScript : MonoBehaviour
{
    public MeshRenderer gameObject;
    public Material material;
    Stopwatch watch = new Stopwatch();

    public int i = 1000;
    Vector3 position;
    MeshRenderer[] g;


    // Start is called before the first frame update
    void Start()
    {
        Vector3 a = new Vector3(1, 0, 1);
        Test b = new Test();
        b.b = a;
        a += Vector3.left;
        print(b.b);
    }

  
}
