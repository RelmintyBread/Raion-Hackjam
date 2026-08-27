using UnityEngine;
using UnityEngine.Events;

public class Room : MonoBehaviour
{
    public string roomName = "Room 1";
    public RoomLight[] lights;
    public Door[] roomDoors;
    public Transform[] investigatePoints;

    [Header("Events")]
    public UnityEvent OnRoomPowerOut; // <-- ini juga muncul di Inspector

    [HideInInspector] public bool monsterCleared;

    void Awake()
    {
        foreach (RoomLight light in lights)
            light.OnPowerOut.AddListener(HandleLightPowerOut);
    }

    void HandleLightPowerOut()
    {
        Debug.Log($"[Room] {roomName} kehilangan daya, invoking OnRoomPowerOut.");
        OnRoomPowerOut?.Invoke();
    }

    public void TurnOnRoom()
    {
        foreach (RoomLight light in lights)
            light.TurnOn();
    }

    public void TurnOffRoom()
    {
        foreach (RoomLight light in lights)
            light.TurnOff();
    }

    public void ToggleRoom()
    {
        if (IsAnyLightOn()) TurnOffRoom();
        else TurnOnRoom();
    }

    public bool IsAnyLightOn()
    {
        foreach (RoomLight light in lights)
        {
            if (light.IsOn) return true;
        }
        return false;
    }

    public bool IsRoomDark()
    {
        if (lights == null || lights.Length == 0) return false;

        foreach (RoomLight light in lights)
        {
            if (light.IsOn) return false;
        }
        return true;
    }
}