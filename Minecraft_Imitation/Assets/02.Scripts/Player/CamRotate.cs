using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamRotate : MonoBehaviour
{
    // 회전 속도 변수
    public float rotSpeed = 200f;

    // 회전 값 변수
    float mx = 0;
    float my = 0;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 사용자의 마우스 입력을 받음
        float mouse_X = Input.GetAxisRaw("Mouse X");
        float mouse_Y = Input.GetAxisRaw("Mouse Y");


        // 회전 값 변수에 마우스 입력 값만큼 누적시킨다.
        mx += mouse_X * rotSpeed * Time.deltaTime;
        my += mouse_Y * rotSpeed * Time.deltaTime;

        // 마우스 상하 이동 회전 변수(my)의 값을 -90~90도로 제한
        my = Mathf.Clamp(my, -90f, 90f);

        // 회전 방향으로 물체 회전
        transform.eulerAngles = new Vector3(-my, mx, 0);


        
    }
}
