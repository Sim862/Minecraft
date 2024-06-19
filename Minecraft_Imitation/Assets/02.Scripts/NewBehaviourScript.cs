using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;

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
        position = new Vector3(0, 0, 0);
        g = new MeshRenderer[i];

        for (int i = 0; i < g.Length; i++)
        {
            if (i % 1000 == 0)
            {
                position = new Vector3(0, position.y + 1, 0);
            }
            position += Vector3.right;
            
            g[i] = Instantiate(gameObject, position, Quaternion.identity, transform);
        }
    }

    public bool check = false;
    // Update is called once per frame
    void Update()
    {
        if (check)
        {
            check = false;
            if(position.z == 0)
            {
                position = new Vector3(0, 0, -30);
            }
            else
            {
                position = Vector3.zero;
            }
            watch.Start();
            for (int i = 0; i < g.Length; i++)
            {
                if (i % 1000 == 0)
                {
                    position = new Vector3(0, position.y + 1, position.z);
                }
                position += Vector3.right;
                
                g[i].transform.position = position;
                g[i].material = material;
            }

            watch.Stop();

            print(watch.ElapsedMilliseconds + "ms");
        }
    }
}
