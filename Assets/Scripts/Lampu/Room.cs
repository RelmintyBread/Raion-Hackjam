using UnityEngine;

public class Room : MonoBehaviour
{
    public string roomName = "Room 1";
    public RoomLight[] lights;

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
}