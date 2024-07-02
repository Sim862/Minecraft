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
    
    public ObjectParticleData.ParticleKind particleKind; // ������Ʈ ��ƼŬ ����
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
        icon = DataManager.instance.GetUIItemIcon(particleKind);
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

    private void MovementPosY() // Y�� �̵�
    {
        runningTime += Time.deltaTime * rotatingSpeed;
        y = Mathf.Sin(runningTime) * rotatingLength;
        particleObject.transform.localPosition += Vector3.up * y * Time.deltaTime;

    }

    private void ParticleObjectMovement() // ȸ��
    {
        particleObject.Rotate(Vector3.up * rotatingSpeed * Time.deltaTime);
        
    }

    private void CollisionParticle(ObjectParticle other)
    {
        if (particleKind != other.particleKind) // ���� ������ �������� �ƴϸ� ����
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

        if (target == null) // �̹� �浹�� Ÿ������ �̵� ���̸� ����
        {
            if (other.CompareTag("ObjectParticle"))
            {

            }
        }
    }

}
