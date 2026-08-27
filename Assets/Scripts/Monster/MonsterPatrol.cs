using UnityEngine;

/// <summary>
/// Plain waypoint patrol. Door interaction is no longer handled here -
/// DoorSensor.cs opens doors automatically when the monster walks into its
/// trigger zone, so this script just needs to move the monster around.
/// </summary>
public class MonsterPatrol : MonoBehaviour
{
    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float speed = 2f;
    public float waitTime = 1f;

    private int currentPointIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }
    }

    void Update()
    {
        TickPatrol();
    }

    void TickPatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                isWaiting = false;
                waitTimer = 0f;
            }
            return;
        }

        Transform targetPoint = patrolPoints[currentPointIndex];
        if (targetPoint == null)
            return;

        MoveTowards(targetPoint.position);

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            isWaiting = true;
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        }
    }

    void MoveTowards(Vector3 target)
    {
        Vector2 next = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (rb != null)
            rb.MovePosition(next);
        else
            transform.position = next;

        Vector2 direction = (Vector2)target - next;
        if (Mathf.Abs(direction.x) > 0.01f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(direction.x) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    void OnDrawGizmos()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        Gizmos.color = Color.red;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null)
                continue;

            Gizmos.DrawSphere(patrolPoints[i].position, 0.1f);

            int nextIndex = (i + 1) % patrolPoints.Length;
            if (patrolPoints[nextIndex] != null)
                Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[nextIndex].position);
        }
    }
}
