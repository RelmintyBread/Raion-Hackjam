using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RoomLight : MonoBehaviour
{
    public Light2D lightSource;
    public float dimIntensity = 0.05f;
    public float brightIntensity = 1f;

    private bool isOn = false;

    void Awake()
    {
        if (lightSource != null)
            lightSource.intensity = dimIntensity;
    }

    public void TurnOn()
    {
        lightSource.intensity = brightIntensity;
        isOn = true;
    }

    public void TurnOff()
    {
        lightSource.intensity = dimIntensity;
        isOn = false;
    }

    public bool IsOn => isOn;
}