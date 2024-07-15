using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider : Mob
{

    public Transform temp;

    private float idleSoundTimer = 0;
    private float attackCoolTime = 0;

    public float damage = -5;

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

    public void LifeCycle()
    {
        if (alive)
        {
            Fall();
            mobSpawnData.positionData = MapManager.instance.PositionToBlockData(transform.position);
            SetChunkData();

            //print(mobSpawnData.positionData.chunk_X + " , " + mobSpawnData.positionData.chunk_Z);
            idleSoundTimer += Time.deltaTime;
            attackCoolTime += Time.deltaTime;
            if (mobState == MobState.Idle)
            {
                nextMovementTime -= Time.deltaTime;
                if (idleSoundTimer > 3)
                {
                    if (Random.value < 0.3)
                    {
                        SoundManager.instance.ActiveOnShotSFXSound(idleSound, transform, transform.position);
                    }
                    idleSoundTimer = 0;
                }

                if (targetTransform == null)
                {
                    FindVisibleTargets();
                }

                if (targetTransform != null)
                {
                    if (Vector3.Distance(transform.position, targetTransform.position) < 10)
                    {
                        AStar(MapManager.instance.PositionToBlockData(targetTransform.position), targetTransform);
                        SetWayPosition();
                    }
                    if (Vector3.Distance(transform.position, targetTransform.position) < 1)
                    {
                        if (attackCoolTime > 2 && !PlayerManager.instance.playerDead)
                        {
                            attackCoolTime = 0;
                            targetTransform.GetComponent<PlayerMove>().UpdateHP(-1);
                            wayPoints.Clear();
                            SetWayPosition();
                        }
                    }
                    else
                    {
                        targetTransform = null;
                    }
                }

                if (nextMovementTime <= 0 && targetTransform == null)
                {
                    nextMovementTime = 5;
                    AStar_Random();
                    SetWayPosition();
                }
            }
            else if (mobState == MobState.Hit)
            {

                nextMovementTime -= Time.deltaTime;
                if (nextMovementTime <= 0)
                {
                    nextMovementTime = 5;
                    if(target != null)
                    {
                        targetTransform = target;
                        AStar(MapManager.instance.PositionToBlockData(target.position), target);
                        SetWayPosition();
                    }
                }
            }
            else if (mobState == MobState.Move)
            {
                nextMovementTime -= Time.deltaTime;
                if (nextMovementTime <= 0)
                {
                    nextMovementTime = 5;
                    wayPoints.Clear();
                    SetWayPosition();
                }

                if (targetTransform != null)
                {
                    if (Vector3.Distance(transform.position, targetTransform.position) < 1 && attackCoolTime > 2)
                    {
                        if (!PlayerManager.instance.playerDead)
                        {
                            attackCoolTime = 0;
                            targetTransform.GetComponent<PlayerMove>().UpdateHP(damage);
                            wayPoints.Clear();
                            SetWayPosition();
                        }
                    }
                }
            }

            Movement();
        }
    }
}