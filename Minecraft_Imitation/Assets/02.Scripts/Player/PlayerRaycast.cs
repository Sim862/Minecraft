using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRaycast : MonoBehaviour
{
    //public GameObject camera;
    //bool printFirst = true;
    public float aimRange = 10;
    public GameObject blockFac;
    public float mouseOneCool = 1;
    float curTime = 0;
    bool firstAim = true;
    public float breakPower = 5;

    bool cursorLock = true;
    Transform hitNowBlock; // 클릭할때
    Transform hitBlockTr;
    Block hitBlockCs;

    BlockData.BlockType test = BlockData.BlockType.Pick;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    bool walk = false;
    // Update is called once per frame
    void Update()
    {
        // 레이를 생성한 후 발사될 위치와 진행방향을 설정한다.
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        // 레이가 부딪힌 대상의 정보를 저장할 변수를 생성한다.
        RaycastHit hitInfo = new RaycastHit();

        Physics.Raycast(ray, out hitInfo, aimRange); // 에임 사정거리
        Vector3 normalVec = hitInfo.normal;
        if (hitInfo.transform != null && hitInfo.transform.GetComponent<Block>() != null)
        {
            hitBlockTr = hitInfo.transform; // 에임이 보고있는 블럭 저장.
            hitBlockCs = hitInfo.transform.GetComponent<Block>();
        }
        if (Input.GetMouseButtonDown(1)) // 마우스 우클릭시 설치 및 사용
        {
            GameObject block = Instantiate(blockFac);
            block.transform.position = hitInfo.transform.position + normalVec*SizeVector(hitInfo);

        }
        Transform nowbreakBlock = null;
        Block nowbreakBlockCs = null;
        bool isWork = false;
        if (Input.GetMouseButton(0))
        {
            if(nowbreakBlock == null && hitBlockTr != null)
            {
                print("Break 실행.");
                print(hitBlockTr + " / " + nowbreakBlock);
                nowbreakBlock = hitBlockTr;
                nowbreakBlockCs = nowbreakBlock.GetComponent<Block>();
                nowbreakBlockCs.Break(test, breakPower);
                isWork = true;
            }
            if (isWork)
            {
                if(nowbreakBlock != hitBlockTr)
                {
                    print("대상 바뀜");
                    nowbreakBlockCs.StopBroke();
                    nowbreakBlock = hitBlockTr;
                    nowbreakBlockCs = nowbreakBlock.GetComponent<Block>();
                    if(nowbreakBlock != null)
                    {
                        nowbreakBlockCs.Break(test, breakPower);
                        print("대상 바뀐 후 다시 Break 실행.");
                        print("대상 바뀐 후 : " + hitBlockTr + " / " + nowbreakBlock);
                    }
                    else
                    {
                        isWork = false;
                    }
                }
            }
        }
        else
        {
            if (isWork)
            {
                print("마우스를 떼서 멈춤.");
                nowbreakBlockCs.StopBroke();
                nowbreakBlock = null;
                isWork = false;
            }
        }
        

        CursurLockMethod();

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

    void CursurLockMethod()
    {
        if (Input.GetKeyDown(KeyCode.Q)) // q를 누르면 부수는 행위
        {
            cursorLock = !cursorLock;
        }
        if (cursorLock)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
