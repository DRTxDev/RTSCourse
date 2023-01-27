using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Attack : NetworkBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] float attackDamage = 10f;

    //Temporary
    [SerializeField] float attackSpeed = 2f;
    float timeSinceLastAttack = 0;
    bool attackInProgress;

    GameObject currentTarget;

    [Server]
    public void TryAttack(GameObject target)
    {
        if(!attackInProgress && CanAttack())
        {

            animator.SetTrigger("attack");
        }
    }

    [Server]
    bool CanAttack()
    {
        if((Time.time - timeSinceLastAttack) > attackSpeed)
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
            targetHealth.DamageHealth(attackDamage);
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
        currentTarget = null;
    }

}   
