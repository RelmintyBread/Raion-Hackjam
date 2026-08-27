using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MonsterAI : MonoBehaviour
{
    private enum State { Patrol, Attack, Investigate }

    [Header("Patrol (lorong, titik 1-4)")]
    public Transform[] patrolPoints;
    public Room[] rooms;
    public float waitTime = 1f;
    [Header("Attack Points (dalam ruangan)")]
    public Transform[] attackPoints; // sebelumnya investigatePoints

    [Header("Attack")]
    public float doorOpenDelay = 10f;

    [Header("Investigate (dalam ruangan)")]
    public float investigateWaitTime = 1f;

    private NavMeshAgent agent;
    private State currentState = State.Patrol;

    private int currentPointIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    private Room targetRoom;
    private Door targetDoor;
    private FamilyMember targetVictim;
    private bool enteredRoom;

    private int investigateIndex = 0;
    private float[] darkTimers;
    private float stuckTimer;
    private float actionTimer;
    private bool canHunt = true;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.autoBraking = false;
        agent.radius = 0.2f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    void Start()
    {
        if (rooms == null || rooms.Length == 0)
            rooms = FindObjectsByType<Room>();

        darkTimers = new float[rooms.Length];

        if (patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[currentPointIndex].position);
    }

    void Update()
    {
        FlipSprite();
        CheckDarkRooms();

        switch (currentState)
        {
            case State.Patrol: TickPatrol(); break;
            case State.Attack: TickAttack(); break;
            case State.Investigate: TickInvestigate(); break;
        }
    }

    // Lampu mati selama doorOpenDelay detik -> Attack ke ruangan gelap terdekat
    void CheckDarkRooms()
    {
        if (rooms == null || darkTimers == null) return;

        if (targetRoom != null && targetRoom.monsterCleared && !FamilyMember.HasAliveInRoom(targetRoom))
            ReturnToPatrol();

        if (!canHunt) return;

        Room nearest = null;
        float nearestDist = float.MaxValue;

        for (int i = 0; i < rooms.Length; i++)
        {
            Room room = rooms[i];
            if (room == null || room.monsterCleared) continue;
            if (!FamilyMember.HasAliveInRoom(room)) continue;

            if (room.IsRoomDark())
            {
                darkTimers[i] += Time.deltaTime;
                if (darkTimers[i] < doorOpenDelay) continue;

                float dist = Vector2.Distance(transform.position, room.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = room;
                }
            }
            else
            {
                darkTimers[i] = 0f;
            }
        }

        if (nearest == null || nearest == targetRoom) return;

        // Jangan ganti target di tengah jalan — itu bikin monster ke-tarik Room 1 <-> Room 2.
        if (currentState != State.Patrol || targetRoom != null) return;

        Debug.Log($"[MonsterAI] Lampu '{nearest.roomName}' mati {doorOpenDelay}s, ruangan terdekat. Patrol -> Attack.");
        StartAttack(nearest);
    }

    // ---------- DIPANGGIL LEWAT UNITY EVENT DI INSPECTOR ----------
    // Cara pakai: di tiap Room -> OnRoomPowerOut (+) -> drag GameObject Monster
    // -> pilih MonsterAI -> OnRoomWentDark -> ISI PARAMETER dengan Room yang sama
    public void OnRoomWentDark(Room room)
    {
        if (room == null || room.monsterCleared || !FamilyMember.HasAliveInRoom(room)) return;

        // Kalau lagi Attack/Investigate ke room lain, dan room baru ini lebih dekat -> ganti target.
        // Kalau lagi idle di Patrol -> langsung pergi.
        if (currentState == State.Patrol && canHunt)
            StartAttack(room);
    }

    // ---------- PATROL ----------
    void TickPatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                isWaiting = false;
                waitTimer = 0f;
                currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[currentPointIndex].position);
            }
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.1f)
        {
            isWaiting = true;
            canHunt = true;
        }
    }

    // ---------- ATTACK ----------
    void StartAttack(Room room)
    {
        targetRoom = room;
        targetVictim = null;
        enteredRoom = false;
        isWaiting = false;
        stuckTimer = 0f;
        actionTimer = 0f;
        currentState = State.Attack;
        agent.isStopped = false;

        EnableAgent();
        targetDoor = GetNearestDoor(room);

        Transform inside = GetInsidePoint(room);
        if (inside != null)
            agent.SetDestination(inside.position);
        else if (targetDoor != null)
            agent.SetDestination(targetDoor.transform.position);

        Debug.Log($"[MonsterAI] -> Attack. Masuk '{room.roomName}'.");
    }

    Transform GetInsidePoint(Room room)
    {
        FamilyMember victim = FindVictimInRoom(room);
        if (victim != null)
            return victim.transform;
        return GetAttackPoint(room);
    }

    Transform GetAttackPoint(Room room)
    {
        if (room.investigatePoints != null && room.investigatePoints.Length > 0 && room.investigatePoints[0] != null)
            return room.investigatePoints[0];

        if (attackPoints != null && attackPoints.Length > 0)
            return attackPoints[0];

        return null;
    }

    Door GetNearestDoor(Room room)
    {
        if (room.roomDoors == null || room.roomDoors.Length == 0)
            return null;

        Door nearest = null;
        float shortestDist = float.MaxValue;

        foreach (Door door in room.roomDoors)
        {
            if (door == null || !door.gameObject.activeInHierarchy) continue;
            float dist = Vector2.Distance(transform.position, door.transform.position);
            if (dist < shortestDist)
            {
                shortestDist = dist;
                nearest = door;
            }
        }

        return nearest;
    }

    void TickAttack()
    {
        if (targetRoom == null)
        {
            ReturnToPatrol();
            return;
        }

        actionTimer += Time.deltaTime;
        if (actionTimer > 10f)
        {
            Debug.Log($"[MonsterAI] Attack '{targetRoom.roomName}' selesai (timeout). Attack -> Patrol.");
            ReturnToPatrol();
            return;
        }

        TryOpenNearbyDoor();

        if (targetVictim == null || !targetVictim.IsAlive)
            targetVictim = FindVictimInRoom(targetRoom);

        if (targetVictim != null)
        {
            MoveTo(targetVictim.transform.position);
            return;
        }

        // Masih menuju ruangan — jangan batal Attack cuma karena belum ketemu korban.
        Transform inside = GetAttackPoint(targetRoom);
        if (!enteredRoom)
        {
            if (inside != null)
                MoveTo(inside.position);
            else if (targetDoor != null)
                MoveTo(targetDoor.transform.position);
            return;
        }

        Debug.Log($"[MonsterAI] Tidak ada korban di '{targetRoom.roomName}'. Attack -> Patrol.");
        ReturnToPatrol();
    }

    void TryOpenNearbyDoor()
    {
        if (targetDoor == null || enteredRoom) return;
        if (Vector2.Distance(transform.position, targetDoor.transform.position) > 2.5f) return;

        targetDoor.Interact();
        enteredRoom = true;
    }

    void MoveTo(Vector3 dest)
    {
        dest.z = transform.position.z;
        float dist = Vector2.Distance(transform.position, dest);
        if (dist < 0.12f) return;

        bool notMoving = !agent.pathPending && agent.velocity.sqrMagnitude < 0.08f;
        if (notMoving)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > 0.15f)
            {
                agent.enabled = false;
                transform.position = Vector3.MoveTowards(transform.position, dest, 3.5f * Time.deltaTime);
                return;
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        if (agent.enabled && agent.isOnNavMesh)
        {
            if ((agent.destination - dest).sqrMagnitude > 0.15f)
                agent.SetDestination(dest);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, dest, 3.5f * Time.deltaTime);
        }
    }

    void EnableAgent()
    {
        if (!agent.enabled)
            agent.enabled = true;
        if (agent.isOnNavMesh)
            agent.isStopped = false;
    }

    void ResetRoomTimer(Room room)
    {
        if (rooms == null || darkTimers == null || room == null) return;
        for (int i = 0; i < rooms.Length; i++)
        {
            if (rooms[i] == room)
                darkTimers[i] = 0f;
        }
    }

    FamilyMember FindVictimInRoom(Room room)
    {
        if (room == null) return null;

        FamilyMember nearest = null;
        float nearestDist = float.MaxValue;

        foreach (FamilyMember member in FindObjectsByType<FamilyMember>())
        {
            if (member == null || !member.IsAlive || !member.IsInRoom(room)) continue;

            float dist = Vector2.Distance(transform.position, member.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = member;
            }
        }

        return nearest;
    }

    // ---------- INVESTIGATE ----------
    void StartInvestigate()
    {
        currentState = State.Investigate;
        investigateIndex = 0;
        isWaiting = false;
        waitTimer = 0f;

        if (targetRoom.investigatePoints != null && targetRoom.investigatePoints.Length > 0)
        {
            Debug.Log($"[MonsterAI] Masuk ke '{targetRoom.roomName}', memeriksa {targetRoom.investigatePoints.Length} titik.");
        }
        else
        {
            ReturnToPatrol();
        }
    }

    void TickInvestigate()
    {
        if (targetRoom == null || targetRoom.investigatePoints == null || targetRoom.investigatePoints.Length == 0)
        {
            ReturnToPatrol();
            return;
        }

        actionTimer += Time.deltaTime;
        if (actionTimer > 8f)
        {
            ReturnToPatrol();
            return;
        }

        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= investigateWaitTime)
            {
                isWaiting = false;
                waitTimer = 0f;
                investigateIndex++;

                if (investigateIndex >= targetRoom.investigatePoints.Length)
                {
                    Debug.Log($"[MonsterAI] Selesai memeriksa '{targetRoom.roomName}'. Investigate -> Patrol.");
                    ReturnToPatrol();
                    return;
                }

            }
            return;
        }

        Transform point = targetRoom.investigatePoints[investigateIndex];
        if (point != null && Vector2.Distance(transform.position, point.position) < 0.2f)
            isWaiting = true;
        else if (point != null)
            MoveTo(point.position);
    }

    void ReturnToPatrol()
    {
        ResetRoomTimer(targetRoom);
        canHunt = false;
        currentState = State.Patrol;
        EnableAgent();
        targetRoom = null;
        targetDoor = null;
        targetVictim = null;
        enteredRoom = false;
        isWaiting = false;
        waitTimer = 0f;

        if (patrolPoints.Length > 0 && agent.enabled && agent.isOnNavMesh)
            agent.SetDestination(patrolPoints[currentPointIndex].position);
    }

    void FlipSprite()
    {
        float vx = agent.enabled ? agent.velocity.x : 0f;
        if (Mathf.Abs(vx) < 0.05f && targetVictim != null)
            vx = targetVictim.transform.position.x - transform.position.x;
        if (Mathf.Abs(vx) > 0.05f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(vx) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    void OnDrawGizmos()
    {
        // Titik-titik patrol
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] == null) continue;
                Gizmos.DrawSphere(patrolPoints[i].position, 0.15f);
                int next = (i + 1) % patrolPoints.Length;
                if (patrolPoints[next] != null)
                    Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[next].position);
            }
        }

        // Status text di atas kepala Monster - INI YANG PALING PENTING
#if UNITY_EDITOR
    string status = Application.isPlaying
        ? $"STATE: {currentState}\nTarget: {(targetRoom != null ? targetRoom.roomName : "none")}\nRooms assigned: {(rooms != null ? rooms.Length : 0)}"
        : "(Play mode untuk lihat status)";

    GUIStyle style = new GUIStyle();
    style.normal.textColor = Color.yellow;
    style.fontSize = 14;
    style.fontStyle = FontStyle.Bold;
    Handles.Label(transform.position + Vector3.up * 1.5f, status, style);
#endif

        // Garis ke target room aktif
        if (Application.isPlaying && targetRoom != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, targetRoom.transform.position);
            Gizmos.DrawWireSphere(targetRoom.transform.position, 0.4f);
        }
    }
}
