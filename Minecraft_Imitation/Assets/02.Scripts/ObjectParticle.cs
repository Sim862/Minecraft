using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ObjectParticle : MonoBehaviour
{

    private Rigidbody rigidbody;
    private Collider rigidCollider;
    public Transform particleObject;
    public float rotatingSpeed = 1;
    public float rotatingLength = 0.15f;
    public float moveSpeed = 5;
    float runningTime = 0f; 
    float y = 0f;
    
    public ObjectParticleData.ParticleKind particleKind; // 오브젝트 파티클 종류
    public int count = 1;
    public Sprite icon;
    private Transform target;
    private bool onTarget = false;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidCollider = GetComponent<Collider>();
    }


    void Update()
    {
        MovementPosY();
        ParticleObjectMovement();
        Movement();
    }

    public void InitParticle(int count)
    {
        this.count = count;
    }

    private void Movement()
    {
        if(onTarget)
        {
            if(target != null)
            {
                if(Vector3.Distance(transform.position, target.position) > 0.3f)
                {
                    transform.position += (target.position - transform.position).normalized * moveSpeed * Time.deltaTime;
                }
                else
                {
                    if (target.CompareTag("Player"))
                    {
                        int remainder = 0;
                        //remainder = target.GetComponent<플레이어 인벤토리 관리 컴포턴트>().인벤토리에 넣어주고 남은값 주는 함수;
                        if (remainder == 0)
                        {
                            Destroy(gameObject);
                        }
                        else
                        {
                            count = remainder;
                            ResetRigid();
                        }
                    }
                }
            }
            else
            {

            }
        }
        
    }

    private void MovementPosY() // Y축 이동
    {
        runningTime += Time.deltaTime * rotatingSpeed;
        y = Mathf.Sin(runningTime) * rotatingLength;
        particleObject.transform.position += Vector3.up * y * Time.deltaTime;

    }

    private void ParticleObjectMovement() // 회전
    {
        particleObject.Rotate(Vector3.up * rotatingSpeed * Time.deltaTime);
        
    }

    private void ResetRigid()
    {
        rigidbody.useGravity = true;
        rigidCollider.isTrigger = false;
    }

    private void CollisionPlayer(Transform other)
    {
        target = other.transform;
        rigidbody.useGravity = false;
        rigidCollider.isTrigger = true;
        onTarget = true;
    }


    private void CollisionParticle(ObjectParticle other)
    {
        if (particleKind != other.particleKind) // 같은 종류의 아이템이 아니면 리턴
            return;

        if (other.count < count)
            return;

        if(other.count == count)
        {
            if(gameObject.GetHashCode() > other.gameObject.GetHashCode())
            {
                return;
            }
        }

        other.count += count;
        if(other.count > 64)
        {
            other.count = 64;
            count = other.count - 64;
            ResetRigid();
        }
        else
        {
            Destroy(gameObject);
        }
    }



    private void OnTriggerStay(Collider other)
    {

        if (!onTarget) // 이미 충돌해 타겟한테 이동 중이면 리턴
        {
            if (other.CompareTag("Player"))
            {
                //if(other.GetComponent<플레이어>.아이템을 먹을 수 있는지){
                //    onTarget = true;
                //    target = other.transform;
                //    CollisionPlayer(other.transform);
                //}
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!onTarget)
        {
            if (other.CompareTag("ObjectParticle"))
            {
                if(Vector3.Distance(transform.position, other.transform.position) < 0.4f)
                {
                    CollisionParticle(other.GetComponent<ObjectParticle>());
                }
            }
        }
    }
}
