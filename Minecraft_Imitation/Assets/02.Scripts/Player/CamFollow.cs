using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{

    public Transform myPos;
    CamRotate rotate;
    // Start is called before the first frame update
    void Start()
    {
        rotate = myPos.GetComponent<CamRotate>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = myPos.position;
        transform.eulerAngles = rotate.rotationValue;
    }
}
