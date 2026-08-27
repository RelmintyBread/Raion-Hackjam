using UnityEngine;

/// <summary>
/// Put this on a separate child GameObject positioned in front of the door -
/// a BoxCollider2D with no sprite ("invisible square"). When the monster
/// walks into it, the door opens. Since you're making the monster ignore
/// physical collision with the door, this trigger is the only thing that
/// needs to detect it.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class DoorSensor : MonoBehaviour
{
    [Tooltip("Drag the Door component (on the hinge object) here.")]
    public Door door;

    [Tooltip("Tag your monster GameObject with this so the sensor knows what to react to.")]
    public string enemyTag = "Enemy";

    [Tooltip("If true, the door closes again once the monster leaves the sensor zone.")]
    public bool closeWhenEmpty = true;

    void Reset()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (door == null) return;

        if (other.CompareTag(enemyTag))
            door.Open();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (door == null || !closeWhenEmpty) return;

        if (other.CompareTag(enemyTag))
            door.Close();
    }
}
