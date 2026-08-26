using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RoomLight : MonoBehaviour
{
    [Header("Light Setup")]
    public Light2D lightSource;
    public float dimIntensity = 0.05f;
    public float brightIntensity = 1f;

    [Header("Power Limit")]
    public float maxDuration = 10f;      // batas maksimal lampu boleh nyala (detik)
    public float currentDuration;         // sisa waktu nyala saat ini

    private bool isOn = false;

    void Awake()
    {
        if (lightSource == null)
            lightSource = GetComponent<Light2D>();

        if (lightSource != null)
            lightSource.intensity = dimIntensity;

        currentDuration = maxDuration; // mulai penuh
    }

    void Update()
    {
        if (isOn)
        {
            currentDuration -= Time.deltaTime;

            if (currentDuration <= 0f)
            {
                currentDuration = 0f;
                TurnOff();
                Debug.Log(gameObject.name + " mati otomatis karena daya habis!");
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

        lightSource.intensity = brightIntensity;
        isOn = true;
    }

    public void TurnOff()
    {
        lightSource.intensity = dimIntensity;
        isOn = false;
    }

    public bool IsOn => isOn;

    public float GetDurationPercent() => currentDuration / maxDuration;
}