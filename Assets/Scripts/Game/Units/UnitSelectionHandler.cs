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

    [Header("DragBox Settings")]
    [Tooltip("Minimum SizeDelta height or width size of DragBox before activation")]
    [SerializeField] float minimumSize = 0.1f;
    [Tooltip("Extra width and height of dragbox")]
    [SerializeField] float additionalAreaThreshold;
    Vector2 additionalAreaVector;

    #region Client

    void Start()
    {
        //player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        additionalAreaVector = new Vector2(additionalAreaThreshold, additionalAreaThreshold);
    }

    void Update()
    {
        if(player == null)
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartSelectionArea();
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
        if(!Input.GetKey(KeyCode.LeftShift))
        {
            foreach (Unit selectedUnit in selectedUnits)
            {
                selectedUnit.Deselect();
            }

            selectedUnits.Clear();
        }

        dragBox.gameObject.SetActive(true);

        startPosition = Mouse.current.position.ReadValue();

        UpdateSelectionArea();
    }

    void UpdateSelectionArea()
    {
        Vector2 currentMousePosition = Mouse.current.position.ReadValue();

        float areaWidth = currentMousePosition.x - startPosition.x;
        float areaHeight = currentMousePosition.y - startPosition.y;

        if(Mathf.Abs(areaHeight) <= minimumSize || 
            Mathf.Abs(areaWidth) <= minimumSize) return;

        dragBox.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        dragBox.anchoredPosition = startPosition + new Vector2(areaWidth / 2, areaHeight / 2);
    }

    void ClearSelectionArea()
    {
        dragBox.gameObject.SetActive(false);

        if (dragBox.sizeDelta.magnitude == 0)
        {
            ClickSelect();

            return;
        }

        ProcessSeclectionArea();

        dragBox.sizeDelta = Vector2.zero;
    }

    void ProcessSeclectionArea()
    {
        Vector2 min = dragBox.anchoredPosition - (dragBox.sizeDelta / 2) - additionalAreaVector;
        Vector2 max = dragBox.anchoredPosition + (dragBox.sizeDelta / 2) + additionalAreaVector;

        foreach (Unit unit in player.GetUnits)
        {
            if(selectedUnits.Contains(unit)) continue;
            
            Vector2 pos = Camera.main.WorldToScreenPoint(unit.transform.position);

            if (pos.x > min.x && pos.x < max.x && pos.y > min.y && pos.y < max.y)
            {
                selectedUnits.Add(unit);
                unit.Select();
            }
        }
    }

    void ClickSelect()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            hit.collider.TryGetComponent<Unit>(out Unit unit);

            if(unit is not null && unit.isOwned)
            {
                if(selectedUnits.Contains(unit))
                    selectedUnits.Remove(unit);
                else
                    selectedUnits.Add(unit);

                if(!unit.isSelected)
                    unit.Select();
                else
                    unit.Deselect();
            }
        }
    }

    public List<Unit> GetSelectedUnits()
    {
        return selectedUnits;
    }

    #endregion
}
