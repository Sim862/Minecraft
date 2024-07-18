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

    public Arrow prefab_Arrow;
    public Transform firePosition;

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
                    if (Vector3.Distance(transform.position, targetTransform.position) < attackDistance)
                    {
                        if (attackCoolTime > 2 && !PlayerManager.instance.playerDead)
                        {
                            attackCoolTime = 2;
                            mobState = MobState.Attack;
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
                    if (nextMovementTime <= 0 && targetTransform == null)
                    {
                        nextMovementTime = 5;
                        AStar_Random();
                        SetWayPosition();
                    }
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
                    if (Vector3.Distance(transform.position, targetTransform.position) < attackDistance)
                    {
                        if (!PlayerManager.instance.playerDead && attackCoolTime > 2)
                        {
                            attackCoolTime = 2;
                            mobState = MobState.Attack;
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
                if (targetTransform != null && !PlayerManager.instance.playerDead)
                {
                    if (Vector3.Distance(transform.position, targetTransform.position) < attackDistance)
                    {
                        if (!PlayerManager.instance.playerDead)
                        {
                            animator.SetBool("Attack", true);
                            if (attackCoolTime > 3)
                            {
                                Arrow arrow = Instantiate(prefab_Arrow, firePosition.position, transform.rotation, SpawnManager.instance.transform);
                                arrow.Fire(targetTransform.position - transform.position, 0.5f);
                                attackCoolTime = 0;
                                wayPoints.Clear();
                                SetWayPosition();
                            }
                            else if(attackCoolTime < 2)
                            {
                                nextMovementTime = 5;
                                AStar_Random(1);
                                SetWayPosition();
                            }
                        }
                    }
                    else
                    {
                        animator.SetBool("Attack", false);
                        AStar(MapManager.instance.PositionToBlockData(targetTransform.position), targetTransform);
                        SetWayPosition();
                    }
                }
                else
                {
                    animator.SetBool("Attack", false);
                    nextMovementTime = 5;
                    AStar_Random();
                    SetWayPosition();
                }
            }
            Rotation();
            Movement();
        }
    }
}