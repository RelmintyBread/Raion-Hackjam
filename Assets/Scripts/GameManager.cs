using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Pasang script ini ke Empty GameObject (misal beri nama "GameManager").
/// Lalu di Button "Play" pada Main Menu:
/// OnClick() -> drag GameObject ini -> pilih GameManager.PlayGame
/// </summary>
public class GameManager : MonoBehaviour
{
    [Tooltip("Nama scene yang mau dibuka saat tombol Play ditekan")]
    public string gameSceneName = "SampleScene";

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit game");
        Application.Quit();
    }
}