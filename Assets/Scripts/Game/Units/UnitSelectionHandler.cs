using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionHandler : MonoBehaviour
{
    List<Unit> selectedUnits = new List<Unit>();
    [SerializeField] RectTransform dragBox;

    Vector2 startPosition;
    RTSPlayer player;

    #region Client

    void Start()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
    }

    void Update()
    {
        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartSelectionArea();



            //Start AOESelectorArea
        }

        else if(Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ClearSelectionArea();
        }

        else if(Mouse.current.leftButton.isPressed)
        {
            UpdateSelectionArea();
        }
    }

    void StartSelectionArea()
    {
        foreach (Unit selectedUnit in selectedUnits)
        {
            selectedUnit.Deselect();
        }

        selectedUnits.Clear();

        dragBox.gameObject.SetActive(true);

        startPosition = Mouse.current.position.ReadValue();

        UpdateSelectionArea();
    }

    void UpdateSelectionArea()
    {
        
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
