using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviour
{
    CharacterController cc; // 캐릭터 컨트롤러 변수
    public float moveSpeed = 7f;// 이동 속도 변수
    float gravity = -20f; // 중력 변수
    public float yVelocity = 0f; // 수직 속력 변수
    public float jumpPower = 10f; // 점프력 변수
    public bool isJumping = false; // 점프 상태 변수
    public float hp = 20; // 플레이어 체력 변수
    public float maxHp = 20; // 최대 체력 변수
    public Slider hpSlider; // hp 슬라이더 변수
    float orgSpeed;
    PlayerDamaged damagedCs;
    Animator anim;
    Camera cam;
    float runFOV = 55f;
    float walkFOV = 60f;
    bool isRun;

    void Start()
    {
        // 캐릭터 컨트롤러 컴포넌트 받아오기.
        cc = GetComponent<CharacterController>();
        damagedCs = gameObject.GetComponent<PlayerDamaged>();
        anim = GetComponent<Animator>();
        PlayerManager.instance.respawnUI.SetActive(false);
        cam = Camera.main;
    }


    void Update()
    {
        // 4. 현재 플레이어 hp(%)를 hp 슬라이더의 value에 반영한다.
        hpSlider.value = hp / maxHp;

        if (PlayerManager.instance.playerDead) return;
        PlayerMoveMethod();

        if (Input.GetKeyDown(KeyCode.M))
        {
            UpdateHP(-5);
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            orgSpeed = moveSpeed;
            moveSpeed *= 1.6f;
            isRun = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            moveSpeed = orgSpeed;
            isRun = false;
            
        }

        if (isRun)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, runFOV, 0.05f);
        }
        else if (!isRun)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, walkFOV, 0.05f);
        }

        if (hp <= 0)
        {
            PlayerManager.instance.playerDead = true;
            if(PlayerManager.instance.playerDead) PlayerManager.instance.PlayerDead();
            return;
        }
        if (!PlayerManager.onInventory) // 인벤토리가 켜져있으면 안되게
        {
            AnimatorControll();
        }

        
        MapManager.instance.playerPositionData = MapManager.instance.PositionToBlockData(transform.position);
    }

    void PlayerMoveMethod()
    {
        // 1. 사용자의 입력을 받는다.
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 2. 이동 방향을 설정한다.
        Vector3 dir = new Vector3(h, 0, v);
        dir = dir.normalized;

        // 2-1. 메인 카메라를 기준으로 방향을 변환한다. -> 캐릭터 기준으로 변경
        dir = transform.TransformDirection(dir);

        // 만약 바닥에 다시 착지했다면 == 땅에 닿고있다면
        if (cc.collisionFlags == CollisionFlags.Below || cc.isGrounded)
        {
            if(yVelocity < -10)
            {
                hp -= (int)((Mathf.Abs(yVelocity) - 5));
                if(hp <= 0)
                {
                    PlayerManager.instance.PlayerDead();
                    PlayerManager.instance.playerDead = true;
                    return;
                }
                damagedCs.DamagedEff();
            }
            // 만약 점프 중이었다면
            if (isJumping)
            {
                // 점프 전 상태로 초기화
                isJumping = false;
            }
            // 캐릭터 수직 속도를 0으로 만든다.
            yVelocity = 0;
        }
        else
        {
            yVelocity += gravity * Time.deltaTime;
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
        dir.y = yVelocity;


        // 3. 이동 속도에 맞춰 이동한다.
        cc.Move(dir * moveSpeed * Time.deltaTime);
    }

    void AnimatorControll()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            anim.SetBool("isAction", true);
        }
        else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            anim.SetBool("isAction", false);
        }
    }


    public void UpdateHP(float dmg)
    {
        hp += dmg;
    }

}
