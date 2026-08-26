using UnityEngine;

public enum MonsterState
{
    Patrol,
    TryOpen
}

public class MonsterPatrol : MonoBehaviour
{
    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float speed = 2f;
    public float waitTime = 1f;

    [Header("Try Open")]
    public Transform[] doorTargets;
    public float darknessBeforeTryOpen = 10f;
    public float arriveDistance = 0.4f;
    public float approachDistance = 0f;

    [Header("Lights")]
    public Room[] rooms;

    private MonsterState currentState = MonsterState.Patrol;
    private int currentPointIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private float darknessTimer = 0f;
    private bool hasStartedOpening = false;
    private Transform currentDoorTarget;
    private Door currentDoor;
    private Rigidbody2D rb;
    private Collider2D monsterCollider;
    private bool colliderWasTrigger;

    public MonsterState CurrentState => currentState;
    public bool IsOpeningDoor => hasStartedOpening;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        monsterCollider = GetComponent<Collider2D>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }
    }

    void Start()
    {
        if (rooms == null || rooms.Length == 0)
        {
            PowerControlSystem power = FindFirstObjectByType<PowerControlSystem>();
            if (power != null)
                rooms = power.rooms;
        }

        if (doorTargets == null || doorTargets.Length == 0)
        {
            Door[] foundDoors = FindObjectsByType<Door>(FindObjectsSortMode.None);
            doorTargets = new Transform[foundDoors.Length];
            for (int i = 0; i < foundDoors.Length; i++)
                doorTargets[i] = foundDoors[i].transform;
        }

        EnsureDoorsHaveScript();
    }

    void EnsureDoorsHaveScript()
    {
        if (doorTargets == null)
            return;

        foreach (Transform hinge in doorTargets)
        {
            if (hinge == null)
                continue;

            if (GetDoorOn(hinge) == null)
                hinge.gameObject.AddComponent<Door>();
        }
    }

    void Update()
    {
        UpdateDarknessTimer();

        switch (currentState)
        {
            case MonsterState.Patrol:
                TickPatrol();
                break;
            case MonsterState.TryOpen:
                TickTryOpen();
                break;
        }
    }

    void UpdateDarknessTimer()
    {
        if (AnyLightOn())
        {
            darknessTimer = 0f;
            if (currentState == MonsterState.TryOpen)
                ChangeState(MonsterState.Patrol);
            return;
        }

        darknessTimer += Time.deltaTime;
        if (currentState == MonsterState.Patrol && darknessTimer >= darknessBeforeTryOpen)
            ChangeState(MonsterState.TryOpen);
    }

    void ChangeState(MonsterState nextState)
    {
        if (currentState == nextState)
            return;

        ExitState(currentState);
        currentState = nextState;
        EnterState(currentState);
    }

    void EnterState(MonsterState state)
    {
        switch (state)
        {
            case MonsterState.Patrol:
                isWaiting = false;
                waitTimer = 0f;
                Debug.Log("Monster: Patrol");
                break;
            case MonsterState.TryOpen:
                isWaiting = false;
                waitTimer = 0f;
                hasStartedOpening = false;
                SetPassThroughWalls(true);
                PickClosestDoor();
                Debug.Log("Monster: lampu mati 10 detik, mencoba membuka pintu.");
                break;
        }
    }

    void ExitState(MonsterState state)
    {
        if (state == MonsterState.TryOpen)
        {
            hasStartedOpening = false;
            currentDoorTarget = null;
            currentDoor = null;
            SetPassThroughWalls(false);
        }
    }

    void SetPassThroughWalls(bool passThrough)
    {
        if (monsterCollider == null)
            return;

        if (passThrough)
        {
            colliderWasTrigger = monsterCollider.isTrigger;
            monsterCollider.isTrigger = true;
        }
        else
        {
            monsterCollider.isTrigger = colliderWasTrigger;
        }
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

    void TickTryOpen()
    {
        if (currentDoorTarget == null)
            PickClosestDoor();

        if (currentDoorTarget == null || hasStartedOpening)
            return;

        Vector3 dest = currentDoorTarget.position;
        MoveTowards(dest);

        if (Vector2.Distance(transform.position, dest) <= arriveDistance)
            StartOpeningDoor();
    }

    void PickClosestDoor()
    {
        currentDoorTarget = null;
        currentDoor = null;

        if (doorTargets == null)
            return;

        float bestDist = float.MaxValue;
        foreach (Transform hinge in doorTargets)
        {
            if (hinge == null)
                continue;

            Door door = GetDoorOn(hinge);
            if (door != null && door.IsOpen)
                continue;

            float dist = Vector2.Distance(transform.position, hinge.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                currentDoorTarget = hinge;
                currentDoor = door;
            }
        }
    }

    Door GetDoorOn(Transform hinge)
    {
        return hinge.GetComponent<Door>()
            ?? hinge.GetComponentInParent<Door>()
            ?? hinge.GetComponentInChildren<Door>();
    }

    void StartOpeningDoor()
    {
        hasStartedOpening = true;

        if (currentDoor == null && currentDoorTarget != null)
            currentDoor = GetDoorOn(currentDoorTarget);

        if (currentDoor != null)
            currentDoor.TryOpen(monsterCollider);
        else
            Debug.Log("Monster: sampai di pintu, tapi tidak ada script Door.");
    }

    void MoveTowards(Vector3 target)
    {
        Vector2 next = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (rb != null)
            rb.position = next;
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

    bool AnyLightOn()
    {
        if (rooms == null)
            return false;

        foreach (Room room in rooms)
        {
            if (room != null && room.IsAnyLightOn())
                return true;
        }

        return false;
    }

    void OnDrawGizmos()
    {
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
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

        if (doorTargets == null)
            return;

        foreach (Transform hinge in doorTargets)
        {
            if (hinge == null)
                continue;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(hinge.position, 0.12f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(hinge.position, arriveDistance);
            Gizmos.DrawLine(transform.position, hinge.position);
        }
    }
}
