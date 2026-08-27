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
    private Coroutine rotateRoutine;

    public bool IsOpen => isOpen;

    void Awake()
    {
        closedZ = transform.eulerAngles.z;
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        float targetZ = closedZ + openAngle * (clockwise ? -1f : 1f);
        StartRotation(targetZ);
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        StartRotation(closedZ);
    }

    void StartRotation(float targetZ)
    {
        if (rotateRoutine != null)
            StopCoroutine(rotateRoutine);

        rotateRoutine = StartCoroutine(RotateTo(targetZ));
    }

    public void Interact()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        Open();
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
