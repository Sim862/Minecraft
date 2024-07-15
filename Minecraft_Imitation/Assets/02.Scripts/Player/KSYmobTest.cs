using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KSYmobTest : MonoBehaviour
{
    float maxHp = 10;
    float curhp;
    GameObject target;
    Vector3 dir;
    float moveSpeed = 5;

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        dir = target.transform.position - transform.position;
        dir.Normalize();
        transform.Translate(dir * moveSpeed * Time.deltaTime);
        transform.LookAt(dir);
    }
}
