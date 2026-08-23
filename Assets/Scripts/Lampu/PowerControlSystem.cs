using UnityEngine;

public class PowerControlSystem : MonoBehaviour
{
    public Room[] rooms;
    private bool isPanelActive = false; // baru bisa pencet 1/2/3 kalau ini true

    void Start()
    {
        Debug.Log("PowerControlSystem aktif. Jumlah rooms: " + rooms.Length);
        for (int i = 0; i < rooms.Length; i++)
        {
            Debug.Log("Room index " + i + ": " + (rooms[i] != null ? rooms[i].roomName : "NULL"));
        }
    }

    void OnMouseDown()
    {
        isPanelActive = !isPanelActive; // toggle aktif/nggak tiap diklik
        Debug.Log("Generator diklik! Panel aktif: " + isPanelActive);
    }

    void Update()
    {
        if (!isPanelActive) return; // kalau belum diklik, skip semua logic di bawah

        if (Input.GetKeyDown(KeyCode.Alpha1) && rooms.Length > 0)
            rooms[0].ToggleRoom();

        if (Input.GetKeyDown(KeyCode.Alpha2) && rooms.Length > 1)
            rooms[1].ToggleRoom();

        if (Input.GetKeyDown(KeyCode.Alpha3) && rooms.Length > 2)
            rooms[2].ToggleRoom();
    }
}