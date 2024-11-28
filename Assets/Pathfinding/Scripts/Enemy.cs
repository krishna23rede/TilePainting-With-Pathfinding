using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum EnemyState { IdleMoving, Investigating, Shooting }
    public EnemyState currentState = EnemyState.IdleMoving;

    public float idleSpeed = 2.0f;
    public float idleMoveTime = 3.0f;
    public float investigateTime = 3.0f;
    private float idleMoveTimer;
    private NavMeshAgent navMeshAgent;
    public Dictionary<Vector2Int, Vector3> gridIndex;
    private Vector3 targetPosition;
    public Transform Player;
    public bool playerInSight = false;
    public MeshRenderer radar;

    public float idleStoppingDistance = 1.5f;
    public float investigatingStoppingDistance = 1.0f;

    void Start()
    {
        // Initialize the gridIndex with the grid script's dictionary
        Grid gridScript = FindObjectOfType<Grid>();
        gridIndex = gridScript.gridIndex;
        radar = transform.GetChild(0).GetComponent<MeshRenderer>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.autoBraking = true;
        // Start in IdleMoving state and pick the first target
        currentState = EnemyState.IdleMoving;
        PickRandomTargetPosition();
        idleMoveTimer = idleMoveTime;
    }

    void Update()
    {
        // Check if Player transform is not null and switch to Investigating state
        if (Player != null)
        {
            SetState(EnemyState.Investigating);
        }

        switch (currentState)
        {
            case EnemyState.IdleMoving:
                IdleMoving();
                break;
            case EnemyState.Investigating:
                Investigating();
                break;
            case EnemyState.Shooting:
                Shooting();
                break;
        }
    }

    void IdleMoving()
    {
        navMeshAgent.speed = idleSpeed;
        navMeshAgent.stoppingDistance = idleStoppingDistance;

        // Move towards the target position
        navMeshAgent.SetDestination(targetPosition);

        RotateTowards(targetPosition);
        // Check if the enemy has reached the target position
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            idleMoveTimer -= Time.deltaTime;
            if (idleMoveTimer <= 0)
            {
                PickRandomTargetPosition();
                idleMoveTimer = idleMoveTime;
            }
        }
    }

    public void Investigating()
    {
        navMeshAgent.speed = 8f;
        navMeshAgent.updateRotation = true;
        navMeshAgent.angularSpeed = 300f;
        if (Player != null && playerInSight == false)
        {
            StartCoroutine(EmptyPLayer());
            // Move towards the player's position, stopping at the specified distance
            targetPosition = Player.position;
            Vector3 adjustedTargetPosition = CalculateAdjustedTargetPosition(targetPosition, 2f);
            navMeshAgent.SetDestination(adjustedTargetPosition);
            RotateTowards(targetPosition);
        }
        else
        {
            SetState(EnemyState.IdleMoving);
        }
    }

    void Shooting()
    {
        if(Player != null && playerInSight)
        {
            Debug.Log("Shoot ...");
        }
        // Implement your hunting logic here
    }

    void PickRandomTargetPosition()
    {
        // Pick a random point from the gridIndex
        List<Vector3> positions = new List<Vector3>(gridIndex.Values);
        if (positions.Count > 0)
        {
            targetPosition = positions[Random.Range(0, positions.Count)];
        }
        else
        {
            Debug.LogWarning("No positions available in gridIndex.");
        }
    }

    public void SetState(EnemyState newState)
    {
        currentState = newState;
        Debug.Log("Changed state to: " + newState);
    }
    Vector3 CalculateAdjustedTargetPosition(Vector3 targetPos, float stopDist)
    {
        // Calculate the direction from the agent to the target
        Vector3 direction = targetPos - transform.position;

        // Ensure the direction vector has a magnitude greater than the stop distance
        if (direction.magnitude > stopDist)
        {
            // Normalize the direction and scale it to the desired stop distance
            direction = direction.normalized * (direction.magnitude - stopDist);

            // Return the new target position
            return transform.position + direction;
        }

        // If the distance to the target is less than the stop distance, return the current position
        return transform.position;
    }
    void RotateTowards(Vector3 targetPos)
    {
        // Calculate the direction to the target
        Vector3 direction = (targetPos - transform.position).normalized;

        // Calculate the target rotation
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

        // Smoothly rotate towards the target
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * navMeshAgent.angularSpeed / 100f);
    }
    private IEnumerator EmptyPLayer()
    {
        yield return new WaitForSeconds(5f);
        Player = null;
    }
}