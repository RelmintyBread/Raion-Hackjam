using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

public class RoomLight : MonoBehaviour
{
    [Header("Light Setup")]
    public Light2D lightSource;
    public float dimIntensity = 0.05f;
    public float brightIntensity = 2f;
    public float dimRadius = 1.2f;
    public float onRadius = 16f;

    [Header("Power Limit")]
    public float maxDuration = 10f;
    [SerializeField] public float currentDuration;
    public bool startOn = false;

    [Header("Events")]
    public UnityEvent OnPowerOut; // <-- muncul di Inspector, bisa drag target manual

    private bool isOn = false;

    void Awake()
    {
        if (lightSource == null)
            lightSource = GetComponent<Light2D>();

        if (lightSource != null)
        {
            lightSource.shadowsEnabled = true;
            lightSource.shadowIntensity = 1f;
            ApplyLight(false);
        }

        currentDuration = maxDuration;
        LightOccluders2D.Install();
    }

    void Start()
    {
        if (startOn)
            TurnOn();
    }

    void Update()
    {
        if (CutsceneManager.IsPlaying) return;

        if (isOn)
        {
            currentDuration -= Time.deltaTime;

            if (currentDuration <= 0f)
            {
                currentDuration = 0f;
                TurnOff();
                Debug.Log(gameObject.name + " mati otomatis karena daya habis!");
                OnPowerOut?.Invoke();
            }
        }
    }

    public void TurnOn()
    {
        if (currentDuration <= 0f)
        {
            Debug.Log(gameObject.name + " tidak bisa nyala, daya habis!");
            return;
        }

        ApplyLight(true);
        isOn = true;
    }

    public void TurnOff()
    {
        ApplyLight(false);
        isOn = false;
    }

    void ApplyLight(bool on)
    {
        if (lightSource == null) return;
        lightSource.intensity = on ? brightIntensity : dimIntensity;
        lightSource.pointLightInnerRadius = on ? onRadius * 0.35f : 0f;
        lightSource.pointLightOuterRadius = on ? onRadius : dimRadius;
    }

    public bool IsOn => isOn;

    public float GetDurationPercent() => currentDuration / maxDuration;
}