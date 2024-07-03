using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryStatic : MonoBehaviour
{
    public static InventoryStatic instance;
    public GameObject[] slots = new GameObject[9]; // 퀵슬롯 배열.
    public GameObject test;
    public GameObject highlight;
    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
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
        if (Input.GetKeyDown(KeyCode.H))
        {
            GameObject testobj = Instantiate(test);
            SetItemPosition(testobj);
            print("h누름");
        }
        Highlight();
    }


    public void SetItemPosition(GameObject item)
    {
        print("아이템포지션 시작");
        for(int i = 0; i < slots.Length; i++)
        {
            if (slots[i].transform.childCount == 4)
            {
                item.transform.parent = slots[i].transform;
                item.transform.localPosition = Vector3.zero;
                item.transform.localScale = Vector3.one;
                print("슬롯 확인 및 포지션 완료.");
                return;
            }
        }
    }

    public void Highlight()
    {
        highlight.transform.localPosition = slots[PlayerManager.instance.usingSlot].transform.localPosition;
    }
}
