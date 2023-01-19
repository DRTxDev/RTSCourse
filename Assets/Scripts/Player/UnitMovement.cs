using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class UnitMovement : NetworkBehaviour 
{
    [SerializeField] NavMeshAgent navAgent;
    [SerializeField] float turnSpeed = 5f;

    Coroutine turnRoutine;
    Coroutine whileMoving;

    #region Server

    [Command]
    public void CmdMovePlayer(Vector3 targetPosition)
    {   
        if(!NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 1f, NavMesh.AllAreas)) return;

        SetTargetPosition(hit.position);
    }

    [Server]
    void SetTargetPosition(Vector3 targetPosition)
    {
        NavMeshPath path = new NavMeshPath();
        if(navAgent.CalculatePath(targetPosition, path))
        {
            Vector3[] firstCorner = path.corners;

            if(turnRoutine is not null)
                StopCoroutine(turnRoutine);
            if(whileMoving is not null)
                StopCoroutine(whileMoving);
            turnRoutine = StartCoroutine(TurnToNextCorner(firstCorner[1], turnSpeed));

            navAgent.destination = targetPosition;
        }
    }

    [Server]
    IEnumerator TurnToNextCorner(Vector3 target, float turnSpeed)
    {
        //disables navmesh control of rotation
        navAgent.updateRotation = false;

        //determines current y rotation of character is, and what the target y rotation will be by using lookat(target)
        float currentYRotation, targetYRotation;
        GetCurrentAndTargetYRotation(target, out currentYRotation, out targetYRotation);

        //takes both current and target y rotations, determines spread, and modifies the rotation if needed, to prevent rotating the wrong way
        //returns what the percentage of the movement is based on a 0 - 180 degree turn to maintain uniform turn speed regardless of how far the turn is
        float spreadPercent = GetAngleSpreadAndModifyRotation(ref currentYRotation, targetYRotation);

        //lerped rotation angle
        Vector3 targetYAngle = new Vector3();
        //lerp time variable
        float timeElapsed = 0;

        //basic lerp loop using turnspeed and spreadpercent variables as modifiers
        while ((timeElapsed * turnSpeed) / spreadPercent <= 1)
        {
            targetYAngle.y = Mathf.Lerp(currentYRotation, targetYRotation, (turnSpeed * timeElapsed) / spreadPercent);
            transform.eulerAngles = targetYAngle;
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        //sets the angle in case of minor discrepancies and returns rotation control back to navmesh
        transform.eulerAngles = targetYAngle;
        
        whileMoving = StartCoroutine(WhileMovingAdjustments());
    }

    [Server]
    static float GetAngleSpreadAndModifyRotation(ref float currentYRotation, float targetYRotation)
    {
        float spread = currentYRotation - targetYRotation;

        if (Mathf.Abs(spread) > 180 && spread > 0)
        {
            currentYRotation -= 360;
        }
        else if (Mathf.Abs(spread) > 180 && spread < 0)
        {
            currentYRotation += 360;
        }

        spread = currentYRotation - targetYRotation;
        return Mathf.Abs(spread / 180);
    }

    [Server]
    void GetCurrentAndTargetYRotation(Vector3 target, out float currentYRotation, out float targetYRotation)
    {
        currentYRotation = transform.eulerAngles.y;
        transform.LookAt(target, Vector3.up);
        targetYRotation = transform.eulerAngles.y;
    }

    [Server]
    IEnumerator WhileMovingAdjustments()
    {
        while(Vector3.Distance(transform.position, navAgent.destination) > navAgent.stoppingDistance)
        {
            transform.LookAt(transform.position + navAgent.velocity);
            yield return null;
        }

        Debug.Log("Completed");
    }

    #endregion

    #region Client

    #endregion
}
