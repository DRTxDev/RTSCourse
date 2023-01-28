using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class UnitMovement : NetworkBehaviour 
{
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

    NavMeshAgent navAgent;
    NetworkAnimator netAnimator;
    Targeter targeter;
    Attack attack;
    Coroutine turnRoutine;
    Coroutine moveRoutine;
    Coroutine storedAction;
    NavMeshPath path;

    GameObject target;

    float capsuleColliderRadius;
    bool isRooted;
    bool canMove => !isRooted;

    #region Server

    public override void OnStartServer()
    {
        navAgent = GetComponent<NavMeshAgent>();
        netAnimator = GetComponent<NetworkAnimator>();
        attack = GetComponent<Attack>();
        targeter = GetComponent<Targeter>();
        path = new NavMeshPath();
        capsuleColliderRadius = GetComponent<CapsuleCollider>().radius;
    }

    [Command]
    public void CmdMoveUnit(Vector3 targetPosition)
    {   
        targeter.ClearTarget();
        target = null;

        if(!NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 0.2f, NavMesh.AllAreas)) return;

        if(isRooted)
        {
            if(storedAction is not null)
                StopCoroutine(storedAction);

            storedAction = StartCoroutine(StoreMoveAction(hit.position));

            return;
        }

        BeginMove(hit.position);
    }

    [Command]
    public void CmdChaseTarget(GameObject target)
    {
        if(!NavMesh.SamplePosition(target.transform.position, out NavMeshHit hit, 0.2f, NavMesh.AllAreas)) return;
        if(target is null) return;
        
        SetTarget(target);

        if(isRooted)
        {
            if(storedAction is not null)
                StopCoroutine(storedAction);

            storedAction = StartCoroutine(StoreMoveAction(hit.position));

            return;
        }

        BeginMove(hit.position);
    }

    [Server]
    void SetTarget(GameObject target)
    {
        this.target = target;
    }

    [Server]
    float GetStoppingDistance()
    {
        if(targeter.hasTarget)
            return attackRange;
        else
            return baseStoppingDistance;
    }

    [Server]
    void BeginMove(Vector3 targetPosition)
    {
        navAgent.stoppingDistance = GetStoppingDistance();

        StopMovementCoroutines();

        moveRoutine = StartCoroutine(CommitMove(targetPosition));
    }

    [Server]
    IEnumerator CommitMove(Vector3 targetPosition)
    {
        float timeElapsed = 1;
        navAgent.destination = targetPosition;
        float squaredDistanceToDestination;
        float squaredStoppingDistance = navAgent.stoppingDistance * navAgent.stoppingDistance;
        Vector3 moveDestination;
        bool isTargeting = targeter.hasTarget;

        while(isTargeting == targeter.hasTarget)
        {
            moveDestination = targeter.hasTarget? target.transform.position : targetPosition;
            squaredDistanceToDestination = (transform.position - moveDestination).sqrMagnitude;
            timeElapsed += Time.deltaTime;
            netAnimator.animator.SetFloat("speed", navAgent.velocity.magnitude);

            if(squaredDistanceToDestination > squaredStoppingDistance)
            {
                if(timeElapsed > 0.1f)
                {
                    navAgent.destination = moveDestination;
                    timeElapsed = 0;
                }
            }

            else if(targeter.hasTarget)
            {
                transform.LookAt(target.transform);

                bool isAttacking = attack.TryAttack(target);

                //wait until attack is finished to reduce overhead
            }

            else
            {
                navAgent.ResetPath();
                netAnimator.animator.SetFloat("speed", 0f);
                yield break;
            }

            transform.LookAt(transform.position + navAgent.velocity);

            if(isRooted)
                yield return new WaitWhile(()=> isRooted);
                
            yield return null;
        }

        if(!targeter.hasTarget)
            navAgent.ResetPath();
    }

    //disabled. needs updating or deletion
    #region TurnToNextCorner

    // [Server]
    // IEnumerator TurnToNextCorner(Vector3 target)
    // {
    //     //disables navmesh control of rotation
    //     navAgent.updateRotation = false;

    //     //determines current y rotation of character is, and what the target y rotation will be by using lookat(target)
    //     float currentYRotation, targetYRotation;
    //     GetCurrentAndTargetYRotation(target, out currentYRotation, out targetYRotation);

    //     //takes both current and target y rotations, determines spread, and modifies the rotation if needed, to prevent rotating the wrong way
    //     //returns what the percentage of the movement is based on a 0 - 180 degree turn to maintain uniform turn speed regardless of how far the turn is
    //     float spreadPercent = GetAngleSpreadAndModifyRotation(ref currentYRotation, targetYRotation);

    //     //lerped rotation angle
    //     Vector3 targetYAngle = new Vector3();
    //     //lerp time variable
    //     float timeElapsed = 0;

    //     //basic lerp loop using turnspeed and spreadpercent variables as modifiers
    //     while ((timeElapsed * turnSpeed) / spreadPercent <= 1)
    //     {
    //         targetYAngle.y = Mathf.Lerp(currentYRotation, targetYRotation, (turnSpeed * timeElapsed) / spreadPercent);
    //         transform.eulerAngles = targetYAngle;
    //         timeElapsed += Time.deltaTime;
    //         yield return null;
    //     }

    //     //sets the angle in case of minor discrepancies
    //     transform.eulerAngles = targetYAngle;
    // }

    // [Server]
    // static float GetAngleSpreadAndModifyRotation(ref float currentYRotation, float targetYRotation)
    // {
    //     float spread = currentYRotation - targetYRotation;

    //     if (Mathf.Abs(spread) > 180 && spread > 0)
    //     {
    //         currentYRotation -= 360;
    //     }
    //     else if (Mathf.Abs(spread) > 180 && spread < 0)
    //     {
    //         currentYRotation += 360;
    //     }

    //     spread = currentYRotation - targetYRotation;
    //     return Mathf.Abs(spread / 180);
    // }

    // [Server]
    // void GetCurrentAndTargetYRotation(Vector3 target, out float currentYRotation, out float targetYRotation)
    // {
    //     currentYRotation = transform.eulerAngles.y;
    //     transform.LookAt(target, Vector3.up);
    //     targetYRotation = transform.eulerAngles.y;
    // }

    #endregion

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

    [Server]
    IEnumerator RootPlayer(float duration)
    {
        isRooted = true;
        navAgent.isStopped = true;

        //Start Root Effect

        yield return new WaitForSeconds(duration);

        //End Root Effect

        isRooted = false;
        navAgent.isStopped = false;
    }

    [Server]
    IEnumerator StoreMoveAction(Vector3 position)
    {
        yield return new WaitUntil(()=> canMove);

        //if still is holding action

        CommitMove(position);
    }

    [Server]
    void StopMovementCoroutines()
    {   
        if(moveRoutine is not null)
            StopCoroutine(moveRoutine);
        if(turnRoutine is not null)
            StopCoroutine(turnRoutine);
    }

    #endregion

    #region Client

    #endregion


}
