using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class Unit : NetworkBehaviour
{
    [SerializeField] UnityEvent onSelection = null;
    [SerializeField] UnityEvent onDeselection = null;

    [SerializeField] UnitMovement unitMovement = null;
    [SerializeField] Targeter targeter = null;

    public static event Action<Unit> ServerOnUnitSpawn;
    public static event Action<Unit> ServerOnUnitDespawn;

    public static event Action<Unit> AuthorityOnUnitSpawn;
    public static event Action<Unit> AuthorityOnUnitDespawn;

    public bool isSelected = false;

    public UnitMovement GetUnitMovement()
    {
        return unitMovement;
    }

    public Targeter GetTargeter()
    {
        return targeter;
    }

    #region Server

    public override void OnStartServer()
    {
        ServerOnUnitSpawn?.Invoke(this);
    }

    public override void OnStopServer()
    {
        ServerOnUnitDespawn?.Invoke(this);
    }

    #endregion

    #region Client

    [Client]
    public void Select()
    {   
        if(!isOwned) return;

        onSelection?.Invoke();
        isSelected = true;
    }

    [Client]
    public void Deselect()
    {
        if(!isOwned) return;

        onDeselection?.Invoke();
        isSelected = false;
    }

    public override void OnStartClient()
    {
        if(!isOwned || !isClientOnly) return;

        AuthorityOnUnitSpawn?.Invoke(this);
    }

    public override void OnStopClient()
    {
        if(!isOwned || !isClientOnly) return;
        AuthorityOnUnitDespawn?.Invoke(this);
    }

    #endregion
}
