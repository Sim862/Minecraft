using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragItemEvent : MonoBehaviour//, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static GameObject dragingItem; // 드래그 중인 아이템
    public static ItemImage dragItemImage; // 드래그 중인 아이템의 ItemImage Script

    Transform orgParent;
    Transform dragingParent;
    Transform startPos;
    GameObject nowMakeSlots;
    MakingSlot makingSlot;


    #region 마우스 관련 변수
    EventSystem eventSystem;
    GraphicRaycaster graphicRaycaster;
    Canvas canvas;
    PointerEventData pointerEventData;
    RaycastResult raycastResult;
    #endregion

    ItemImage myItemImage;
    public static bool isDraging;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
        eventSystem = EventSystem.current;
        pointerEventData = new PointerEventData(eventSystem);
        myItemImage = GetComponent<ItemImage>();
    }
    private void Update()
    {
        return;
        if (!PlayerManager.onInventory) return; // 인벤토리가 꺼져있으면 바로 탈출
        // 클릭했을 때 현재 마우스가 가리키고 있는게 뭔지 체크.
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            CheckLayer();
            if (raycastResult.gameObject.layer != LayerMask.NameToLayer("ItemImage") && raycastResult.gameObject.layer != LayerMask.NameToLayer("Slot"))
            {
                print("ItemImage, Slot 둘 다 아님");
                return;
            }
            print("현재 가리키고 있는 게임의 layer는 : " + raycastResult.gameObject.layer);
        }

        print(gameObject.transform.parent + " / "+myItemImage.count);
        
        // 처음 좌클릭시 마우스 따라가기.
        if(raycastResult.gameObject != null && raycastResult.gameObject.layer == LayerMask.NameToLayer("ItemImage") && !isDraging)
        {
            if (Input.GetMouseButtonDown(0) && !isDraging)
            {
                isDraging = true;
                print(isDraging);
                dragingItem = gameObject;
                orgParent = transform.parent;
                dragItemImage = GetComponent<ItemImage>();
                transform.SetParent(transform.parent.parent.parent.parent);
                GetComponentInChildren<Image>().raycastTarget = false;
                GetComponentInChildren<TextMeshProUGUI>().raycastTarget = false;
                nowMakeSlots = InventoryPopup.instance.nowMakeSlots.transform.GetChild(1).gameObject;
                makingSlot = nowMakeSlots.GetComponent<MakingSlot>();
                makingSlot.SaveAllData();
                if (dragItemImage.wasInTakeSlot)
                {
                    DecreaseInMakeSlot();
                    dragItemImage.wasInTakeSlot = false;
                }
            }
            // 처음 우클릭시 
            else if (Input.GetMouseButtonDown(1) && !isDraging)
            {
                // 개수는 반개만 해서 
                // 마우스 따라가게
                print("우클릭");
                isDraging = true;
                GameObject halfDragingItem = Instantiate(gameObject, canvas.transform); // 새로운 이미지 생성.
                myItemImage.ChangeItemCnt(-(myItemImage.count / 2));
                dragingItem = halfDragingItem;
                dragItemImage = halfDragingItem.GetComponent<ItemImage>();
                dragItemImage.ChangeItemCnt(-(dragItemImage.count / 2));
                Destroy(gameObject);

            }
        }
        

        // 이미지가 마우스 따라다니고 있을 때
        if (isDraging)
        {
            dragingItem.transform.position = Input.mousePosition;
            if (Input.GetMouseButtonDown(0))// 다시 좌클릭시 슬롯이 있으면
            {
                // 그 자리 들어가고
                // 슬롯이 아니라 이미지가 있으면
                // 이미지 검사후 
                // 이미지가 같은 거면 숫자 체크
                // 이미지가 다르면 그 자리 들어가고
                // 그 자리 있던 이미지를 다시 마우스 따라가게


            }
            else if (Input.GetMouseButtonDown(1))
            {
                // 이미지가 마우스 따라다니고 있을 때
                // 다시 우클릭시 슬롯에 이미지가 있을 때
                // 이미지가 같으면 한개만 그 이미지에게 주고
                // 이미지가 다르면 전체 바꿈.
                // 슬롯에 이미지가 없으면 그 슬롯에 한개만 줌.
            }
        }
        
        

        

    }

    void CheckLayer()
    {
        List<RaycastResult> results = new List<RaycastResult>();
        pointerEventData.position = Input.mousePosition;
        print(pointerEventData);
        graphicRaycaster.Raycast(pointerEventData, results);
        raycastResult = results[0];
        results.RemoveAt(0);
        
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        return;
        dragingItem = gameObject;
        orgParent = transform.parent;
        dragItemImage = GetComponent<ItemImage>();
        transform.SetParent(transform.parent.parent.parent.parent);
        GetComponentInChildren<Image>().raycastTarget = false;
        GetComponentInChildren<TextMeshProUGUI>().raycastTarget = false;
        nowMakeSlots = InventoryPopup.instance.nowMakeSlots.transform.GetChild(1).gameObject;
        makingSlot = nowMakeSlots.GetComponent<MakingSlot>();
        makingSlot.SaveAllData();
        if (dragItemImage.wasInTakeSlot)
        {
            DecreaseInMakeSlot();
            dragItemImage.wasInTakeSlot = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        return;
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        return;
        if (eventData.pointerCurrentRaycast.gameObject.layer != LayerMask.NameToLayer("Slot") && eventData.pointerCurrentRaycast.gameObject.layer != LayerMask.NameToLayer("ItemImage"))
        {
            transform.SetParent(orgParent);
            transform.localPosition = Vector3.zero;
        }
        GetComponentInChildren<Image>().raycastTarget = true;
        GetComponentInChildren<TextMeshProUGUI>().raycastTarget = true;
        InventoryPopup.instance.CheckQuickSlot();
    }


    public void SwitchPos(ItemImage previous) // 이미지아이템 위치 바꾸기.
    {
        previous.gameObject.transform.SetParent(orgParent);
        previous.gameObject.transform.localPosition = Vector3.zero;
    }

    public Transform SwitchPos()
    {
        return orgParent;
    }

    void DecreaseInMakeSlot()
    {
        for(int i = 0; i < makingSlot.dropSlot.Length; i++)
        {
            ItemImage item = null;
            if (makingSlot.dropSlot[i] == null)
            {
                continue;
            }
            if (makingSlot.dropSlot[i].transform.childCount != 0)
            {
                item = makingSlot.dropSlot[i].GetComponentInChildren<ItemImage>();
                item.ChangeItemCnt(-1);
            }
        }
    }

}
