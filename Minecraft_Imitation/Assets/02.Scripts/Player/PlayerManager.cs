using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public static bool onInventory = false;
    public static PlayerManager instance;
    public GameObject respawnUI;
    public GameObject inventory;
    public GameObject pauseUI;
    public bool onPauseUI;
    public float aimRange;
    public int usingSlot = 0; // 사용중인 퀵슬롯넘버
    public int remainder = 0;
    CamRotate camRotate;
    PlayerRotate playerRotate;
    PlayerMove playerMove;
    float orgCamRotSpeed;
    float orgPlayerRotSpeed;
    bool cursorLock = true;
    bool canGetItem;
    public bool canEat;
    public bool playerDead;
    public GameObject player;

    public GameObject hand;
    public GameObject pickPos;

    public ItemImage arrow;
    public bool isBow;
    public bool canFire;
    int previous;
    int now;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player");
        playerMove = player.GetComponent<PlayerMove>();
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        now = usingSlot;
        onPauseUI = false;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            PlayerRespawn();
        }

        if (pauseUI.activeSelf || inventory.activeSelf || playerDead)
        {
            cursorLock = false;
            onInventory = true;
        }
        else if (!pauseUI.activeSelf || !inventory.activeSelf || !playerDead)
        {
            cursorLock = true;
            onInventory = false;
        }
        CursurLockMethod();
        if (PlayerManager.instance.playerDead)
        {
            return;
        }
        OnOffPauseUI();
        CheckCanFire();
        if (playerMove.currHunger < playerMove.maxHunger) canEat = true;
        else if (playerMove.currHunger >= playerMove.maxHunger) canEat = false;

        

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            InventoryPopup.instance.useMaker = false;
            OnOffInventory();
        }
        if (!onInventory)
        {
            NowUsingSlotNumber();
        }
        if (InventoryStatic.instance.slots[usingSlot].transform.childCount == 4)
        {
            hand.SetActive(true);
            pickPos.SetActive(false);
            isBow = false;
        }
        else
        {
            hand.SetActive(false);
            pickPos.SetActive(true);
            if(InventoryStatic.instance.slots[usingSlot].transform.GetChild(4).GetComponent<ItemImage>() != null)
            {
                if (InventoryStatic.instance.slots[usingSlot].transform.GetChild(4).GetComponent<ItemImage>().particleName == ObjectParticleData.ParticleName.Bow)
                {
                    isBow = true;
                }
                else
                {
                    isBow = false;
                }
            }
            
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (inventory.activeSelf)
            {
                OnOffInventory();
            }
            else
            {
                ChangeBoolPause();
            }
        }
        


    }

    public void ChangeBoolPause()
    {
        onPauseUI = !onPauseUI;
    }

    public void OnOffPauseUI()
    {
        if (onPauseUI)
        {
            pauseUI.SetActive(true);
        }
        else if (!onPauseUI)
        {
            pauseUI.SetActive(false);
        }
    }

    public void OnOffInventory()
    {
        if (onPauseUI) return;
        if (inventory.activeSelf)
        {
            inventory.SetActive(false);
        }
        else if (!inventory.activeSelf)
        {
            inventory.SetActive(true);
        }
    }

    void CursurLockMethod()
    {
        if (cursorLock)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if(!cursorLock)
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    void NowUsingSlotNumber() // 숫자키 누르면 해당 슬롯 누르기.
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) usingSlot = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) usingSlot = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) usingSlot = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4)) usingSlot = 3;
        else if (Input.GetKeyDown(KeyCode.Alpha5)) usingSlot = 4;
        else if (Input.GetKeyDown(KeyCode.Alpha6)) usingSlot = 5;
        else if (Input.GetKeyDown(KeyCode.Alpha7)) usingSlot = 6;
        else if (Input.GetKeyDown(KeyCode.Alpha8)) usingSlot = 7;
        else if (Input.GetKeyDown(KeyCode.Alpha9)) usingSlot = 8;

        if(Input.GetAxisRaw("Mouse ScrollWheel") < 0)
        {
            usingSlot++;
            if(usingSlot > 8)
            {
                usingSlot = 0;
            }
        }
        else if(Input.GetAxisRaw("Mouse ScrollWheel") > 0)
        {
            usingSlot--;
            if (usingSlot < 0)
            {
                usingSlot = 8;
            }
        }
        previous = now;
        now = usingSlot;
        InventoryStatic.instance.CheckIsUsing(previous, false);
        InventoryStatic.instance.CheckIsUsing(now, true);
    }

    public bool CheckGetItem() // 용도 : 인벤토리에 들어갈 수 있으면true.
    {
        if (canGetItem)
        {
            return true;
        }
        else
        {
            remainder = InventoryStatic.instance.exceededCnt;
            return false;
        }

    }

    public void PlayerDead()
    {
        respawnUI.SetActive(true);
        playerDead = true;

        
        foreach(GameObject go in InventoryPopup.instance.quickSlot)
        {
            if(go.transform.childCount != 0)
            {
                Destroy(go.transform.GetChild(0).gameObject);
            }
        }
        foreach(GameObject go in InventoryPopup.instance.inven)
        {
            if (go.transform.childCount != 0)
            {
                Destroy(go.transform.GetChild(0).gameObject);
            }
        }
        foreach(GameObject go in InventoryStatic.instance.slots)
        {
            if(go.transform.childCount > 4)
            {
                Destroy(go.transform.GetChild(4).gameObject);
            }
        }


        InventoryPopup.instance.gameObject.SetActive(false);
    }

    public void PlayerRespawn()
    {
        playerMove.hp = playerMove.maxHp;
        playerDead = false;
        respawnUI.SetActive(false);

    }

    void CheckCanFire()
    {
        for (int i = 0; i < InventoryStatic.instance.slots.Length; i++)
        {
            if (InventoryStatic.instance.slots[i].transform.childCount == 4) continue;
            else
            {
                if(InventoryStatic.instance.slots[i].transform.GetChild(4).GetComponent<ItemImage>().particleName == ObjectParticleData.ParticleName.Arrow)
                {
                    arrow = InventoryStatic.instance.slots[i].transform.GetChild(4).GetComponent<ItemImage>();
                    canFire = true;
                    return;
                }
            }
        }
        for(int i = 0; i < InventoryPopup.instance.inven.Length; i++)
        {
            if (InventoryPopup.instance.inven[i].transform.childCount == 0) continue;
            else
            {
                if (InventoryPopup.instance.inven[i].transform.GetChild(0).GetComponent<ItemImage>().particleName == ObjectParticleData.ParticleName.Arrow)
                {
                    arrow = InventoryPopup.instance.inven[i].transform.GetChild(0).GetComponent<ItemImage>();
                    canFire = true;
                    return;
                }
            }
        }
        
        canFire = false;
    }


    public void GoToTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
