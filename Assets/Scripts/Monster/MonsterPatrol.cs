using UnityEngine;

public class MonsterPatrol : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float speed = 2f;
    public float waitTime = 1f; // jeda pas nyampe di patrol point

    private int currentPointIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    void Update()
    {
        if (patrolPoints.Length == 0) return;

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
        Vector2 direction = (targetPoint.position - transform.position);

        transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, speed * Time.deltaTime);

        // Flip sprite sesuai arah gerak (opsional, kalau monster punya sprite)
        if (direction.x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(direction.x), 1, 1);
        }

        // Kalau udah nyampe di patrol point, pindah ke point berikutnya
        if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            isWaiting = true;
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        }
    }

    // Buat lihat patrol points di Scene view (opsional tapi enak buat debug)
    void OnDrawGizmos()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] != null)
            {
                Gizmos.DrawSphere(patrolPoints[i].position, 0.1f);

                int nextIndex = (i + 1) % patrolPoints.Length;
                if (patrolPoints[nextIndex] != null)
                {
                    Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[nextIndex].position);
                }
            }
        }
    }
}