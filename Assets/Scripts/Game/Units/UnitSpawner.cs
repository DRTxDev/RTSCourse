using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UnitSpawner : NetworkBehaviour
{
    NetworkManager network;

    [SerializeField] Transform spawnLocation = null;

    #region Server

    [Command]
    void CmdSpawnUnit()
    {
        GameObject unit = Instantiate(FindObjectOfType<NetworkManager>().spawnPrefabs[1], spawnLocation.position, Quaternion.identity);
        SpawnUnit(unit);
    }

    [Server]
    void SpawnUnit(GameObject unit)
    {
        NetworkServer.Spawn(unit, connectionToClient);
    }


    #endregion

    #region Client

    [ClientCallback]
    void Update()
    {
        if(isOwned && Input.GetKeyDown(KeyCode.W))
        {
            CmdSpawnUnit();
        }
    }

    void Start() 
    {
        if(!isOwned) return;

        network = FindObjectOfType<NetworkManager>();
    }

    #endregion
}
