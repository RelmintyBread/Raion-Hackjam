using UnityEngine;

public class CameraFollowCursor : MonoBehaviour
{
    public float smoothSpeed = 5f;
    public float maxDistanceX = 2f;
    public float maxDistanceY = 4f;

    private Vector3 startPosition;

    [HideInInspector] public bool isLocked = false;
    Transform followTarget;

    void Start()
    {
        startPosition = transform.position;
    }

    void LateUpdate()
    {
        Vector3 targetPos;

        if (followTarget != null)
        {
            targetPos = followTarget.position;
            targetPos.z = transform.position.z;
            transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.unscaledDeltaTime);
            return;
        }

        if (isLocked) return;

        Vector3 viewport = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        float nx = (viewport.x - 0.5f) * 2f;
        float ny = (viewport.y - 0.5f) * 2f;

        targetPos = startPosition;
        targetPos.x += Mathf.Clamp(nx, -1f, 1f) * maxDistanceX;
        targetPos.y += Mathf.Clamp(ny, -1f, 1f) * maxDistanceY;
        targetPos.z = transform.position.z;

        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
    }

    public void LockToTarget(Transform target)
    {
        followTarget = target;
        isLocked = true;
    }

    public void FollowMouse()
    {
        followTarget = null;
        isLocked = false;
        startPosition = transform.position;
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