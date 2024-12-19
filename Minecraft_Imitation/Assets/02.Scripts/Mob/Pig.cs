using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Pig : Mob
{

    public Transform temp;

    private float idleSoundTimer = 0;
    private void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        LifeCycle();
    }

    public void LifeCycle()
    {
        if (alive)
        {
            Fall();
            mobSpawnData.positionData = MapManager.instance.PositionToBlockData(transform.position);
            SetChunkData();

            idleSoundTimer += Time.deltaTime;
            nextMovementTime -= Time.deltaTime;
            if (mobState == MobState.Idle)
            {
                if (idleSoundTimer > 3)
                {
                    if (Random.value < 0.3)
                    {
                        SoundManager.instance.ActiveOnShotSFXSound(idleSound, transform, transform.position);
                    }
                    idleSoundTimer = 0;
                }
                if (nextMovementTime <= 0)
                {
                    nextMovementTime = 15;
                    int x = Random.Range(1, 10);
                    int z = Random.Range(1, 10);
                    PositionData positionData = new PositionData(mobSpawnData.positionData.chunk_X, mobSpawnData.positionData.chunk_Z, x, mobSpawnData.positionData.blockIndex_y, z);
                    while(MapManager.instance.GetBlock(positionData.chunk, positionData.blockIndex_x,positionData.blockIndex_y+1, positionData.blockIndex_z) != null)
                        positionData = new PositionData(mobSpawnData.positionData.chunk_X, mobSpawnData.positionData.chunk_Z, x, mobSpawnData.positionData.blockIndex_y+1, z);
                    AStar(positionData, null);
                    SetWayPosition();
                }
                else if(nextMovementTime > 5) 
                {
                    FindVisibleTargets();
                    if (targetTransform != null)
                    {
                        Rotation();
                    }
                }// 랜덤값이 5보다 큰 객체는 주변 탐색

                else
                {
                    if(targetTransform != null)
                    {
                        FindVisibleTargets();
                        if (targetTransform != null)
                        {
                            Rotation();
                        }
                    }
                }// 탐색해서 무언가 찾았다면 계속 쳐다보기
            }
            else if (mobState == MobState.Hit)
            {
                if (nextMovementTime <= 0)
                {
                    nextMovementTime = 5;
                    Runaway();
                }
            }
            else if(mobState == MobState.Move)
            {
                if (nextMovementTime <= 0)
                {
                    nextMovementTime = 5;
                    wayPoints.Clear();
                    SetWayPosition();
                }
            }
            Movement();
        }
    }
}
