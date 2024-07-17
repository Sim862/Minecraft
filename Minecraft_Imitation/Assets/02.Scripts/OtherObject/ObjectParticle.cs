using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ObjectParticle : MonoBehaviour
{

    public Rigidbody rigidbody;
    public Collider rigidCollider;
    public Transform particleObject;
    public float rotateSpeed = 1;
    public float rotateLength = 0.15f;
    public float moveSpeed = 5;
    float runningTime = 0f; 
    float y = 0f;

    private List<Transform> particleObjects = new List<Transform>();
    
    public ObjectParticleData.ParticleName particleName; // 오브젝트 파티클 이름
    public ObjectParticleData.ParticleType particleType; // 오브젝트 파티클 타입
    public Sprite icon;

    public int count = 1;
    public int durability = 100;
    private Transform target;
    private bool onTarget = false;
    public float power = 1;
    private bool drop = false;
    private float pickupCooltime = 1.5f;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidCollider = GetComponent<Collider>();
        particleObjects.Add(particleObject);
    }


    void Update()
    {
        MovementPosY();
        ParticleObjectMovement();
        Movement();
        
    }

    public void InitParticle(int count)
    {
        UpdateCount(count);
    }

    public void UpdateCount(int count)
    {
        this.count = count;
        if (count > 1)
        {
            if (particleObjects.Count < 2)
            {
                float ran = Random.Range(0.03f, 0.07f);
                Transform temp = Instantiate(particleObject, particleObject.position + new Vector3(ran, ran, ran), particleObject.rotation, particleObject);
                temp.localScale = Vector3.one;
                particleObjects.Add(temp);
            }
        }
        else
        {
            if (particleObjects.Count > 1)
            {
                Transform temp = particleObjects[1];
                particleObjects.RemoveAt(1);
                Destroy(temp);
            }
        }

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
                        UpdateCount(PlayerManager.instance.remainder);
                        if (count == 0)
                        {
                            Destroy(gameObject);
                        }
                        else
                        {
                            ResetRigid();
                        }
                    }
                }
            }
            else
            {
                onTarget = false;
                ResetRigid();
            }
        }
        
    }

    private void MovementPosY() // Y축 이동
    {
        runningTime += Time.deltaTime * rotateSpeed;
        y = Mathf.Sin(runningTime) * rotateLength;
        particleObject.transform.position += Vector3.up * y * Time.deltaTime;

    }

    private void ParticleObjectMovement() // 회전
    {
        particleObject.Rotate(Vector3.up * rotateSpeed * Time.deltaTime * 10);
        
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
        if (particleName != other.particleName) // 같은 종류의 아이템이 아니면 리턴
            return;

        if (other.count < count)
            return;

        if (other.count == count)
        {
            if(gameObject.GetHashCode() > other.gameObject.GetHashCode())
            {
                return;
            }
        }

        if(other.count + count > 64)
        {
            other.count = 64;
            UpdateCount(other.count - 64);
            ResetRigid();
        }
        else
        {
            other.UpdateCount(other.count + count);
            StartCoroutine(MoveParticle(other.transform.position));
        }
    }

    IEnumerator MoveParticle(Vector3 position)
    {
        rigidbody.useGravity = false;
        rigidCollider.isTrigger = true;
        onTarget = true;
        float distance = Vector3.Distance(transform.position, position) / 15;
        Vector3 direction = (position - transform.position).normalized;
        for (int i = 0; i < 10; i++)
        {
            transform.position += direction * distance;
            yield return new WaitForSeconds(0.02f);
        }

        Destroy(gameObject);
    }

    public void Drop()
    {
        drop = true;
    }

    IEnumerator DropCoolTime()
    {
        if (drop)
        {
            yield return new WaitForSeconds(pickupCooltime);
            drop = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {

        if (!onTarget && !drop) // 이미 충돌해 타겟한테 이동 중이면 리턴
        {
            if (other.CompareTag("Player"))
            {
                if (PlayerManager.instance.CheckGetItem()){
                    onTarget = true;
                    target = other.transform;
                    CollisionPlayer(other.transform);
                }
            }
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (!onTarget && !drop)
    //    {
    //        if (other.CompareTag("ObjectParticle"))
    //        {
    //            if(other.GetComponent<ObjectParticle>() == null)
    //            CollisionParticle(other.GetComponentInParent<ObjectParticle>());
    //        }
    //    }
    //}
}
