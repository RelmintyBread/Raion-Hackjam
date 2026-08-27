using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class FamilyMember : MonoBehaviour
{
    public Room room;
    public UnityEvent OnKilled;
    public string monsterTag = "Enemy";

    bool dead;

    public bool IsAlive => !dead;

    void Reset()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    void Start()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;

        if (room == null)
            room = GetComponentInParent<Room>();

        if (room == null)
            room = FindNearestRoom();
    }

    Room FindNearestRoom()
    {
        Room nearest = null;
        float best = float.MaxValue;
        foreach (Room r in FindObjectsByType<Room>())
        {
            if (r == null) continue;
            float d = Vector2.Distance(transform.position, r.transform.position);
            if (d < best)
            {
                best = d;
                nearest = r;
            }
        }
        return nearest;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(monsterTag))
            Kill();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag(monsterTag))
            Kill();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(monsterTag))
            Kill();
    }

    public void Kill()
    {
        if (dead) return;
        dead = true;
        OnKilled?.Invoke();

        Room r = room;
        Vector3 pos = transform.position;
        Destroy(gameObject);

        foreach (FamilyMember member in FindObjectsByType<FamilyMember>())
        {
            if (member == null || member == this || !member.IsAlive) continue;

            bool sameRoom = (r != null && member.IsInRoom(r))
                || Vector2.Distance(member.transform.position, pos) < 8f;
            if (sameRoom)
                member.Kill();
        }

        if (r != null && !HasAliveInRoom(r))
            r.monsterCleared = true;
    }

    public bool BelongsTo(Room target)
    {
        if (target == null) return false;
        if (room == target) return true;
        return transform.IsChildOf(target.transform);
    }

    public bool IsInRoom(Room target)
    {
        if (target == null) return false;
        if (BelongsTo(target)) return true;
        if (room != null && room != target) return false;
        return Vector2.Distance(transform.position, target.transform.position) < 10f;
    }

    public static bool HasAliveInRoom(Room room)
    {
        if (room == null) return false;
        foreach (FamilyMember member in FindObjectsByType<FamilyMember>())
        {
            if (member != null && member.IsAlive && member.IsInRoom(room))
                return true;
        }
        return false;
    }
}
