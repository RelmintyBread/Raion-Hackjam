using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Door : MonoBehaviour
{
    [Header("Open")]
    public float openTorque = 12f;
    public float maxOpenAngle = 90f;
    public bool clockwise = false;

    [Header("Feel")]
    public float angularDamping = 2f;

    private Rigidbody2D rb;
    private float startAngle;
    private bool isOpening;
    private bool isOpen;

    public bool IsOpening => isOpening;
    public bool IsOpen => isOpen;

    float Direction => clockwise ? -1f : 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        SetupRigidbody();
    }

    void SetupRigidbody()
    {
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
        rb.centerOfMass = Vector2.zero;
        rb.angularDamping = angularDamping;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void TryOpen(Collider2D opener = null)
    {
        if (isOpening || isOpen)
            return;

        isOpening = true;
        startAngle = rb.rotation;
        rb.constraints = RigidbodyConstraints2D.FreezePosition;

        if (opener != null)
            IgnoreOpener(opener);

        Debug.Log(name + ": monster membuka pintu.");
    }

    void FixedUpdate()
    {
        if (!isOpening || isOpen)
            return;

        float targetAngle = startAngle + maxOpenAngle * Direction;
        float remaining = Mathf.DeltaAngle(rb.rotation, targetAngle);

        if (Mathf.Abs(remaining) <= 1f)
        {
            FinishOpen(targetAngle);
            return;
        }

        rb.AddTorque(openTorque * Direction);
    }

    void FinishOpen(float targetAngle)
    {
        isOpening = false;
        isOpen = true;
        rb.angularVelocity = 0f;
        rb.SetRotation(targetAngle);
        rb.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
        Debug.Log(name + ": pintu terbuka.");
    }

    void IgnoreOpener(Collider2D opener)
    {
        Collider2D[] cols = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in cols)
        {
            if (col != null)
                Physics2D.IgnoreCollision(col, opener, true);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.12f);

        Vector3 closedDir = transform.right;
        Vector3 openDir = Quaternion.Euler(0f, 0f, maxOpenAngle * Direction) * closedDir;
        Gizmos.DrawLine(transform.position, transform.position + closedDir * 0.6f);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + openDir * 0.6f);
    }
}
