using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Управление главным меню: показ рекорда, навигация между панелями,
/// запуск игры или выход.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Кнопки главной панели")]
    public Button playButton;
    public Button howToPlayButton;
    public Button quitButton;

    [Header("Текст рекорда")]
    public Text highScoreText;

    [Header("Панель правил")]
    public GameObject rulesPanel;
    public Button backButton;

    [Header("Имя игровой сцены")]
    [Tooltip("Должно совпадать с именем файла сцены в папке Scenes")]
    public string gameSceneName = "Main";

    private const string HighScoreKey = "BridgeRunner_HighScore";

    private void Start()
    {
        // Восстанавливаем нормальное состояние времени и курсора, на случай
        // если в меню вернулись из остановленной Game Over сцены.
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Показываем рекорд (если он есть в PlayerPrefs).
        int highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        if (highScoreText != null)
        {
            highScoreText.text = "Best: " + highScore + " m";
        }

        // Панель правил скрыта по умолчанию.
        if (rulesPanel != null) rulesPanel.SetActive(false);

        // Подписываемся на нажатия.
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnPlay);
        }
        if (howToPlayButton != null)
        {
            howToPlayButton.onClick.RemoveAllListeners();
            howToPlayButton.onClick.AddListener(OnHowToPlay);
        }
        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(OnQuit);
        }
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackToMenu);
        }
    }

    private void OnPlay()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnHowToPlay()
    {
        if (rulesPanel != null) rulesPanel.SetActive(true);
    }

    private void OnBackToMenu()
    {
        if (rulesPanel != null) rulesPanel.SetActive(false);
    }

    private void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}