using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static bool onInventory = false;
    public static PlayerManager instance;
    public GameObject inventory;
    public int usingSlot = 0; // 사용중인 퀵슬롯넘버
    CamRotate camRotate;
    PlayerRotate playerRotate;
    float orgCamRotSpeed;
    float orgPlayerRotSpeed;
    bool cursorLock = true;
    // Start is called before the first frame update
    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            onInventory = !onInventory;
        }
        OnOffInventory();
        CursurLockMethod();
        NowUsingSlotNumber();
    }

    void OnOffInventory()
    {
        if (onInventory)
        {
            inventory.SetActive(true);
            cursorLock = false;
        }
        else
        {
            inventory.SetActive(false);
            cursorLock = true;
        }
    }

    void CursurLockMethod()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            cursorLock = !cursorLock;
        }
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
    }
}
