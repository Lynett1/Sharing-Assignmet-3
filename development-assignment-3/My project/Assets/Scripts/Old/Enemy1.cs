using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy1 : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private GameObject player;
    private Rigidbody rb;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private EnemySettings enemySettings;
    private float targetDistance;
    private float distance;
    private bool isChasing = false;
    private bool isAttacking = false;
    [SerializeField] private GameObject attack;
    private Coroutine attackCoroutine;
    private float HP;
    private float attackDuration;
    private bool isKnockedBack = false;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackDuration = 1f;
    

    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (enemySettings != null)
        {
            targetDistance = enemySettings.targetDistance;
            HP = enemySettings.HP;
            attackDuration = enemySettings.AttackDuration;
        }

        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }
    }

    void Update()
    {
        if (player == null || enemySettings == null) return;

        distance = Vector3.Distance(transform.position, player.transform.position);

        float attackRange = targetDistance + 2.0f; 

        if (distance <= attackRange)
        {
            if (!isAttacking)
            {
                isAttacking = true;
                isChasing = false;
                
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

                if (attackCoroutine != null) StopCoroutine(attackCoroutine);
                attackCoroutine = StartCoroutine(Attacking());
            }
        }
        else if (distance <= detectionRange)
        {
            isChasing = true;
            isAttacking = false;
            
            if (attackCoroutine != null) 
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
            if (attack != null) attack.SetActive(false);
        }
        else 
        {
            if (isChasing || isAttacking)
            {
                isChasing = false;
                isAttacking = false;
                
                if (attackCoroutine != null) 
                {
                    StopCoroutine(attackCoroutine);
                    attackCoroutine = null;
                }
                if (attack != null) attack.SetActive(false);
            }
        }
    }

    void FixedUpdate() 
    {
        if (isKnockedBack) return;

        if(isChasing || isAttacking)
        {
            Vector3 lookDirection = (player.transform.position - transform.position).normalized;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(-lookDirection);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f));
            }
        }

        if (isChasing) 
        {
            Vector3 direction = (player.transform.position - transform.position).normalized;
            direction.y = 0; // Keeping horizontal
            
            Vector3 newPos = rb.position + direction * speed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerAttack"))
        {
            Debug.Log("Enemy hit");
            
            Vector3 attackDirection = Vector3.zero;
            Rigidbody cubeRb = other.attachedRigidbody;
            
            // Use the cube's velocity direction if it has one, otherwise fallback to position difference
            if (cubeRb != null && cubeRb.linearVelocity.sqrMagnitude > 0.1f)
            {
                attackDirection = cubeRb.linearVelocity;
            }
            else
            {
                attackDirection = transform.position - other.transform.position;
            }

            attackDirection.y = 0; // Keep knockback horizontal
            if (attackDirection != Vector3.zero) 
            {
                attackDirection.Normalize();
            }
            
            Destroy(other.gameObject);
            
            StartCoroutine(ApplyKnockback(attackDirection));

            HP--;
            if (HP <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator ApplyKnockback(Vector3 direction)
    {
        isKnockedBack = true;
        
        // Reset velocity before applying knockback for consistency
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); 
        rb.AddForce(direction * knockbackForce, ForceMode.Impulse);
        
        yield return new WaitForSeconds(knockbackDuration);
        
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); 
        isKnockedBack = false;
    }

    private IEnumerator Attacking()
    {
        while (isAttacking)
        {
            yield return new WaitForSeconds(0.5f);
            
            if (!isAttacking) break;
            
            if (attack != null) 
            {
                attack.transform.position = player.transform.position;
                attack.SetActive(true);
            }
            
            yield return new WaitForSeconds(attackDuration);
            
            if (!isAttacking) break;
            
            if (attack != null) attack.SetActive(false);

            // Attack Cooldown
            yield return new WaitForSeconds(1f);
        }
        
        if (attack != null) attack.SetActive(false);
    }

}
