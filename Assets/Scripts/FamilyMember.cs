using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class FamilyMember : MonoBehaviour
{
    public Room room;
    public UnityEvent OnKilled;
    public string monsterTag = "Enemy";
    [Tooltip("Kalau true, membunuh orang lain di ruangan yang sama (ayah+ibu). Matikan di Player.")]
    public bool chainKill = true;

    public enum DeathEnding { Player, Adik, OrangTua }
    [Tooltip("Ending yang dipicu kalau orang ini mati")]
    public DeathEnding deathEnding = DeathEnding.Player;

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

        if (IsPlayerObject())
            chainKill = false;

        if (room == null)
            room = GetComponentInParent<Room>();

        if (room == null && chainKill)
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

        CutsceneManager cutscene = FindAnyObjectByType<CutsceneManager>();
        if (cutscene != null)
            cutscene.PlayEnding(ResolveDeathEnding());

        Room r = room;
        Destroy(gameObject);

        if (chainKill && r != null)
        {
            foreach (FamilyMember member in FindObjectsByType<FamilyMember>())
            {
                if (member == null || member == this || !member.IsAlive) continue;
                if (!member.chainKill) continue;
                if (member.room != r) continue;
                member.Kill();
            }

            if (!HasAliveInRoom(r))
                r.monsterCleared = true;
        }
    }

    public DeathEnding ResolveDeathEnding()
    {
        string n = gameObject.name;
        if (ContainsIgnoreCase(n, "Adik")) return DeathEnding.Adik;
        if (ContainsIgnoreCase(n, "Ayah") || ContainsIgnoreCase(n, "Ibu")) return DeathEnding.OrangTua;
        if (IsPlayerObject()) return DeathEnding.Player;
        return deathEnding;
    }

    bool IsPlayerObject()
    {
        string n = gameObject.name;
        return ContainsIgnoreCase(n, "Player") || n == "MC" || ContainsIgnoreCase(n, "MC (");
    }

    static bool ContainsIgnoreCase(string value, string part)
    {
        return value.IndexOf(part, System.StringComparison.OrdinalIgnoreCase) >= 0;
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
