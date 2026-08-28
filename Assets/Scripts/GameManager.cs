using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Ami.BroAudio;

/// <summary>
/// Pasang script ini ke Empty GameObject (misal beri nama "GameManager").
/// Lalu di Button "Play" pada Main Menu:
/// OnClick() -> drag GameObject ini -> pilih GameManager.PlayGame
///
/// Semua audio (BGM/SFX) dan volume TIDAK lagi memanggil BroAudio langsung,
/// melainkan lewat AudioSystem (singleton) supaya satu sumber kebenaran.
/// - Play BGM/SFX -> AudioSystem.Instance.PlayAudio(soundID)
/// - Ubah volume  -> GameEvents.onValueChangeMaster/BGM/SFX.Invoke(value)
///   (AudioSystem sudah subscribe ke event ini di OnEnable)
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Nama scene yang mau dibuka saat tombol Play ditekan")]
    public string gameSceneName = "SampleScene";

    [Header("BroAudio - Sound yang dipakai di Main Menu")]
    [Tooltip("SFX yang diputar saat tombol di-klik (Play/Quit/dll)")]
    [SerializeField] SoundID buttonClickSFX;

    [Header("Volume Sliders")]
    [Tooltip("Slider untuk volume keseluruhan (Master)")]
    public Slider masterVolumeSlider;

    [Tooltip("Slider untuk volume musik (BGM)")]
    public Slider musicVolumeSlider;

    [Tooltip("Slider untuk volume sound effect (SFX)")]
    public Slider sfxVolumeSlider;

    void Start()
    {
        // Putar BGM Main Menu lewat AudioSystem (loop diatur di Library Manager BroAudio)
        AudioSystem.Instance.PlayAudio(AudioSystem.Instance.bgm_MainMenu);

        // Set slider sesuai volume tersimpan (default sesuai AudioSystem) lalu daftarkan listener
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", AudioSystem.Instance.defaultMaster);
            OnMasterSliderChanged(masterVolumeSlider.value);
            masterVolumeSlider.onValueChanged.AddListener(OnMasterSliderChanged);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", AudioSystem.Instance.defaultBGM);
            OnMusicSliderChanged(musicVolumeSlider.value);
            musicVolumeSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", AudioSystem.Instance.defaultSFX);
            OnSFXSliderChanged(sfxVolumeSlider.value);
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        }
    }

    // Dipanggil oleh Master Volume Slider (OnValueChanged)
    public void OnMasterSliderChanged(float value)
    {
        GameEvents.onValueChangeMaster?.Invoke(value);
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    // Dipanggil oleh Music Volume Slider (OnValueChanged)
    public void OnMusicSliderChanged(float value)
    {
        GameEvents.onValueChangeBGM?.Invoke(value);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    // Dipanggil oleh SFX Volume Slider (OnValueChanged)
    public void OnSFXSliderChanged(float value)
    {
        GameEvents.onValueChangeSFX?.Invoke(value);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public void PlayGame()
    {
        AudioSystem.Instance.PlayAudio(buttonClickSFX);
        AudioSystem.Instance.StopBGM();
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        AudioSystem.Instance.PlayAudio(buttonClickSFX);
        Debug.Log("Quit game");
        Application.Quit();
    }

    // Bisa dipanggil dari tombol lain (Settings, Back, dll) yang butuh SFX klik
    // OnClick() -> drag GameObject ini -> pilih GameManager.PlayButtonClickSFX
    public void PlayButtonClickSFX()
    {
        AudioSystem.Instance.PlayAudio(buttonClickSFX);
    }

    public void OpenPopUp(GameObject gameobject)
    {
        gameobject.SetActive(true);
    }

    public void ClosePopup(GameObject gameobject)
    {
        gameobject.SetActive(false);
    }
}