using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Radar : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float detectionAngle = 60f;
    [SerializeField] private LayerMask detectionLayer;
    private Enemy _enemyRef;
    private void Start()
    {
        _enemyRef = GetComponentInParent<Enemy>();
    }

    private void Update()
    {
        DetectObjects();
    }

    private void DetectObjects()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, detectionLayer);

        foreach (Collider hit in hits)
        {
            Vector3 directionToTarget = (hit.transform.position - transform.position).normalized;
            float angleBetween = Vector3.Angle(transform.forward, directionToTarget);

            if (angleBetween < detectionAngle / 2f)
            {
                RaycastHit hitInfo;
                Vector3 rayOrigin = transform.position + Vector3.up * 0.5f; // Adjust ray origin slightly above the ground to avoid downward casting

                if (Physics.Raycast(rayOrigin, directionToTarget, out hitInfo, detectionRadius, detectionLayer))
                {
                    if (hitInfo.transform == hit.transform)
                    {
                        _enemyRef.Player = hitInfo.transform;
                        _enemyRef.playerInSight = true;
                        GetComponentInParent<MeshRenderer>().material.color = Color.black;
                        return;
                    }
                }
            }
        }

        // If no player is detected
        //_enemyRef.Player = null;
        _enemyRef.playerInSight = false;
        GetComponentInParent<MeshRenderer>().material.color = Color.cyan;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Vector3 leftBoundary = Quaternion.Euler(0, -detectionAngle / 2f, 0) * transform.forward * detectionRadius;
        Vector3 rightBoundary = Quaternion.Euler(0, detectionAngle / 2f, 0) * transform.forward * detectionRadius;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }
}