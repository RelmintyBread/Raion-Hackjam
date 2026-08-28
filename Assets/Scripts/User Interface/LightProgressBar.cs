using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pasang script ini ke GameObject UI (misal child dari Canvas, sejajar dengan Slider-nya).
/// Menampilkan sisa daya (currentDuration / maxDuration) dari satu RoomLight tertentu.
///
/// Setup:
/// 1. Buat UI > Slider di Canvas, matikan interactable-nya (ini cuma buat display).
/// 2. Pasang script ini ke Slider (atau GameObject manapun), drag Slider itu ke field "durationSlider".
/// 3. Drag RoomLight yang mau dipantau (misal lampu ruangan tertentu) ke field "targetLight".
/// </summary>
public class LightProgressBar : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("RoomLight yang dayanya mau ditampilkan di progress bar ini")]
    public RoomLight targetLight;

    [Header("UI")]
    [Tooltip("Slider yang menampilkan persentase daya (0-1)")]
    public Slider durationSlider;

    [Tooltip("Opsional: Image fill dari slider, buat ganti warna saat daya menipis")]
    public Image fillImage;

    [Header("Color Stages")]
    [Tooltip("100% - 75%")]
    public Color colorFull = Color.green;
    [Tooltip("Di bawah 75%")]
    public Color colorMedium = Color.yellow;
    [Tooltip("Di bawah 50%")]
    public Color colorLow = new Color(1f, 0.55f, 0f); // orange
    [Tooltip("Di bawah 25%")]
    public Color colorCritical = Color.red;

    [Tooltip("Sembunyikan progress bar kalau lampu sedang mati")]
    public bool hideWhenLightOff = false;

    void Reset()
    {
        durationSlider = GetComponent<Slider>();
    }

    void Update()
    {
        if (CutsceneManager.IsPlaying) return;
        if (targetLight == null || durationSlider == null) return;

        float percent = targetLight.GetDurationPercent();
        durationSlider.value = percent;

        if (fillImage != null)
            fillImage.color = GetColorForPercent(percent);

        if (hideWhenLightOff)
            durationSlider.gameObject.SetActive(targetLight.IsOn);
    }

    Color GetColorForPercent(float percent)
    {
        if (percent > 0.75f) return colorFull;
        if (percent > 0.5f) return colorMedium;
        if (percent > 0.25f) return colorLow;
        return colorCritical;
    }
}
