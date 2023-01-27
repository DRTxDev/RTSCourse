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
        SetHealth(maxHealth);
    }

    [Server]
    void SetHealth(float maxHealth)
    {
        currentHealth = maxHealth;
        isAlive = true;
    }

    [Server]
    public void DamageHealth(float damageAmount)
    {
        currentHealth -= damageAmount;

        if(currentHealth <= 0)
        {
            Die();
            return;
        }

        ServerOnDamagedEvent?.Invoke();
        //onDamagedAnimation
        //onDamageEffects -- dust effect at random place on mesh? collider? 
    } 

    void Die()
    {
        animator.SetTrigger("die");
        ServerOnDeathEvent?.Invoke();
    }



}
