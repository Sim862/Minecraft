using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRaycast : MonoBehaviour
{
    public float aimRange = 10;
    public GameObject blockFac;
    public float mouseOneCool = 1;
    public float breakPower = 5;
    Vector3 newBlockPos;
    bool isBlock;

    Transform hitNowBlock; // 클릭할때
    Transform hitBlockTr;
    Block hitBlockCs;

    Vector3 normalVec;
    RaycastHit hitInfo;

    void Update()
    {
        // 레이를 생성한 후 발사될 위치와 진행방향을 설정한다.
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        // 레이가 부딪힌 대상의 정보를 저장할 변수를 생성한다.
        hitInfo = new RaycastHit();

        bool anyHit = Physics.Raycast(ray, out hitInfo, aimRange); // 에임 사정거리
        if (anyHit && PlayerManager.onInventory == false)
        {
            normalVec = hitInfo.normal;
            hitBlockTr = hitInfo.transform; // 에임이 보고있는 블럭 저장.
            if (hitInfo.transform.GetComponent<Block>() != null)
            {
                hitBlockCs = hitInfo.transform.GetComponent<Block>();
            }
            if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Block"))
            {
                isBlock = true;
            }
            else
            {
                isBlock = false;
            }
            MouseRitghtClick();

        }

        #region 좌클릭 이벤트 EventWithBox.cs에 구현함.
        /*if (Input.GetMouseButton(0)) // 좌클릭하면
        {
            if(nowbreakBlock == null && hitBlockTr != null) // 에임에 블럭이 있고 과거 블럭이 저장 안되있다면
            {
                print("Break 실행.");
                print(hitBlockTr + " / " + nowbreakBlock);
                nowbreakBlock = hitBlockTr;
                nowbreakBlockCs = nowbreakBlock.GetComponent<Block>();
                nowbreakBlockCs.Break(test, breakPower);
                isWork = true;
            }
            if (isWork) // 블럭이 저장 됐다면
            {
                if(nowbreakBlock != hitBlockTr) // 지금 가리키는 블럭이 바뀌었다면
                {
                    print("대상 바뀜");
                    nowbreakBlockCs.StopBroke();
                    nowbreakBlock = hitBlockTr;
                    nowbreakBlockCs = nowbreakBlock.GetComponent<Block>();
                    if(nowbreakBlock != null) // 가리키는 블럭이 바뀌고 부수던 블럭이 사라지지 않았다면
                    {
                        nowbreakBlockCs.Break(test, breakPower);
                        print("대상 바뀐 후 다시 Break 실행.");
                        print("대상 바뀐 후 : " + hitBlockTr + " / " + nowbreakBlock);
                    }
                    else // 부수던 블럭이 사라졌다면
                    {
                        isWork = false;
                    }
                }
            }
        }
        else // 좌클릭을 뗐을때
        {
            if (isWork) // 블럭을 저장하고 에임이 벗어나지 않은 상태에서 좌클릭만 떼면
            {
                print("마우스를 떼서 멈춤.");
                nowbreakBlockCs.StopBroke();
                
                isWork = false;
            }
            nowbreakBlock = null;
        }
        // 좌클릭하면 캐기 시작한 블럭 저장.*/
        #endregion
    }

    float SizeVector(RaycastHit hitInfo) // 사이즈별 설치 실험
    {
        if (Mathf.Abs(hitInfo.normal.x) > 0)
        {
            return hitInfo.transform.localScale.x/2 + 0.5f;
        }
        else if (Mathf.Abs(hitInfo.normal.y) > 0)
        {
            return hitInfo.transform.localScale.y/2 + 0.5f;
        }
        else if (Mathf.Abs(hitInfo.normal.z) > 0)
        {
            return hitInfo.transform.localScale.z/2 + 0.5f;
        }
        else
        {
            return 0;
        }
    }

    void MouseRitghtClick() // 우클릭시 일어나는 함수.
    {
        if (Input.GetMouseButtonDown(1)) // 마우스 우클릭시 설치 및 사용
        {
            int slotnumber = PlayerManager.instance.usingSlot; // 현재 사용중인 슬롯넘버 저장.
            if (isBlock)
            {
                InstallBlock(slotnumber);
            }
            if(hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Maker"))
            {
                InventoryPopup.instance.useMaker = true;
                PlayerManager.instance.OnOffInventory();
            }
        }
    }
    void InstallBlock(int slotNumber)
    {
        GameObject nowUsingObject = InventoryStatic.instance.slots[slotNumber];
        GameObject nowUsingObjectInQuick = InventoryPopup.instance.quickSlot[slotNumber];
        ItemImage nowItemImage = nowUsingObject.GetComponentInChildren<ItemImage>();
        ItemImage nowItemImageInQuick = nowUsingObjectInQuick.GetComponentInChildren<ItemImage>();
        if (nowItemImage != null)
        {
            newBlockPos = hitInfo.transform.position + normalVec * SizeVector(hitInfo);
            print(newBlockPos);
            if (Vector3.Distance(transform.position, newBlockPos) > 1)
            {
                nowItemImage.ChangeItemCnt(-1);
                nowItemImageInQuick.ChangeItemCnt(-1);
                PositionData positionData = MapManager.instance.PositionToBlockData(newBlockPos);
                BlockData.BlockKind blockKind = DataManager.instance.ParticleToBlockKind(nowItemImage.particleKind);
                MapManager.instance.CreateBlock(positionData.chunk, blockKind, positionData.blockIndex_x,positionData.blockIndex_y,positionData.blockIndex_z, "SS");

            }
        }
    }

    void UseMaker()
    {

    }
}
