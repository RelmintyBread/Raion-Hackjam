using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Panel/GameObject that holds the pause menu UI")]
    public GameObject pauseMenuUI;

    [Header("Scene")]
    [Tooltip("Build index of the main menu scene")]
    public int mainMenuSceneIndex = 0;

    public static bool IsPaused { get; private set; }

    void Start()
    {
        // Make sure the game starts unpaused
        IsPaused = false;
        Time.timeScale = 1f;
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
    }

    void Update()
    {
        if (CutsceneManager.IsPlaying) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);

        Time.timeScale = 0f; // freeze the game
        IsPaused = true;
    }

    public void Resume()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        Time.timeScale = 1f; // unfreeze the game
        IsPaused = false;
    }

    // Hook this up to your "Resume" button's OnClick()
    public void OnResumeButton()
    {
        Resume();
    }

    // Hook this up to your "Main Menu" button's OnClick()
    public void OnMainMenuButton()
    {
        // Always reset timescale before loading a new scene
        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene(0);
    }
}