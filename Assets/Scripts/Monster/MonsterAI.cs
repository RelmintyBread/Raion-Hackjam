using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
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
    private bool waitingAtDoor = false;
    private float attackTimer = 0f;

    private int investigateIndex = 0;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    void Start()
    {
        if (patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[currentPointIndex].position);
    }

    void Update()
    {
        FlipSprite();

        switch (currentState)
        {
            case State.Patrol: TickPatrol(); break;
            case State.Attack: TickAttack(); break;
            case State.Investigate: TickInvestigate(); break;
        }
    }

    // ---------- DIPANGGIL LEWAT UNITY EVENT DI INSPECTOR ----------
    // Cara pakai: di tiap Room -> OnRoomPowerOut (+) -> drag GameObject Monster
    // -> pilih MonsterAI -> OnRoomWentDark -> ISI PARAMETER dengan Room yang sama
    public void OnRoomWentDark(Room room)
    {
        if (room == null) return;

        // Kalau lagi Attack/Investigate ke room lain, dan room baru ini lebih dekat -> ganti target.
        // Kalau lagi idle di Patrol -> langsung pergi.
        if (currentState == State.Patrol)
        {
            StartAttack(room);
            return;
        }

        if (currentState == State.Attack || currentState == State.Investigate)
        {
            float distCurrent = targetRoom != null ? Vector2.Distance(transform.position, targetRoom.transform.position) : float.MaxValue;
            float distNew = Vector2.Distance(transform.position, room.transform.position);

            if (distNew < distCurrent)
            {
                Debug.Log($"[MonsterAI] Room '{room.roomName}' lebih dekat, ganti target dari '{targetRoom?.roomName}'.");
                StartAttack(room);
            }
        }
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
            isWaiting = true;
    }

    // ---------- ATTACK ----------
    void StartAttack(Room room)
    {
        targetRoom = room;
        waitingAtDoor = false;
        attackTimer = 0f;
        currentState = State.Attack;

        Debug.Log($"[MonsterAI] -> Attack. Menuju pintu terdekat ruangan '{room.roomName}'.");

        targetDoor = GetNearestDoor(room);

        if (targetDoor != null)
            agent.SetDestination(targetDoor.transform.position);
        else
            Debug.LogWarning($"[MonsterAI] Room '{room.roomName}' tidak punya roomDoors yang di-assign!");
    }

    Door GetNearestDoor(Room room)
    {
        if (room.roomDoors == null || room.roomDoors.Length == 0)
            return null;

        Door nearest = null;
        float shortestDist = float.MaxValue;

        foreach (Door door in room.roomDoors)
        {
            if (door == null) continue;
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
        if (targetRoom == null || targetDoor == null)
        {
            ReturnToPatrol();
            return;
        }

        if (!waitingAtDoor)
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                waitingAtDoor = true;
                agent.isStopped = true;
                Debug.Log($"[MonsterAI] Sampai di pintu '{targetDoor.name}' ({targetRoom.roomName}). Menunggu {doorOpenDelay} detik.");
            }
            return;
        }

        attackTimer += Time.deltaTime;
        if (attackTimer >= doorOpenDelay)
        {
            Debug.Log($"[MonsterAI] Waktu habis! Membuka pintu '{targetDoor.name}'. Attack -> Investigate.");
            (targetDoor as IInteractable).Interact();
            agent.isStopped = false;
            StartInvestigate();
        }
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
            agent.SetDestination(targetRoom.investigatePoints[0].position);
            Debug.Log($"[MonsterAI] Masuk ke '{targetRoom.roomName}', memeriksa {targetRoom.investigatePoints.Length} titik.");
        }
        else
        {
            Debug.LogWarning($"[MonsterAI] Room '{targetRoom.roomName}' tidak punya investigatePoints, langsung kembali patrol.");
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

                agent.SetDestination(targetRoom.investigatePoints[investigateIndex].position);
            }
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.1f)
            isWaiting = true;
    }

    void ReturnToPatrol()
    {
        currentState = State.Patrol;
        agent.isStopped = false;
        targetRoom = null;
        targetDoor = null;
        waitingAtDoor = false;

        if (patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[currentPointIndex].position);
    }

    void FlipSprite()
    {
        if (Mathf.Abs(agent.velocity.x) > 0.05f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(agent.velocity.x) * Mathf.Abs(scale.x);
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
