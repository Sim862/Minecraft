using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance { get; private set; }

    private bool mobKindCheck = false;
    public MobData.MobKind[] passiveMonsterList = new MobData.MobKind[] { MobData.MobKind.Pig, };
    public MobData.MobKind[] hostileeMonsterList = new MobData.MobKind[] { MobData.MobKind.Spider, MobData.MobKind.Skeleton_Arrow};
    public int passiveMonsterCount { get; private set; } = 0;
    public int hostileeMonsterCount { get; private set; } = 0;
    public int monsterMaxCount { get; set; } = 10;
    private Dictionary<MobData.MobKind, List<Mob>> mobList = new Dictionary<MobData.MobKind, List<Mob>>();

    private float random_Radian;
    private Vector3 spawnDirection;
    private Vector3 playerPosition;
    private PositionData spawnPositionData;
    private IEnumerator Cor_spawnMob;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Cor_spawnMob = null;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public void AddMob(Mob mob)
    {
        mobKindCheck = false;
        if (mobList.ContainsKey(mob.mobData.mobKind))
        {
            mobList[mob.mobData.mobKind].Add(mob);
        }
        else
        {
            mobList.Add(mob.mobData.mobKind, new List<Mob>() { mob });
        }

        for (int i = 0; i < passiveMonsterList.Length; i++)
        {
            if(mob.mobData.mobKind == passiveMonsterList[i])
            {
                mobKindCheck = true;
                passiveMonsterCount++;
                break;
            }
        }

        if (!mobKindCheck)
        {
            hostileeMonsterCount++;
        }

    }

    public void RemoveMob(Mob mob)
    {
        mob.alive = false;
        mobKindCheck = false;
        if (mobList.ContainsKey(mob.mobData.mobKind))
        {
            mobList[mob.mobData.mobKind].Remove(mob);
        }

        for (int i = 0; i < passiveMonsterList.Length; i++)
        {
            if (mob.mobData.mobKind == passiveMonsterList[i])
            {
                mobKindCheck = true;
                passiveMonsterCount--;
            }
        }
        if (!mobKindCheck)
        {
            hostileeMonsterCount--;
        }


        if (mob != null)
            Destroy(mob.gameObject);
    }

    public void StartSpawnMob()
    {
        if (Cor_spawnMob == null)
        {
            Cor_spawnMob = SpawnCycle();
            StartCoroutine(Cor_spawnMob);
        }
    }

    IEnumerator SpawnCycle()
    {
        while (GameManager.instance.gameStart)
        {
            if (passiveMonsterCount + hostileeMonsterCount < monsterMaxCount)
            {
                random_Radian = Mathf.Deg2Rad * (Random.Range(120f, 250f) + PlayerManager.instance.player.transform.eulerAngles.y);
                spawnDirection = new Vector3(Mathf.Cos(random_Radian), 0, Mathf.Sin(random_Radian));
                playerPosition = PlayerManager.instance.player.transform.position;
                spawnPositionData = MapManager.instance.PositionToBlockData(new Vector3(playerPosition.x, 0, playerPosition.z) + (spawnDirection * Random.Range(10, 13)));

                if (spawnPositionData.chunk != null)
                {
                    spawnPositionData = MapManager.instance.GetSpawnPositionY(spawnPositionData);
                    if (GameManager.instance.day > 0)
                    {
                        if (GameManager.instance.time < 180)
                        {
                            if (Random.value < 0.7f)
                            {
                                MapManager.instance.SpawnMonster(passiveMonsterList[Random.Range(0, passiveMonsterList.Length)], spawnPositionData);
                            }
                            else
                            {
                                MapManager.instance.SpawnMonster(hostileeMonsterList[Random.Range(0, hostileeMonsterList.Length)], spawnPositionData);
                            }
                        }
                        else
                        {
                            MapManager.instance.SpawnMonster(hostileeMonsterList[Random.Range(0, hostileeMonsterList.Length)], spawnPositionData);
                        }
                    }
                    else
                    {
                        MapManager.instance.SpawnMonster(passiveMonsterList[Random.Range(0, passiveMonsterList.Length)], spawnPositionData);
                    }

                }
            }

            yield return new WaitForSeconds(8);
        }
        Cor_spawnMob = null;
    }
}
