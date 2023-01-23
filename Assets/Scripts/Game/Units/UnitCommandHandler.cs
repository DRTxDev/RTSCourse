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
            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out RaycastHit hit)) return;

            List<Unit> units = unitSelectionHandler.GetSelectedUnits();

            if(hit.collider.TryGetComponent<Targetable>(out var targetable) && !targetable.isOwned)
            {
                TryTarget(targetable.gameObject, units);

                return;
            }

            TryMove(hit.point, units);
        }
    }

    void TryMove(Vector3 target, List<Unit> units)
    {
        List<Transform> movementPoints = groupMovement.GetMovementPoints(units.Count);

        for (int i = 0; i < units.Count; i++)
        {
            units[i].GetUnitMovement().CmdMoveUnit(target + movementPoints[i].position);
        }
    }

    void TryTarget(GameObject target, List<Unit> units)
    {
        foreach(Unit unit in units)
        {
            unit.GetTargeter().CmdSetTarget(target);
        }
    }

    #endregion
}
