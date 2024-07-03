using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ObjectParticle : MonoBehaviour
{

    private Rigidbody rigidbody;
    private Collider collider;
    public Transform particleObject;
    public float rotatingSpeed = 1;
    public float rotatingLength = 0.15f;
    public float moveSpeed = 5;
    float runningTime = 0f; 
    float y = 0f;
    
    public ObjectParticleData.ParticleKind particleKind; // 오브젝트 파티클 종류
    public int count = 1;
    private Sprite icon;
    private Transform target;
    private bool onTarget = false;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
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
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
    }

    private void MovementPosY() // Y축 이동
    {
        runningTime += Time.deltaTime * rotatingSpeed;
        y = Mathf.Sin(runningTime) * rotatingLength;
        particleObject.transform.localPosition += Vector3.up * y * Time.deltaTime;

    }

    private void ParticleObjectMovement() // 회전
    {
        particleObject.Rotate(Vector3.up * rotatingSpeed * Time.deltaTime);
        
    }

    private void CollisionParticle(ObjectParticle other)
    {
        if (particleKind != other.particleKind) // 같은 종류의 아이템이 아니면 리턴
            return;

        if(other.count > count)
        {
            other.count += count;
            target = other.transform;
            rigidbody.useGravity = false;
            collider.isTrigger = true;
            onTarget = true;

        }
        else if(other.count == count)
        {
            if(gameObject.GetHashCode() < other.gameObject.GetHashCode())
            {
                other.count += count;
                target = other.transform;
                rigidbody.useGravity = false;
                collider.isTrigger = true;
                onTarget = true;
            }
        }
    }

    private void CollisionPlayer(Transform other)
    {
        target = other.transform;
        rigidbody.useGravity = false;
        collider.isTrigger = true;
        onTarget = true;
    }

    private void OnTriggerStay(Collider other)
    {

        if (target == null) // 이미 충돌해 타겟한테 이동 중이면 리턴
        {
            if (other.CompareTag("ObjectParticle"))
            {

            }
        }
    }

}
