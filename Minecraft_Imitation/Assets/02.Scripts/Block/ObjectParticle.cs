using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectParticle : MonoBehaviour
{

    public ObjectParticleData.ParticleKind particleKind; // 오브젝트 파티클 종류
    public float speed = 1;
    public float length = 0.15f;
    public Transform particleObject;
    float runningTime = 0f; 
    float y = 0f;
    


    void Update()
    {
        MovementPosY();
        ParticleObjectMovement();
    }

    private void MovementPosY() // Y축 이동
    {
        runningTime += Time.deltaTime * speed;
        y = Mathf.Sin(runningTime) * length;
        particleObject.transform.localPosition += Vector3.up * y * Time.deltaTime;
    }

    private void ParticleObjectMovement() // 회전
    {
        particleObject.Rotate(Vector3.up * speed * Time.deltaTime);
        
    }

}
