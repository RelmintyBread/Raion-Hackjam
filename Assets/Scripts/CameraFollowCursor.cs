using UnityEngine;

public class CameraFollowCursor : MonoBehaviour
{
    public float smoothSpeed = 5f;
    public float maxDistanceX = 2f;
    public float maxDistanceY = 4f;

    private Vector3 startPosition;

    [HideInInspector] public bool isLocked = false; // kalau true, kamera berhenti ngikutin cursor

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (isLocked) return; // skip semua logic follow kalau lagi di-lock

        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = transform.position.z;

        Vector3 offset = mouseWorldPos - startPosition;
        offset *= 0.5f;

        offset.x = Mathf.Clamp(offset.x, -maxDistanceX, maxDistanceX);
        offset.y = Mathf.Clamp(offset.y, -maxDistanceY, maxDistanceY);

        Vector3 targetPos = startPosition + offset;

        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
    }

    public void LockCamera()
    {
        isLocked = true;
    }

    public void UnlockCamera()
    {
        isLocked = false;
    }
}