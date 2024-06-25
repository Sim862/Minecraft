using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    CharacterController cc; // 캐릭터 컨트롤러 변수
    public float moveSpeed = 7f;// 이동 속도 변수
    float gravity = -20f; // 중력 변수
    public float yVelocity = 0f; // 수직 속력 변수
    public float jumpPower = 10f; // 점프력 변수
    public bool isJumping = false; // 점프 상태 변수



    void Start()
    {
        // 캐릭터 컨트롤러 컴포넌트 받아오기.
        cc = GetComponent<CharacterController>();
    }


    void Update()
    {
        // 1. 사용자의 입력을 받는다.
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // 2. 이동 방향을 설정한다.
        Vector3 dir = new Vector3(h, 0, v);
        dir = dir.normalized;

        // 2-1. 메인 카메라를 기준으로 방향을 변환한다. -> 캐릭터 기준으로 변경
        dir = transform.TransformDirection(dir);

        // 만약 바닥에 다시 착지했다면
        if(cc.collisionFlags == CollisionFlags.Below)
        {
            // 만약 점프 중이었다면
            if (isJumping)
            {
                // 점프 전 상태로 초기화
                isJumping = false;
                // 캐릭터 수직 속도를 0으로 만든다.
                yVelocity = 0;
            }
        }

        // 2-2. 만약 키보드 spacebar 키를 눌렀다면, 그리고 점프상태가 아니라면
        if (Input.GetButtonDown("Jump") && !isJumping)
        {
            // 캐릭터 수직 속도에 점프력을 적용한다.
            yVelocity = jumpPower;
            // 점프중인 상태로 변경한다.
            isJumping = true;
        }

        // 2-3. 캐릭터 수직 속도에 중력 값을 적용한다.
        yVelocity += gravity * Time.deltaTime;
        dir.y = yVelocity;


        // 3. 이동 속도에 맞춰 이동한다.
        //transform.position += dir * moveSpeed * Time.deltaTime;
        cc.Move(dir * moveSpeed * Time.deltaTime);
    }
}
