using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RTSPlayer : NetworkBehaviour
{

    List<Unit> myUnits = new List<Unit>();

    public List<Unit> GetUnits { get { return myUnits; } }

    #region Server

    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawn += ServerAddUnitToList;
        Unit.ServerOnUnitDespawn += ServerRemoveUnitFromList;
    }

    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawn -= ServerAddUnitToList;
        Unit.ServerOnUnitDespawn -= ServerRemoveUnitFromList;
    }

    void ServerAddUnitToList(Unit unit)
    {
        if(unit.connectionToClient.connectionId != connectionToClient.connectionId) return;
        myUnits.Add(unit);
    }
    
    void ServerRemoveUnitFromList(Unit unit)
    {
        if(unit.connectionToClient.connectionId != connectionToClient.connectionId) return;
        myUnits.Remove(unit);
    }

    #endregion

    #region Client

    public override void OnStartClient()
    {
        if(!isClientOnly) return;

        Unit.AuthorityOnUnitSpawn += AuthorityAddUnitToList;
        Unit.AuthorityOnUnitDespawn += AuthorityRemoveUnitFromList;
    }

    public override void OnStopClient()
    {
        if(!isClientOnly) return;

        Unit.AuthorityOnUnitSpawn -= AuthorityAddUnitToList;
        Unit.AuthorityOnUnitDespawn -= AuthorityRemoveUnitFromList;
    }

    void AuthorityAddUnitToList(Unit unit)
    {
        if(!isOwned) return;

        myUnits.Add(unit);
    }
    
    void AuthorityRemoveUnitFromList(Unit unit)
    {
        if(!isOwned) return;

        myUnits.Remove(unit);
    }

    #endregion
}
