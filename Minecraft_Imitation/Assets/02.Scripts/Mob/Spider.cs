using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider : Mob
{

    public Transform temp;

    private float idleSoundTimer = 0;
    private float attackCoolTime = 0;

    public float damage = -5;
    private void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    protected override void LifeCycle()
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

                if (targetTransform == null)
                {
                    FindVisibleTargets();
                }

                if (targetTransform != null)
                {
                    if (Vector3.Distance(transform.position, targetTransform.position) < 10)
                    {

                        if (Vector3.Distance(transform.position, targetTransform.position) < 1)
                        {
                            if (attackCoolTime > 2 && !PlayerManager.instance.playerDead)
                            {
                                mobState = MobState.Attack;
                                attackCoolTime = 0;
                                targetTransform.GetComponent<PlayerMove>().UpdateHP(-1);
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
                    if (Vector3.Distance(transform.position, targetTransform.position) < 1 && attackCoolTime > 2)
                    {
                        if (!PlayerManager.instance.playerDead)
                        {
                            mobState = MobState.Attack;
                            attackCoolTime = 0;
                            targetTransform.GetComponent<PlayerMove>().UpdateHP(damage);
                            wayPoints.Clear();
                            SetWayPosition();
                        }
                    }
                }
            }
            else if(mobState == MobState.Attack)
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
                else
                {

                    if (target != null)
                    {
                        targetTransform = target;
                        AStar(MapManager.instance.PositionToBlockData(target.position), target);
                        SetWayPosition();
                    }
                    else
                    {
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