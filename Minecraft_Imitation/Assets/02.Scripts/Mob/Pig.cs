using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Pig : Mob
{

    public Transform temp;
    

    void Update()
    {
        if (init_test)
        {
            init_test = false;
            initEntitiy(c_x, c_z, b_x, b_y, b_z);
        }

        if (start_test)
        {
            start_test = false;
            UpdateHP(temp, -1, 3);
        }

        LifeCycle();
    }

    private void LifeCycle()
    {
        if (alive)
        {
            Fall();
            mobSpawnData.positionData = MapManager.instance.PositionToBlockData(transform.position);

            //print(mobSpawnData.positionData.chunk_X + " , " + mobSpawnData.positionData.chunk_Z);
            if (mobState == MobState.Idle)
            {
                nextMovementTime -= Time.deltaTime;
                if (nextMovementTime <= 0)
                {
                    nextMovementTime = 5;
                    AStar_Random();
                    SetWayPosition();
                }
                else if(nextMovementTime > 5) // 랜덤값이 5보다 큰 객체는 주변 탐색
                {
                    FindVisibleTargets();
                    if (targetTransform != null)
                    {
                        Rotation();
                    }
                }
                else // 탐색해서 무언가 찾았다면 계속 쳐다모기
                {
                    if(targetTransform != null)
                    {
                        FindVisibleTargets();
                        if (targetTransform != null)
                        {
                            Rotation();
                        }
                    }
                }
            }
            else if (mobState == MobState.Hit)
            {

                nextMovementTime -= Time.deltaTime;
                if (nextMovementTime <= 0)
                {
                    nextMovementTime = 5;
                    Runaway();
                }
            }
            else if(mobState == MobState.Move)
            {
                nextMovementTime -= Time.deltaTime;
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
