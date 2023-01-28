using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using Mirror;

public class Attack : NetworkBehaviour
{
    NetworkAnimator netAnimator;
    Targeter targeter;
    [SerializeField] float attackDamage = 10f;

    //Temporary
    [SerializeField] float attackSpeedModifier = 2f;
    float attackSpeed;

    float timeSinceLastAttack = 0;

    GameObject currentTarget;

    [Server]
    public override void OnStartServer()
    {
        targeter = GetComponent<Targeter>();
        netAnimator = GetComponent<NetworkAnimator>();

        Debug.Log(netAnimator.animator.runtimeAnimatorController.animationClips[0].name);
    }

    [Server]
    public bool TryAttack(GameObject target)
    {
        if(CanAttack())
        {
            SetTarget(target);
            netAnimator.SetTrigger("attack");
            return true;
        }

        return false;
    }

    [Server]
    bool CanAttack()
    {
        if((Time.time - timeSinceLastAttack) > attackSpeedModifier)
        {
            timeSinceLastAttack = Time.time;
            return true;
        }
        else
            return false;

    }

    [Server]
    void Hit()
    {
        if(currentTarget is null) return;

        if(currentTarget.TryGetComponent<Health>(out var targetHealth))
        {
            bool isDead = targetHealth.ReduceHealthBy(attackDamage);
            if(isDead)
            {
                ClearTarget();
                netAnimator.ResetTrigger("attack");
            }
                
        }
    }

    [Server]
    void SetTarget(GameObject target)
    {
        currentTarget = target;
    }

    [Server]
    void ClearTarget()
    {
        targeter.ClearTarget();
        currentTarget = null;
    }

    [Server]
    void AttackFinished()
    {
        Debug.Log("Finished Attack");
    }

}   
