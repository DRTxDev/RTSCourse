using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class UnitMovement : NetworkBehaviour 
{
    [SerializeField] NavMeshAgent navAgent;

    #region Server

    [Server]
    void SetTargetPosition(Vector3 targetPosition)
    {
        navAgent.destination = targetPosition;
    }

    [Command]
    public void CmdMovePlayer(Vector3 targetPosition)
    {
        if(!NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 1f, NavMesh.AllAreas)) return;

        SetTargetPosition(hit.position);
    }

    #endregion

    #region Client

    #endregion
}
