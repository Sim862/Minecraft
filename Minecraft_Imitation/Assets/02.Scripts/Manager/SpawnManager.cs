using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance { get; private set; }

    private int monsterCount = 0;
    private int monsterMaxCount = 5;
    private Dictionary<MobData.MobKind, List<Mob>> mobList = new Dictionary<MobData.MobKind, List<Mob>>();

    public int passiveMobCount = 0;

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
    
    public void AddMob(Mob mob)
    {
        if (mobList.ContainsKey(mob.mobData.mobKind))
        {
            mobList[mob.mobData.mobKind].Add(mob);
        }
        else
        {
            mobList.Add(mob.mobData.mobKind, new List<Mob>() { mob });
        }

        passiveMobCount++;
    }

    public void RemoveMob(Mob mob)
    {
        mob.alive = false;
        if (mobList.ContainsKey(mob.mobData.mobKind))
        {
            mobList[mob.mobData.mobKind].Remove(mob);
        }

        passiveMobCount--;
        if(mob != null)
            Destroy(mob.gameObject);
    }

    
}
