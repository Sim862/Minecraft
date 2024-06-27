using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRaycast : MonoBehaviour
{
    //public GameObject camera;
    //bool printFirst = true;
    bool cursorLock = true;
    public GameObject blockFac;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        // 레이를 생성한 후 발사될 위치와 진행방향을 설정한다.
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        // 레이가 부딪힌 대상의 정보를 저장할 변수를 생성한다.
        RaycastHit hitInfo = new RaycastHit();

        Physics.Raycast(ray, out hitInfo);
        Vector3 normalVec = hitInfo.normal;
        if (Input.GetMouseButtonDown(0))
        {
            print(normalVec + " : 해당 오브젝트의 노말 벡터");
            print(hitInfo.point+ " : 닿은 위치의 포지션 값");
            print(hitInfo.transform.position + " : 해당 오브젝트의 포지션 값");
            GameObject block = Instantiate(blockFac);
            //block.transform.position = hitInfo.transform.position + normalVec;
            block.transform.position = hitInfo.transform.position + normalVec*SizeVector(hitInfo);
            print(SizeVector(hitInfo));

        }
        if (Input.GetMouseButtonDown(1))
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
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Destroy(hitInfo.collider.gameObject);
        }
        

    }

    float SizeVector(RaycastHit hitInfo)
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
}
