using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CatchDropEvent : MonoBehaviour, IDropHandler
{
    GameObject item()
    {
        // 슬롯에 아이템이 있다면 아이템의 gameObject 리턴
        if (transform.childCount > 0) return transform.GetChild(0).gameObject;
        else return null;
    }
    public void OnDrop(PointerEventData eventData)
    {
        print("OnDrop 시행됨. 나의 이름 : " + gameObject.name);
        if(item() == null)
        {
            DragItemEvent.item.transform.SetParent(transform);
            DragItemEvent.item.transform.localPosition = Vector3.zero;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
