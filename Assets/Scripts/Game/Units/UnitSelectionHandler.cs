using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionHandler : MonoBehaviour
{
    List<Unit> selectedUnits = new List<Unit>();

    #region Client

    void Update()
    {
        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            foreach(Unit selectedUnit in selectedUnits)
            {
                selectedUnit.Deselect();
            }

            selectedUnits.Clear();

            //Start AOESelectorArea
        }
        else if(Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ClearSelectionArea();
        }
    }

    void ClearSelectionArea()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            hit.collider.TryGetComponent<Unit>(out Unit unit);

            if(unit is not null && unit.isOwned)
            {
                selectedUnits.Add(unit);
            }

            foreach(Unit selectedUnit in selectedUnits)
            {
                selectedUnit.Select();
            }
        }
    }

    public List<Unit> GetSelectedUnits()
    {
        return selectedUnits;
    }

    #endregion
}
