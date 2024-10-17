using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryPopup : MonoBehaviour
{
    public static InventoryPopup instance;
    public GameObject[] quickSlot = new GameObject[0];
    public GameObject[] inven = new GameObject[0];
    public GameObject withMaker;
    public GameObject withoutMaker;
    public GameObject nowMakeSlots;
    public bool useMaker;
    int usingSlot;
    GameObject staticGo;
    GameObject quickGo;
    Transform staticItem;
    Transform quickItem;

    #region 마우스 관련 변수
    EventSystem eventSystem;
    GraphicRaycaster graphicRaycaster;
    Canvas canvas;
    PointerEventData pointerEventData;
    RaycastResult raycastResult;
    #endregion

    public static GameObject dragingItem; // 드래그 중인 아이템
    public static ItemImage dragItemImage; // 드래그 중인 아이템의 ItemImage Script
    public static bool isDraging;

    ItemImage previousItemImage;
    GameObject newItem;
    ItemImage newItemImage;
    Transform orgParent;
    Transform dragingParent;
    Transform startPos;
    MakingSlot makingSlot;
    GameObject rayObject;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        gameObject.SetActive(false);
        canvas = GetComponentInParent<Canvas>();
        graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
        eventSystem = EventSystem.current;
        pointerEventData = new PointerEventData(eventSystem);
    }
    

    private void Update()
    {
        CheckUseMaker();
        makingSlot = nowMakeSlots.transform.GetChild(1).GetComponent<MakingSlot>();
        makingSlot.SaveAllData();
        if (!PlayerManager.onInventory) return; // 인벤토리가 꺼져있으면 바로 탈출
        if (PlayerManager.onInventory)
        {
            foreach(GameObject go in quickSlot)
            {
                if(go.transform.childCount != 0)
                {
                    go.GetComponent<Image>().raycastTarget = false;
                }
                else
                {
                    go.GetComponent<Image>().raycastTarget = true;
                }
            }
            foreach (GameObject go in inven)
            {
                if (go.transform.childCount != 0)
                {
                    go.GetComponent<Image>().raycastTarget = false;
                }
                else
                {
                    go.GetComponent<Image>().raycastTarget = true;
                }
            }
            CheckQuickSlot();
        } 
        // 클릭했을 때 현재 마우스가 가리키고 있는게 뭔지 체크.
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            CheckLayer();
            
            if (rayObject.layer != LayerMask.NameToLayer("ItemImage") 
                && rayObject.layer != LayerMask.NameToLayer("Slot"))
            {
                return;
            }
            if(rayObject.layer == LayerMask.NameToLayer("ItemImage"))
            {
                previousItemImage = rayObject.GetComponent<ItemImage>();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (isDraging && dragingItem != null)
            {
                if (rayObject.layer == LayerMask.NameToLayer("Slot")) //슬롯이 있으면
                {
                    // 그 자리 들어가고
                    dragingItem.transform.SetParent(rayObject.transform);
                    dragingItem.transform.localPosition = Vector3.zero;
                    dragingItem.GetComponent<Image>().raycastTarget = true;
                    dragItemImage = null;
                    dragingItem = null;
                    isDraging = false;
                }
                else if (rayObject.layer == LayerMask.NameToLayer("ItemImage")) // 슬롯이 아니라 이미지가 있으면
                {
                    previousItemImage = rayObject.GetComponent<ItemImage>();
                    if (dragItemImage.particleName != previousItemImage.particleName)// 이미지가 다르면
                    {
                        // 그 자리 들어가고
                        dragingItem.transform.SetParent(rayObject.transform.parent);
                        dragingItem.transform.localPosition = Vector3.zero;
                        dragingItem.GetComponent<Image>().raycastTarget = true;
                        dragingItem = previousItemImage.gameObject;
                        // 그 자리 있던 이미지가 따라가게
                        dragItemImage = previousItemImage;
                        dragingItem.transform.SetParent(transform);
                        dragingItem.GetComponent<Image>().raycastTarget = false;
                    }
                    else if (dragItemImage.particleName == previousItemImage.particleName)// 이미지 검사후
                    {
                        
                        if (previousItemImage.wasInTakeSlot)
                        {
                            TransferCnt(dragItemImage, previousItemImage, true);
                            DecreaseInMakeSlot();
                        }
                        else if (!previousItemImage.wasInTakeSlot)
                        {
                            // 이미지가 같은 거면 숫자 체크
                            TransferCnt(previousItemImage, dragItemImage, true);

                        }
                        
                    }
                }
            }
            else if (!isDraging && rayObject.layer == LayerMask.NameToLayer("ItemImage"))
            {
                isDraging = true;
                dragingItem = rayObject;
                dragItemImage = dragingItem.GetComponent<ItemImage>();
                dragingItem.transform.SetParent(transform);
                dragingItem.GetComponent<Image>().raycastTarget = false;
                if (dragItemImage.wasInTakeSlot)
                {
                    DecreaseInMakeSlot();
                    dragItemImage.wasInTakeSlot = false;
                }
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            if (isDraging && dragingItem != null)
            {
                // 이미지가 마우스 따라다니고 있을 때
                if (rayObject.layer == LayerMask.NameToLayer("Slot")) // 슬롯에 이미지가 없으면
                {
                    // 그 슬롯에 한개만 줌.
                    newItem = Instantiate(dragingItem, rayObject.transform);
                    newItem.transform.localPosition = Vector3.zero;
                    newItem.GetComponent<Image>().raycastTarget = true;
                    newItemImage = newItem.GetComponent<ItemImage>();
                    if (dragItemImage.count == 1) isDraging = false;
                    dragItemImage.ChangeItemCnt(-1);
                    newItemImage.ChangeItemCnt(1 - newItemImage.count);
                }
                else if (rayObject.layer == LayerMask.NameToLayer("ItemImage")) // 슬롯에 이미지가 있을 때
                {
                    previousItemImage = rayObject.GetComponent<ItemImage>();
                    if (previousItemImage.particleName == dragItemImage.particleName)// 이미지가 같으면
                    {
                        if (previousItemImage.wasInTakeSlot)
                        {
                            TransferCnt(dragItemImage, previousItemImage, true);
                            DecreaseInMakeSlot();

                        }
                        else if(!previousItemImage.wasInTakeSlot)
                        {
                            // 한개만 그 이미지에게 주고
                            TransferCnt(previousItemImage, dragItemImage, false);
                        }
                    }
                    else if (previousItemImage.particleName != dragItemImage.particleName)// 이미지가 다르면
                    {
                        // 이미지가 다르면 전체 바꿈.
                        // 그 자리 들어가고
                        dragingItem.transform.SetParent(rayObject.transform.parent);
                        dragingItem.transform.localPosition = Vector3.zero;
                        dragingItem.GetComponent<Image>().raycastTarget = true;
                        dragingItem = previousItemImage.gameObject;
                        // 그 자리 있던 이미지가 따라가게
                        dragItemImage = previousItemImage;
                        dragingItem.transform.SetParent(transform);
                        dragingItem.GetComponent<Image>().raycastTarget = false;
                    }
                }
            }
            else if (!isDraging && rayObject.layer == LayerMask.NameToLayer("ItemImage")) // 첫 우클릭
            {
                // 개수는 반개만 해서 
                // 마우스 따라가게
                isDraging = true;
                if (previousItemImage.wasInTakeSlot)
                {
                    dragingItem = previousItemImage.gameObject;
                    dragingItem.transform.SetParent(transform);
                    dragItemImage = previousItemImage;
                    dragItemImage.GetComponent<Image>().raycastTarget = false;
                    dragItemImage.wasInTakeSlot = false;
                    previousItemImage = null;
                    if (dragItemImage.wasInTakeSlot)
                    {
                        DecreaseInMakeSlot();
                        dragItemImage.wasInTakeSlot = false;
                    }
                }
                else if (!previousItemImage.wasInTakeSlot)
                {
                    GameObject halfDragingItem = Instantiate(rayObject, transform); // 새로운 이미지 생성.
                    dragingItem = halfDragingItem;
                    dragItemImage = halfDragingItem.GetComponent<ItemImage>();
                    previousItemImage.ChangeItemCnt(-(previousItemImage.count / 2) - (previousItemImage.count % 2));
                    dragItemImage.ChangeItemCnt(-(dragItemImage.count / 2));
                    dragingItem.GetComponent<Image>().raycastTarget = false;
                }
            }
        }
        if(isDraging && dragingItem != null)
        {
            dragingItem.transform.position = Input.mousePosition;
        }
    }
    void CheckLayer()
    {
        List<RaycastResult> results = new List<RaycastResult>();
        pointerEventData.position = Input.mousePosition;
        print(pointerEventData);
        graphicRaycaster.Raycast(pointerEventData, results);
        raycastResult = results[0];
        rayObject = raycastResult.gameObject;
        results.RemoveAt(0);

    }
    #region 아이템을 놓는 이벤트
    /*// 이미지가 마우스 따라다니고 있을 때
    if (isDraging && dragingItem != null)
    {
        dragingItem.transform.position = Input.mousePosition;
        if (Input.GetMouseButtonDown(0))// 다시 좌클릭시 
        {
            if(rayObject.layer == LayerMask.NameToLayer("Slot")) //슬롯이 있으면
            {
                // 그 자리 들어가고
                dragingItem.transform.SetParent(rayObject.transform);
                dragingItem.transform.localPosition = Vector3.zero;
                dragingItem.GetComponent<Image>().raycastTarget = true;
                dragItemImage = null;
                dragingItem = null;
                isDraging = false;
            }
            else if(rayObject.layer == LayerMask.NameToLayer("ItemImage")) // 슬롯이 아니라 이미지가 있으면
            {
                previousItemImage = rayObject.GetComponent<ItemImage>();
                if (dragItemImage.particleKind != previousItemImage.particleKind)// 이미지가 다르면
                {
                    // 그 자리 들어가고
                    dragingItem.transform.SetParent(rayObject.transform.parent);
                    dragingItem.transform.localPosition = Vector3.zero;
                    dragingItem.GetComponent<Image>().raycastTarget = true;
                    dragingItem = previousItemImage.gameObject;
                    // 그 자리 있던 이미지가 따라가게
                    dragItemImage = previousItemImage;
                    dragingItem.transform.SetParent(transform);
                    dragingItem.GetComponent<Image>().raycastTarget = false;
                    *//*makingSlot = nowMakeSlots.transform.GetChild(1).GetComponent<MakingSlot>();
                    makingSlot.SaveAllData();
                    if (dragItemImage.wasInTakeSlot)
                    {
                        DecreaseInMakeSlot();
                        dragItemImage.wasInTakeSlot = false;
                    }*//*
                }
                else if (dragItemImage.particleKind == previousItemImage.particleKind)// 이미지 검사후
                {
                    // 이미지가 같은 거면 숫자 체크
                    TransferCnt(previousItemImage, dragItemImage, true);
                    print(previousItemImage);
                    print(previousItemImage.transform.parent);
                    print(previousItemImage.gameObject);
                    print(dragingItem);
                    print(dragItemImage);
                }
            }
        }
        else if (Input.GetMouseButtonDown(1)) // 다시 우클릭시
        {
            // 이미지가 마우스 따라다니고 있을 때
            if (rayObject.layer == LayerMask.NameToLayer("Slot")) // 슬롯에 이미지가 없으면
            {
                // 그 슬롯에 한개만 줌.
                newItem = Instantiate(dragingItem, rayObject.transform);
                newItem.transform.localPosition = Vector3.zero;
                newItem.GetComponent<Image>().raycastTarget = true;
                newItemImage = newItem.GetComponent<ItemImage>();
                dragItemImage.ChangeItemCnt(-1);
                newItemImage.ChangeItemCnt(1 - newItemImage.count);
            }
            else if(rayObject.layer == LayerMask.NameToLayer("ItemImage")) // 슬롯에 이미지가 있을 때
            {
                previousItemImage = rayObject.GetComponent<ItemImage>();
                if(previousItemImage.particleKind == dragItemImage.particleKind)// 이미지가 같으면
                {
                    // 한개만 그 이미지에게 주고
                    TransferCnt(previousItemImage, dragItemImage, false);
                }
                else if(previousItemImage.particleKind != dragItemImage.particleKind)// 이미지가 다르면
                {
                    // 이미지가 다르면 전체 바꿈.
                    // 그 자리 들어가고
                    dragingItem.transform.SetParent(rayObject.transform.parent);
                    dragingItem.transform.localPosition = Vector3.zero;
                    dragingItem.GetComponent<Image>().raycastTarget = true;
                    dragingItem = previousItemImage.gameObject;
                    // 그 자리 있던 이미지가 따라가게
                    dragItemImage = previousItemImage;
                    dragingItem.transform.SetParent(transform);
                    dragingItem.GetComponent<Image>().raycastTarget = false;
                }
            }

        }
        print("출력1 : " + dragingItem);
    }*/
    #endregion

    #region 드래그 시작 이벤트
    /*// 처음 좌클릭시 마우스 따라가기.
    if (rayObject != null && rayObject.layer == LayerMask.NameToLayer("ItemImage") && !isDraging)
    {
        if (Input.GetMouseButtonDown(0) && !isDraging)
        {
            isDraging = true;
            dragingItem = rayObject;
            dragItemImage = dragingItem.GetComponent<ItemImage>();
            dragingItem.transform.SetParent(transform);
            dragingItem.GetComponent<Image>().raycastTarget = false;
            makingSlot = nowMakeSlots.transform.GetChild(1).GetComponent<MakingSlot>();
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
            GameObject halfDragingItem = Instantiate(rayObject, transform); // 새로운 이미지 생성.
            dragingItem = halfDragingItem;
            dragItemImage = halfDragingItem.GetComponent<ItemImage>();
            previousItemImage.ChangeItemCnt(-(previousItemImage.count / 2));
            dragItemImage.ChangeItemCnt(-(dragItemImage.count / 2) - (dragItemImage.count % 2));
            dragingItem.GetComponent<Image>().raycastTarget = false;
        }
        print("출력2 : " + dragingItem);
    }*/
    #endregion
    public void ChangeSameCnt(int slotNum, int changeToValue)  // 증감 하는 함수.
    {
        ItemImage pop = quickSlot[slotNum].GetComponentInChildren<ItemImage>();
        pop.ChangeItemCnt(-pop.count + changeToValue); 
    }


    public void SetItemPositionInQuickSlot(GameObject item, int i)
    {
        item.transform.parent = quickSlot[i].transform;
        item.transform.localPosition = Vector3.zero;
        item.transform.localScale = Vector3.one;
    }

    public void SetItemPosInInven(GameObject item) // Quick슬롯에 자리 없을때 들어오게.
    {
        for(int i =0; i< inven.Length; i++)
        {
            if (inven[i].transform.childCount == 0)
            {
                item.transform.parent = inven[i].transform;
                item.transform.localPosition = Vector3.zero;
                item.transform.localScale = Vector3.one;
                return;
            }
        }
    }

    // Static을 읽어서 Popup에 동기화 하는 함수.
    public void CheckQuickSlot() // 퀵슬롯과 static슬롯 동기화 함수.
    {
        // Drag로 인해 아이템이 생기거나 사라질때
        // InvenStatic에도 이미지 및 데이터 동기화.
        for(int i = 0; i < 9; i++)
        {
            
            if (InventoryStatic.instance.slots[i].transform.childCount != 4) // 이미지가 있으면 저장.
            {
                staticItem = InventoryStatic.instance.slots[i].transform.GetChild(4);
                staticGo = staticItem.gameObject;
            }
            if (quickSlot[i].transform.childCount == 1) // 퀵슬롯에 있으면
            {
                // 똑같은 이미지 생성.
                quickItem = instance.quickSlot[i].transform.GetChild(0);
                quickGo = quickItem.gameObject;
                if (InventoryStatic.instance.slots[i].transform.childCount != 4) // 이미 static에 있으면
                {
                    ItemImage quick = quickGo.GetComponent<ItemImage>();
                    ItemImage staticCs = staticGo.GetComponent<ItemImage>();
                    if(quick.particleName == staticCs.particleName) // 종류가 같으면
                    {
                        staticCs.ChangeItemCnt(quick.count-staticCs.count); // quick.count값으로 교체
                    }
                    else // 종류가 다르면
                    {
                        staticCs.particleObjectTr = quick.particleObjectTr;
                        staticCs.particleName = quick.particleName;
                        staticCs.itemImage.sprite = quick.itemImage.sprite;
                        staticCs.ChangeItemCnt(quick.count - staticCs.count);
                    }
                }
                else if(InventoryStatic.instance.slots[i].transform.childCount == 4) // static에 아이템 없으면
                {
                    GameObject sameImage = Instantiate(quickGo, InventoryStatic.instance.slots[i].transform);
                    sameImage.transform.localPosition = Vector3.zero;
                }
            }
            else if (quickSlot[i].transform.childCount == 0) // 퀵슬롯에 아이템이 없으면
            {
                Destroy(staticGo); // static 슬롯도 없애버림.
            }
            staticItem = null; // 초기화
            staticGo = null;
        }

    }

    public void CheckUseMaker()
    {
        if (useMaker)
        {
            withMaker.SetActive(true);
            withoutMaker.SetActive(false);
            nowMakeSlots = withMaker;
        }
        else if (!useMaker)
        {
            withMaker.SetActive(false);
            withoutMaker.SetActive(true);
            nowMakeSlots = withoutMaker;
        }
    }



    public void DecreaseInMakeSlot()
    {
        print("makeSlot 감소");
        for (int i = 0; i < makingSlot.dropSlot.Length; i++)
        {
            ItemImage item = null;
            if (makingSlot.dropSlot[i] == null)
            {
                continue;
            }
            if (makingSlot.dropSlot[i].transform.childCount != 0)
            {
                item = makingSlot.dropSlot[i].GetComponentInChildren<ItemImage>();
                if(item.count == 1)
                {
                    item.ChangeItemCnt(-1);
                    if(item != null)
                    {
                        Destroy(item.gameObject);
                        print(i + " 번째 슬롯의 자식 수 : " +makingSlot.dropSlot[i].transform.childCount);
                    }
                }
                else
                {
                    item.ChangeItemCnt(-1);
                }
            }
        }
    }

    int totalCnt;
    int exceededCnt;
    int maxCnt = 64;
    /// <summary>
    /// 전체를 줄 때는 cnt에 drag.count
    /// 하나만 줄 때는 cnt에 1 
    /// wasIntakeSlot은 drag에 previous를 넣음.
    /// </summary>
    /// <param name="previous"></param>
    /// <param name="drag"></param>
    /// <param name="all"></param>
    void TransferCnt(ItemImage previous, ItemImage drag, bool all)
    {
        int cnt = 0;
        if (all)
        {
            cnt = drag.count;
        }
        else if (!all)
        {
            cnt = 1;
        }
        if (previous.count + cnt <= maxCnt) // maxCnt == 64
        {
            // 우선 숫자 증가시켜.
            previous.ChangeItemCnt(cnt); // 옮긴느 개수만큼.
            if (all)
            {
                if (drag.wasInTakeSlot)
                {
                    
                }
                else
                {
                    if (drag.particleObjectTr != null && previous.particleObjectTr != drag.particleObjectTr)
                    {
                        Destroy(drag.particleObjectTr);
                    }
                    drag.particleObjectTr = null;
                    print(drag.gameObject);
                    Destroy(drag.gameObject);
                    dragItemImage = null;
                    dragingItem = null;
                    isDraging = false;
                }
            }
            if(!all)
            {
                if(drag.count == 1)
                {
                    isDraging = false;
                    dragingItem = null;
                    dragItemImage = null;
                }
                drag.ChangeItemCnt(-cnt);
            }
        }
        else // 초과할경우
        {
            totalCnt = previous.count + cnt;
            exceededCnt = totalCnt - maxCnt; // 초과양.
            previous.ChangeItemCnt(maxCnt - previous.count);
            drag.ChangeItemCnt(exceededCnt - cnt);
        }
    }






}
