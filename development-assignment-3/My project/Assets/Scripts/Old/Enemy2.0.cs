using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy2 : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private GameObject player;
    private Rigidbody rb;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private EnemySettings enemySettings;
    private float targetDistance;
    private float distance;
    private bool isChasing = false;
    private float HP;
    private float attackDuration;
    private bool isKnockedBack = false;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackDuration = 1f;

    [SerializeField] private float groundCheckLength = 1.1f;
    private bool isGrounded;

    // Events for the Manager to listen to
    public static event Action OnTargetLockAcquired;
    public static event Action OnTargetLockLost;
    
    private bool isLockedOn = false;
    private Coroutine behaviorCoroutine;

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

        behaviorCoroutine = StartCoroutine(EnemyBehaviorLoop());
    }

    private void OnDestroy()
    {
        if (isLockedOn)
        {
            // If enemy is destroyed while locked on, successfully free the lock!
            OnTargetLockLost?.Invoke();
        }
    }

    private IEnumerator EnemyBehaviorLoop()
    {
        WaitForSeconds waitTime = new WaitForSeconds(0.1f);

        while (true)
        {
            if (player == null || enemySettings == null || isKnockedBack)
            {
                yield return waitTime;
                continue;
            }

            // Only evaluate distance if we aren't currently locked on.
            // (Rule: Target lock remains in place even if player moves out of distance)
            if (!isLockedOn)
            {
                distance = Vector3.Distance(transform.position, player.transform.position);

                if (distance <= targetDistance)
                {
                    // Acquire target lock
                    isLockedOn = true;
                    isChasing = false;
                    rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); // Stop horizontal movement
                    
                    OnTargetLockAcquired?.Invoke();
                    
                    // Branch into jumping/locked routine
                    yield return StartCoroutine(HandleTargetLockRoutine());
                }
                else if (distance <= detectionRange)
                {
                    isChasing = true;
                }
                else
                {
                    isChasing = false;
                }
            }

            yield return waitTime;
        }
    }

    private IEnumerator HandleTargetLockRoutine()
    {
        float timer = 0f;
        float jumpInterval = 0.6f;
        float nextJump = 0f;

        while (timer < attackDuration) // Uses AttackDuration from ScriptableObject
        {
            if (!isKnockedBack && isGrounded && timer >= nextJump)
            {
                // Start jumping up and down
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // Reset Y velocity for clean hop
                rb.AddForce(Vector3.up * 4f, ForceMode.Impulse);
                nextJump = timer + jumpInterval;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        isLockedOn = false;
        OnTargetLockLost?.Invoke();
    }

    void FixedUpdate() // For movement and rotation
    {
        CheckGroundEnemy();

        if (isKnockedBack) return;

        if (isChasing)
        {
            Vector3 lookDirection = (player.transform.position - transform.position).normalized;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(-lookDirection);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f));
            }

            Vector3 direction = (player.transform.position - transform.position).normalized;
            direction.y = 0;
            Vector3 newPos = rb.position + direction * speed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
        }
        else if (isLockedOn)
        {
            // Only rotate to face player while jumping, no movement
            Vector3 lookDirection = (player.transform.position - transform.position).normalized;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(-lookDirection);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f));
            }
        }
    }

    private void CheckGroundEnemy()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckLength);
        Debug.DrawRay(transform.position, Vector3.down * groundCheckLength, isGrounded ? Color.green : Color.red);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerAttack"))
        {
            Debug.Log("Enemy hit");
            Vector3 attackDirection = Vector3.zero;
            Rigidbody cubeRb = other.attachedRigidbody;
            
            if (cubeRb != null && cubeRb.linearVelocity.sqrMagnitude > 0.1f)
            {
                attackDirection = cubeRb.linearVelocity;
            }
            else
            {
                attackDirection = transform.position - other.transform.position;
            }

            attackDirection.y = 0;

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
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); 
        rb.AddForce(direction * knockbackForce, ForceMode.Impulse);
        
        yield return new WaitForSeconds(knockbackDuration);
        
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); 
        isKnockedBack = false;
    }

}
