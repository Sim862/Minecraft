using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager playerManager;

    public PlayerSample playerPrefab;
    public PlayerSample player;

    public bool spawnPlayer = false;

    // Update is called once per frame
    void Update()
    {
        PlayerSpawn();
    }

    public void PlayerSpawn()
    {
        if (player != null)
            return;

        if (spawnPlayer == true)
        { 
            spawnPlayer = false;
            player = Instantiate(playerPrefab, DataManager.instance.playerData.spawnPosition, Quaternion.identity,transform);
        }
    }
}
