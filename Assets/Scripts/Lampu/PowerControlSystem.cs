using UnityEngine;
using UnityEngine.UI;

public class PowerControlSystem : MonoBehaviour
{
    [Header("Rooms")]
    public Room[] rooms;

    [Header("Limit")]
    public int maxActiveRooms = 2;

    [Header("Energy (Shared Pool - sesuai GDD)")]
    public float totalEnergy = 840f;
    [SerializeField] private float currentEnergy;

    [Header("UI Panel")]
    public GameObject controlPanelUI;
    public Button[] roomButtons;
    public Button closeButton;

    [Header("Camera")]
    public CameraFollowCursor cameraFollow;

    private bool panelOpen = false;

    void Awake()
    {
        currentEnergy = totalEnergy;
    }

    void Start()
    {
        if (controlPanelUI != null)
            controlPanelUI.SetActive(false);

        for (int i = 0; i < roomButtons.Length; i++)
        {
            int index = i;
            roomButtons[i].onClick.AddListener(() => TryToggleRoom(index));
        }

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    void Update()
    {
        int activeCount = CountActiveRooms();
        if (activeCount <= 0) return;

        currentEnergy -= activeCount * Time.deltaTime;

        if (currentEnergy <= 0f)
        {
            currentEnergy = 0f;
            ForceShutdownAllRooms();
        }
    }

    void ForceShutdownAllRooms()
    {
        foreach (Room room in rooms)
        {
            if (room.IsAnyLightOn())
                room.TurnOffRoom();
        }
        Debug.Log("[PowerControlSystem] Energi habis! Semua ruangan dimatikan paksa.");
    }

    public float GetEnergyPercent() => currentEnergy / totalEnergy;
    public float GetCurrentEnergy() => currentEnergy;


    void OnMouseDown()
    {
        if (panelOpen) ClosePanel();
        else OpenPanel();
    }

    void OpenPanel()
    {
        panelOpen = true;
        controlPanelUI.SetActive(true);

        if (cameraFollow != null)
            cameraFollow.LockCamera(); // kunci kamera pas panel dibuka
    }

    public void ClosePanel()
    {
        panelOpen = false;
        if (controlPanelUI != null)
            controlPanelUI.SetActive(false);

        if (cameraFollow != null)
            cameraFollow.UnlockCamera(); // lepas kunci pas panel ditutup
    }

    public void TryToggleRoom(int index)
    {
        if (index < 0 || index >= rooms.Length) return;

        Room targetRoom = rooms[index];

        if (targetRoom.IsAnyLightOn())
        {
            targetRoom.TurnOffRoom();
            Debug.Log(targetRoom.roomName + " dimatikan.");
            return;
        }

        int activeCount = CountActiveRooms();

        if (activeCount >= maxActiveRooms)
        {
            Debug.LogWarning("Tidak bisa menyalakan " + targetRoom.roomName +
                "! Maksimal " + maxActiveRooms + " ruangan menyala secara bersamaan. Matikan ruangan lain terlebih dahulu.");
            return;
        }

        targetRoom.TurnOnRoom();
        Debug.Log(targetRoom.roomName + " dinyalakan.");
    }

    private int CountActiveRooms()
    {
        int count = 0;
        foreach (Room room in rooms)
        {
            if (room.IsAnyLightOn()) count++;
        }
        return count;
    }
}