using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerInput : NetworkBehaviour
{
    NetworkManager network;

    #region Server

    [Command]
    void CmdSpawnBase()
    {
        GameObject playerBase = Instantiate(FindObjectOfType<NetworkManager>().spawnPrefabs[0], transform.position, Quaternion.identity);
        SpawnBase(playerBase);
    }

    [Server]
    void SpawnBase(GameObject playerBase)
    {
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

    [ClientCallback]
    void Update()
    {
        if(!isOwned) return;

        if(Input.GetMouseButtonDown(1))
        {
            
        }
    }

    #endregion
}
