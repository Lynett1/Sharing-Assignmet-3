using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private Transform player;
    [SerializeField] private float followRange = 15f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private Transform[] waypoints;

    private NavMeshAgent agent;
    private int currentWaypointIndex = 0;
    private bool isBlinded = false;
    private string currentState = "";

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        string targetState = "Patrol";

        if (isBlinded)
        {
            targetState = "Blind";
        }
        else if (distance <= attackRange)
        {
            targetState = "Attack";
        }
        else if (distance <= followRange)
        {
            targetState = "Follow";
        }

        if (currentState != targetState) // Should work
        {
            currentState = targetState;
            Debug.Log($"[Enemy State] Transitioning to: {targetState}");
            CustomEvent.Trigger(gameObject, "To" + targetState);
        }
        
        isBlinded = false;
    }

    public void Patrol()
    {
        agent.isStopped = false;
        agent.speed = patrolSpeed;

        if (waypoints.Length == 0) return;

        agent.SetDestination(waypoints[currentWaypointIndex].position);

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    public void Follow()
    {
        agent.isStopped = false;
        agent.speed = followSpeed;
        agent.SetDestination(player.position);
    }

    public void Attack()
    {
        agent.isStopped = true;
        Debug.Log("Attacking the player!");
    }

    public void Blind()
    {
        agent.isStopped = true;
    }

    public void SetBlinded(bool state)
    {
        isBlinded = state;
    }
}

