using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitSpawnEventHandler : NetworkBehaviour
{
    event Action spawnUnit;

    [SyncVar (hook = nameof(RpcUpdateTimeDisplay))] int gameTime;

    #region Server

    void OnServerInitialized() 
    {
        StartCoroutine(GlobalUnitSpawner());
    }


    [Server]
    IEnumerator GlobalUnitSpawner()
    {
        float gameTimerFloat = 0;
        int gameTimerInt = 0;

        while(true)
        {
            gameTimerFloat += Time.deltaTime;

            if(gameTimerFloat >= gameTimerInt + 1)
            {
                gameTimerInt = (int)gameTimerFloat;

            }

            yield return null;
        }
    }

    [ClientRpc]
    void RpcUpdateTimeDisplay(int oldTime, int newTime)
    {

    }

    #endregion

}
