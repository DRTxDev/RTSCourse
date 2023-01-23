using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Targeter : NetworkBehaviour
{
    Targetable target;

    public bool hasTarget
    {
        get
        {
            return target != null;
        }
    }

    #region Server

    [Command]
    public void CmdSetTarget(GameObject targetGameObject)
    {
        if(!targetGameObject.TryGetComponent<Targetable>(out var target)) return;

        this.target = target;
    }

    [Server]
    public void ClearTarget()
    {
        target = null;
    }

    #endregion

    #region Client

    #endregion
}
