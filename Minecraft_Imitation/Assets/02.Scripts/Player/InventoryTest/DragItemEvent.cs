using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragItemEvent : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // itemImage 오브젝트에 연결?
    public static GameObject item;
    GameObject startParent;
    Transform dragingParent;
    Transform startPos;

    GameObject slot;

    public void OnBeginDrag(PointerEventData eventData)
    {
        item = gameObject;
        startParent = transform.parent.gameObject;
        transform.SetParent(transform.parent.parent.parent);
        print("Begin : " + transform.parent.name);
    }

    public void OnDrag(PointerEventData eventData)
    {
        print("OnDrag : " +transform.parent.name);
        print(eventData.pointerCurrentRaycast.gameObject.name);
        
        if (eventData.pointerCurrentRaycast.gameObject.layer == LayerMask.NameToLayer("Slot"))
        {
            slot = eventData.pointerCurrentRaycast.gameObject;
        }
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject.layer == LayerMask.NameToLayer("Slot"))
        {
            slot = eventData.pointerCurrentRaycast.gameObject;
        }
        if (slot != null)
        {
            transform.parent = slot.transform;
        }
        else
        {
            transform.parent = startParent.transform;

        }
        transform.localPosition = Vector3.zero;
        //GetComponent<CanvasGroup>().blocksRaycasts = true;
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
