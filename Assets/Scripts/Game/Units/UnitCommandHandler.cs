using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class UnitCommandHandler : MonoBehaviour
{
    UnitSelectionHandler unitSelectionHandler;
    [SerializeField] GroupMovementPoints groupMovement;
    [SerializeField] float stoppingDistanceModifierPerUnit = .01f;   


    #region Client

    void Awake()
    {
        unitSelectionHandler = GetComponent<UnitSelectionHandler>();
    }

    [ClientCallback]
    void Update() 
    {
        if(Mouse.current.rightButton.wasPressedThisFrame)
        {
            if(!Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out RaycastHit hit)) return;

            List<Unit> units = unitSelectionHandler.GetSelectedUnits();
            List<Transform> movementPoints = groupMovement.GetMovementPoints(units.Count);

            for(int i = 0; i < units.Count; i++)
            {
                units[i].GetComponent<NavMeshAgent>().avoidancePriority = 50 - i;
                units[i].GetComponent<NavMeshAgent>().stoppingDistance = (stoppingDistanceModifierPerUnit * i);
                units[i].GetComponent<UnitMovement>().CmdMovePlayer(hit.point + movementPoints[i].position);
            }

            // foreach(Unit unit in unitSelectionHandler.GetSelectedUnits())
            // {
            //     unit.GetComponent<UnitMovement>().CmdMovePlayer(hit.point);
            // }
        }
    }

    #endregion
}
