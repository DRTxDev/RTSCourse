using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UnitSpawner : NetworkBehaviour
{
    NetworkManager network;

    GameObject unit;

    #region Client

    [ClientCallback]
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.W))
        {
            unit = Instantiate(network.spawnPrefabs[1], transform.position, Quaternion.identity);
            NetworkServer.Spawn(unit, gameObject);
        }
    }

    void Start() 
    {
        if(!isOwned) return;

        network = FindObjectOfType<NetworkManager>();
    }

    #endregion
}
