using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitCommandHandler : MonoBehaviour
{
    UnitSelectionHandler unitSelectionHandler;

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

            Debug.Log("Command");

            foreach(Unit unit in unitSelectionHandler.GetSelectedUnits())
            {
                Debug.Log("Command");
                unit.GetComponent<UnitMovement>().CmdMovePlayer(hit.point);
            }
        }
    }

    #endregion
}
