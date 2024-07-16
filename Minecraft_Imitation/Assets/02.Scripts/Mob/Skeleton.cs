using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : Mob
{
    public Transform temp;

    private float idleSoundTimer = 0;
    private float attackCoolTime = 0;

    public float damage = -5;

    public float attackDistance = 5;

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
            nextMovementTime -= Time.deltaTime;

            if (targetTransform == null)
            {
                FindVisibleTargets();
            }
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

                if (targetTransform != null)
                {
                    if (Vector3.Distance(transform.position, targetTransform.position) < 10)
                    {

                        if (Vector3.Distance(transform.position, targetTransform.position) < attackDistance)
                        {
                            if (attackCoolTime > 2 && !PlayerManager.instance.playerDead)
                            {
                                mobState = MobState.Attack;
                                attackCoolTime = 0;
                                wayPoints.Clear();
                                SetWayPosition();
                            }
                        }
                        else
                        {
                            AStar(MapManager.instance.PositionToBlockData(targetTransform.position), targetTransform);
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
                if (nextMovementTime <= 0)
                {
                    nextMovementTime = 5;
                    wayPoints.Clear();
                    SetWayPosition();
                }

                if (targetTransform != null)
                {
                    if (Vector3.Distance(transform.position, targetTransform.position) < attackDistance && attackCoolTime > 2)
                    {
                        if (!PlayerManager.instance.playerDead)
                        {
                            mobState = MobState.Attack;
                            attackCoolTime = 0;
                            wayPoints.Clear();
                            SetWayPosition();
                        }
                    }
                    else
                    {
                        animator.SetBool("Attack", false);
                    }
                }
            }
            else if(mobState == MobState.Attack)
            {
                if (Vector3.Distance(transform.position, targetTransform.position) < attackDistance)
                {
                    if (!PlayerManager.instance.playerDead)
                    {
                        animator.SetBool("Attack", true);
                        if (attackCoolTime > 2)
                        {
                            attackCoolTime = 0;
                            wayPoints.Clear();
                            SetWayPosition();
                        }
                        else
                        {
                            nextMovementTime = 5;
                            AStar_Random(1);
                            SetWayPosition();
                        }
                    }
                    else
                    {
                        animator.SetBool("Attack", false);
                        if (nextMovementTime <= 0)
                        {
                            nextMovementTime = 5;
                            AStar_Random();
                            SetWayPosition();
                        }
                    }
                }
                else
                {

                    if (target != null)
                    {
                        animator.SetBool("Attack", false);
                        targetTransform = target;
                        AStar(MapManager.instance.PositionToBlockData(target.position), target);
                        SetWayPosition();
                    }
                    else
                    {
                        animator.SetBool("Attack", false);
                        if (nextMovementTime <= 0)
                        {
                            nextMovementTime = 5;
                            AStar_Random();
                            SetWayPosition();
                        }
                    }
                }
            }

            Movement();
        }
    }
}