using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Transform DirectionalLight;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        DirectionalLight.Rotate(Vector3.right * Time.deltaTime * 3);
    }
}
