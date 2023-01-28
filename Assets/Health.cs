using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

public class Health : NetworkBehaviour
{
    [SerializeField] float maxHealth = 100f;
    [SerializeField] float currentHealth;

    Animator animator;
    

    bool isAlive = true;
    public bool IsAlive { get { return isAlive; } }

    public event Action ServerOnDamagedEvent;
    public event Action ServerOnDeathEvent;

    [Server]
    public override void OnStartServer()
    {
        animator = GetComponent<Animator>();
        SetCurrentHealthTo(maxHealth);
    }

    [Server]
    void SetCurrentHealthTo(float maxHealth)
    {
        currentHealth = maxHealth;
        isAlive = true;
    }

    [Server]
    public bool ReduceHealthBy(float damageAmount)
    {
        currentHealth -= damageAmount;

        if(currentHealth <= 0)
        {
            Die();
            return true;
        }

        ServerOnDamagedEvent?.Invoke();
        //onDamagedAnimation
        //onDamageEffects -- dust effect at random place on mesh? collider? 
        return false;
    } 

    [Server]
    void Die()
    {
        animator.SetTrigger("die");
        ServerOnDeathEvent?.Invoke();
    }



}
