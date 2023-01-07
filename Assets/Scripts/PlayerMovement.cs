using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour 
{
    NavMeshAgent navAgent;

    #region Server

    [Server]
    void SetTargetPosition(Vector3 targetPosition)
    {
        navAgent.destination = targetPosition;
    }

    [Command]
    void CmdMovePlayer(Vector3 targetPosition)
    {
        if(!NavMesh.SamplePosition(targetPosition,out NavMeshHit hit, 1f, NavMesh.AllAreas)) return;

        SetTargetPosition(hit.position);
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        navAgent = GetComponent<NavMeshAgent>();
    }

    [ClientCallback]
    void Update() 
    {
        if(!isOwned) return;

        if(Mouse.current.press.wasPressedThisFrame)
        {
            if(!Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out RaycastHit hit)) return;

            CmdMovePlayer(hit.point);
        }
    }

    #endregion
}
