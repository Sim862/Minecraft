using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviour
{
    CharacterController cc; // 캐릭터 컨트롤러 변수
    public float moveSpeed = 7f;// 이동 속도 변수
    private float gravity = -6.673f; // 중력 변수
    public float yVelocity = 0f; // 수직 속력 변수
    public float jumpPower = 1.8f; // 점프력 변수
    public bool isJumping = false; // 점프 상태 변수
    public float hp = 20; // 플레이어 체력 변수
    public float maxHp = 20; // 최대 체력 변수
    public Slider hpSlider; // hp 슬라이더 변수
    public Slider hungerSlider; // 허기 슬라이더 변수
    public float maxHunger = 20;
    public float currHunger;
    float orgSpeed;
    PlayerDamaged damagedCs;
    Animator anim;
    Camera cam;
    float runFOV = 55f;
    float walkFOV = 60f;
    bool isRun;
    float hungerTime;
    public float hungerCoolTime = 10f;
    Vector3 dir;
    float orgX;
    float orgY;
    public float healTime = 10f;
    float currHealTime;
    int usingSlot;
    ItemImage itemImage;
    bool invincibility;
    bool isFiring;
    public GameObject arrowFac;
    GameObject arrowGo;
    Arrow arrowCs;
    float loadTime = 0;
    bool onLoad;

    void Start()
    {
        // 캐릭터 컨트롤러 컴포넌트 받아오기.
        cc = GetComponent<CharacterController>();
        damagedCs = gameObject.GetComponent<PlayerDamaged>();
        anim = GetComponent<Animator>();
        PlayerManager.instance.respawnUI.SetActive(false);
        cam = Camera.main;
        currHunger = maxHunger;
        invincibility = false;
    }


    void Update()
    {
        // 4. 현재 플레이어 hp(%)를 hp 슬라이더의 value에 반영한다.
        hpSlider.value = hp / maxHp;
        hungerSlider.value = currHunger / maxHunger;
        usingSlot = PlayerManager.instance.usingSlot;

        if (Input.GetKeyDown(KeyCode.P))
        {
            invincibility = !invincibility;
        }


        if (PlayerManager.instance.playerDead) return;
        if (PlayerManager.instance.onPauseUI) return;
        PlayerMoveMethod();
        FireArrow();
        


        if(hungerTime > hungerCoolTime)
        {
            hungerTime = 0;
            UpdateHunger(-1);
        }
        else if(hungerTime <= hungerCoolTime)
        {
            hungerTime += Time.deltaTime;
        }

        currHealTime += Time.deltaTime;
        if(hp<maxHp && currHunger >= 3 && currHealTime>healTime)
        {
            UpdateHP(1);
            UpdateHunger(-2);
            currHealTime = 0;
        }


        if (Input.GetKeyDown(KeyCode.M))
        {
            UpdateHP(-5);
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isRun = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            isRun = false;
        }

        if (isRun)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, runFOV, 0.05f);
            hungerTime += Time.deltaTime;
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
        dir = new Vector3(h, 0, v);
        dir.Normalize();

        // 2-1. 메인 카메라를 기준으로 방향을 변환한다. -> 캐릭터 기준으로 변경
        dir = transform.TransformDirection(dir);

        // 만약 바닥에 다시 착지했다면 == 땅에 닿고있다면
        if (cc.collisionFlags == CollisionFlags.Below || cc.isGrounded)
        {
            if(yVelocity < -4)
            {
                UpdateHP(-(int)((Mathf.Abs(yVelocity) - 3)));
                if(hp <= 0)
                {
                    PlayerManager.instance.PlayerDead();
                    PlayerManager.instance.playerDead = true;
                    return;
                }
                
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

        if (isRun)
        {
            dir.x *= 1.6f;
            dir.y *= 1.6f;
        }

        // 3. 이동 속도에 맞춰 이동한다.
        cc.Move(dir * moveSpeed * Time.deltaTime);
    }

    void AnimatorControll()
    {
        if (InventoryStatic.instance.slots[usingSlot].transform.childCount == 5)
            itemImage = InventoryStatic.instance.slots[usingSlot].GetComponentInChildren<ItemImage>();
        else if (InventoryStatic.instance.slots[usingSlot].transform.childCount == 4)
            itemImage = null;
        if (Input.GetMouseButtonDown(0))
        {
            if(itemImage != null && itemImage.particleName == ObjectParticleData.ParticleName.Bow)
            {

            }
            else
            {
                anim.SetBool("isAction", true);
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            if(itemImage != null && itemImage.particleType == ObjectParticleData.ParticleType.Food && PlayerManager.instance.canEat)
            {
                anim.SetTrigger("Eat");
            }
            else
            {
                anim.SetBool("isAction", true);
            }
        }
        else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            anim.SetBool("isAction", false);
        }
    }

    public void FireArrow()
    {
        if (PlayerManager.instance.isBow && PlayerManager.instance.canFire)
        {
            if (Input.GetMouseButtonDown(0))
            {
                onLoad = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                onLoad = false;
                arrowGo = Instantiate(arrowFac);
                arrowGo.transform.position = Camera.main.transform.position + Camera.main.transform.forward;
                arrowGo.GetComponent<Arrow>().Fire(Camera.main.transform.forward, loadTime, true);
                if(PlayerManager.instance.arrow != null)
                PlayerManager.instance.arrow.ChangeItemCnt(-1);
                InventoryPopup.instance.CheckQuickSlot();
                loadTime = 0;
            }
        }
        if (onLoad)
        {
            loadTime += 5*Time.deltaTime;
        }
    }


    public void UpdateHP(float dmg)
    {
        if (PlayerManager.instance.playerDead) return;
        if (PlayerManager.instance.onPauseUI) return;
        if (invincibility) return;
        hp += dmg;
        if (hp > 20) hp = 20;
        if(dmg < 0)
        {
            SoundManager.instance.ActiveOnShotSFXSound(Sound.AudioClipName.Player_Hurt, transform, transform.position);
            damagedCs.DamagedEff();
        }
    }

    public void UpdateHunger(float add)
    {
        currHunger += add;
        if(currHunger > 20)
        {
            UpdateHP((currHunger - maxHunger)*0.5f);
            currHunger = maxHunger;
        }
        if(currHunger < 0)
        {
            UpdateHP(-1);
            currHunger = 0;
        }
    }

}
