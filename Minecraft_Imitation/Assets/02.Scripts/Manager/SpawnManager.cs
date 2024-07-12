using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance { get; private set; }

    private int monsterCount = 0;
    private int monsterMaxCount = 5;
    private Dictionary<MobData.MobKind, List<Mob>> mobList = new Dictionary<MobData.MobKind, List<Mob>>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void CreateMob(Mob mob)
    {
        if (mobList.ContainsKey(mob.mobData.mobKind))
        {
            mobList[mob.mobData.mobKind].Add(mob);
        }
        else
        {
            mobList.Add(mob.mobData.mobKind, new List<Mob>() { mob });
        }
    }

    public void RemoveMob(Mob mob)
    {
        if (mobList.ContainsKey(mob.mobData.mobKind))
        {
            mobList[mob.mobData.mobKind].Remove(mob);
        }

        Destroy(mob.gameObject);
    }

    
}
