using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static bool onInventory = false;
    public static PlayerManager instance;
    public GameObject inventory;
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
}
