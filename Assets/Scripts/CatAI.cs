using UnityEngine;
using Mirror;

public class CatAI : MonoBehaviour
{
    public Transform[] waypoints;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float detectionRange = 5f;
    public Transform player;
    public float rotationSpeed = 5f;

    private int currentWaypointIndex = 0;
    private bool isChasing = false;
    private bool playerInSafeZone = false;

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (!playerInSafeZone && distanceToPlayer < detectionRange)
        {
            isChasing = true;
        }
        else if (distanceToPlayer > detectionRange * 1.5f || playerInSafeZone)
        {
            isChasing = false;
        }

        if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        transform.position += transform.forward * patrolSpeed * Time.deltaTime;

        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.2f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    void ChasePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        transform.position += transform.forward * chaseSpeed * Time.deltaTime;
    }

    // Bu kısımları ekle: Güvenli bölge kontrolü
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SafeZone"))
        {
            playerInSafeZone = true;
        }
    
        if (other.CompareTag("Player"))
        {
            NetworkIdentity identity = other.GetComponent<NetworkIdentity>();
            if (identity != null && identity.connectionToClient != null)
            {
                MouseController mouse = other.GetComponent<MouseController>();
                if (mouse != null)
                {
                    mouse.CaughtByCat(transform.Find("MouthPoint").gameObject);
                }
            }
        }
    
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SafeZone"))
        {
            playerInSafeZone = false;
        }
    }


   

}


