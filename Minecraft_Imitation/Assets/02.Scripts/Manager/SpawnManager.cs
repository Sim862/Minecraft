using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager spawnManager;

    private int monsterCount = 0;
    private int monsterMaxCount = 5;


    private void Awake()
    {
        if (spawnManager == null)
        {
            spawnManager = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    
}
