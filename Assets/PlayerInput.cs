using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerInput : NetworkBehaviour
{
    NetworkManager network;
    GameObject playerBase;

    #region Server

    [Command]
    void CmdSpawnBase()
    {
        SpawnBase();
    }

    [Server]
    void SpawnBase()
    {
        Debug.Log(network);
        playerBase = Instantiate(network.spawnPrefabs[0], transform.position, Quaternion.identity);
        NetworkServer.Spawn(playerBase, gameObject);
    }

    #endregion


    #region Client

    void Start() 
    {
        if(!isOwned) return;

        network = FindObjectOfType<NetworkManager>();
        CmdSpawnBase();
    }

    #endregion
}
