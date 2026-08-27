using System.Collections;
using UnityEngine;

/// <summary>
/// Attach this to the HINGE object (the parent). Put your door sprite as a
/// child, offset from this object's origin, so rotating this transform
/// swings the door around its edge instead of its center.
/// </summary>
public class Door : MonoBehaviour, IInteractable
{
    [Header("Rotation")]
    public float openAngle = 90f;
    public float openDuration = 0.6f;   // seconds to swing fully open/closed
    public bool clockwise = false;

    private float closedZ;
    private bool isOpen;
    private bool stayOpen;
    private Coroutine rotateRoutine;

    public bool IsOpen => isOpen;
    public bool StayOpen => stayOpen;

    void Awake()
    {
        closedZ = transform.eulerAngles.z;
    }

    public void Open()
    {
        if (isOpen) return;
        if (!EnsureActive()) return;

        isOpen = true;
        SetBlockingColliders(false);
        float targetZ = closedZ + openAngle * (clockwise ? -1f : 1f);
        StartRotation(targetZ);
    }

    public void Close()
    {
        if (stayOpen) return;
        if (!isOpen) return;
        if (!EnsureActive()) return;

        isOpen = false;
        SetBlockingColliders(true);
        StartRotation(closedZ);
    }

    bool EnsureActive()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        return isActiveAndEnabled;
    }

    void StartRotation(float targetZ)
    {
        if (!isActiveAndEnabled) return;

        if (rotateRoutine != null)
            StopCoroutine(rotateRoutine);

        rotateRoutine = StartCoroutine(RotateTo(targetZ));
    }

    public void Interact()
    {
        stayOpen = true;
        Open();
    }

    void SetBlockingColliders(bool enabled)
    {
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            if (col == null || col.isTrigger) continue;
            col.enabled = enabled;
        }
    }



    IEnumerator RotateTo(float targetZ)
    {
        float startZ = transform.eulerAngles.z;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / openDuration;
            float z = Mathf.LerpAngle(startZ, targetZ, t);
            transform.eulerAngles = new Vector3(0f, 0f, z);
            yield return null;
        }

        transform.eulerAngles = new Vector3(0f, 0f, targetZ);
        rotateRoutine = null;
    }
}
