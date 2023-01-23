using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class UnitMovement : NetworkBehaviour 
{
    [SerializeField] NavMeshAgent navAgent;
    [SerializeField] float baseStoppingDistance = 0.025f;
    [SerializeField] float turnSpeed = 5f;

    [Header("Strength of the push when character objects collide")]
    [SerializeField] float forceStrength;
    [Tooltip("Push force multiplier if colliding unit does not belong to the same player")]
    [SerializeField] float enemyForceMultiplier = 2f;
    [Tooltip("Push force multiplier when either moving to attack, or while attacking")]
    [SerializeField] float combatStanceMultipler = 0.25f;

    [Header("Temp Attack Range")]
    [SerializeField] float attackRange = 0.1f;

    Targeter targeter;
    Coroutine turnRoutine;
    Coroutine whileMoving;

    float capsuleColliderRadius;

    #region Server

    public override void OnStartServer()
    {
        targeter = GetComponent<Targeter>();
        capsuleColliderRadius = GetComponent<CapsuleCollider>().radius;
    }

    [Command]
    public void CmdMoveUnit(Vector3 targetPosition)
    {   
        targeter.ClearTarget();

        if(!NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 0.2f, NavMesh.AllAreas)) return;

        navAgent.stoppingDistance = baseStoppingDistance;
        BeginMoveToPosition(hit.position);
    }

    [Server]
    void BeginMoveToPosition(Vector3 targetPosition)
    {
        NavMeshPath path = new NavMeshPath();
        if(navAgent.CalculatePath(targetPosition, path))
        {
            Vector3[] firstCorner = path.corners;

            if(turnRoutine is not null)
                StopCoroutine(turnRoutine);
            if(whileMoving is not null)
                StopCoroutine(whileMoving);
            if(firstCorner[1] != null)
                turnRoutine = StartCoroutine(TurnToNextCorner(firstCorner[1], turnSpeed));
            else
                whileMoving = StartCoroutine(WhileMovingAdjustments());

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

        //sets the angle in case of minor discrepancies
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
        if(targeter.hasTarget)
            navAgent.stoppingDistance = attackRange;

        while((transform.position - navAgent.destination).sqrMagnitude > navAgent.stoppingDistance * navAgent.stoppingDistance)
        {
            transform.LookAt(transform.position + navAgent.velocity);
            yield return null;
        }

        navAgent.ResetPath();
    }

    [ServerCallback]
    void OnTriggerStay(Collider other) 
    {   
        float distance = Vector3.Distance(transform.position, other.transform.position);
        float forceModifier = (capsuleColliderRadius * 2) - distance;

        if(other.TryGetComponent<UnitMovement>(out var otherUnit))
        {
            if(!otherUnit.isOwned)
                forceModifier = forceModifier * enemyForceMultiplier;

            if(otherUnit.TryGetComponent<Targeter>(out var otherUnitTargeter))
            {
                if(otherUnitTargeter.hasTarget)
                    forceModifier = forceModifier / (combatStanceMultipler * 1.5f);
            }
        }

        if(targeter.hasTarget)
        {
            forceModifier = forceModifier * combatStanceMultipler;
        }

        if(forceModifier < 0) forceModifier = 0;

        navAgent.Move((transform.position - other.transform.position).normalized * forceModifier * forceStrength);
    }

    #endregion

    #region Client

    #endregion
}
