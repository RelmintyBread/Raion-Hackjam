using UnityEngine;

public class CameraFollowCursor : MonoBehaviour
{
    public float smoothSpeed = 5f;
    public float maxDistanceX = 2f;   // horizontal clamp
    public float maxDistanceY = 4f;   // vertical clamp (bigger)

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = transform.position.z;

        Vector3 offset = mouseWorldPos - startPosition;
        offset *= 0.5f; // how strongly cursor pulls camera, tweak as you like

        // Clamp each axis independently
        offset.x = Mathf.Clamp(offset.x, -maxDistanceX, maxDistanceX);
        offset.y = Mathf.Clamp(offset.y, -maxDistanceY, maxDistanceY);

        Vector3 targetPos = startPosition + offset;

        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
    }
}