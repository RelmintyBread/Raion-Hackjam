using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Slideshow gambar. Pasang di scene GamePlay.
/// Main Menu Play -> scene load -> cutscene otomatis.
/// Klik kiri mouse: next gambar. Klik di slide terakhir: game mulai.
/// </summary>
public class CutsceneManager : MonoBehaviour
{
    public static bool IsPlaying { get; private set; }

    [Header("Gambar intro (urut)")]
    public Sprite[] slides;

    [Header("Gambar ending")]
    public Sprite endingPlayer;
    public Sprite endingAdik;
    public Sprite endingOrangTua;

    [Header("Ending")]
    [Tooltip("Scene yang dibuka setelah klik ending (biasanya MainMenu)")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Kamera")]
    public CameraFollowCursor cameraFollow;

    [Header("Play")]
    public bool playOnSceneStart = true;

    GameObject panel;
    Image slideImage;
    int index;
    bool clickArmed;
    bool isEnding;

    void Start()
    {
        if (cameraFollow == null)
            cameraFollow = FindAnyObjectByType<CameraFollowCursor>();

        if (playOnSceneStart && slides != null && slides.Length > 0)
            Play();
        else
            Hide();
    }

    void Update()
    {
        if (!IsPlaying) return;

        if (Input.GetMouseButtonUp(0))
            clickArmed = true;

        if (!clickArmed) return;
        if (!Input.GetMouseButtonDown(0)) return;

        clickArmed = false;
        if (isEnding)
            LoadMainMenu();
        else
            Next();
    }

    public void Play()
    {
        if (slides == null || slides.Length == 0)
        {
            Debug.LogWarning("[CutsceneManager] Slides masih kosong. Drag gambar ke array Slides.");
            return;
        }

        EnsureUI();
        IsPlaying = true;
        index = 0;
        clickArmed = false;
        Time.timeScale = 0f;

        if (cameraFollow != null)
            cameraFollow.LockCamera();

        ShowCurrent();
        panel.SetActive(true);
    }

    public void PlayEnding(FamilyMember.DeathEnding ending)
    {
        if (isEnding) return;

        Sprite slide = GetEndingSprite(ending);
        if (slide == null)
        {
            Debug.LogWarning("[CutsceneManager] Ending slide untuk " + ending + " masih kosong.");
            return;
        }

        EnsureUI();
        IsPlaying = true;
        isEnding = true;
        clickArmed = false;
        Time.timeScale = 0f;

        if (cameraFollow != null)
            cameraFollow.LockCamera();

        slideImage.sprite = slide;
        panel.SetActive(true);
    }

    Sprite GetEndingSprite(FamilyMember.DeathEnding ending)
    {
        switch (ending)
        {
            case FamilyMember.DeathEnding.Player: return endingPlayer;
            case FamilyMember.DeathEnding.Adik: return endingAdik;
            case FamilyMember.DeathEnding.OrangTua: return endingOrangTua;
            default: return null;
        }
    }

    void Next()
    {
        index++;
        if (index >= slides.Length)
        {
            End();
            return;
        }

        ShowCurrent();
    }

    void End()
    {
        Hide();
        IsPlaying = false;
        Time.timeScale = 1f;

        if (cameraFollow != null)
            cameraFollow.FollowMouse();
    }

    void LoadMainMenu()
    {
        Time.timeScale = 1f;
        IsPlaying = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }

    void ShowCurrent()
    {
        if (slideImage != null && index >= 0 && index < slides.Length)
            slideImage.sprite = slides[index];
    }

    void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    void EnsureUI()
    {
        if (panel != null && slideImage != null) return;

        GameObject canvasGo = new GameObject("CutsceneCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        panel = new GameObject("CutscenePanel");
        panel.transform.SetParent(canvas.transform, false);
        RectTransform panelRt = panel.AddComponent<RectTransform>();
        Stretch(panelRt);
        Image bg = panel.AddComponent<Image>();
        bg.color = Color.black;

        GameObject imageGo = new GameObject("Slide");
        imageGo.transform.SetParent(panel.transform, false);
        RectTransform imageRt = imageGo.AddComponent<RectTransform>();
        Stretch(imageRt);
        slideImage = imageGo.AddComponent<Image>();
        slideImage.preserveAspect = true;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
