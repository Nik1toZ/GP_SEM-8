using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Глобальное состояние игры. Один экземпляр на сцену, доступ через GameManager.Instance.
/// Хранит текущую дистанцию, рекорд, состояние Playing/GameOver.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, GameOver }

    [Header("Игрок")]
    [Tooltip("Источник для расчёта дистанции — Z-координата")]
    public Transform player;

    [Tooltip("Z-координата старта игрока. От неё считается пройденная дистанция.")]
    public float startZ = -20f;

    [Header("UI")]
    [Tooltip("Панель, появляющаяся при смерти — должна быть выключена на старте")]
    public GameObject gameOverPanel;

    [Tooltip("HUD во время игры — скрывается на Game Over")]
    public GameObject hudPanel;

    public GameState State { get; private set; } = GameState.Playing;
    public int CurrentDistance { get; private set; }
    public int HighScore { get; private set; }

    // Ключ PlayerPrefs для хранения рекорда между запусками.
    private const string HighScoreKey = "BridgeRunner_HighScore";

    private void Awake()
    {
        // Простой синглтон, без переноса между сценами — нам это не нужно.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
    }

    private void Start()
    {
        // Гарантируем правильное стартовое состояние UI.
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(true);

        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (State != GameState.Playing || player == null) return;

        // Дистанция — целое число метров от точки старта.
        // Math.Max нужен на случай если игрока случайно отбросило назад.
        int dist = Mathf.Max(0, Mathf.FloorToInt(player.position.z - startZ));

        if (dist > CurrentDistance)
        {
            CurrentDistance = dist;
        }
    }

    public void GameOver()
    {
        if (State == GameState.GameOver) return;
        State = GameState.GameOver;

        // Обновляем рекорд если побит.
        if (CurrentDistance > HighScore)
        {
            HighScore = CurrentDistance;
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
            PlayerPrefs.Save();
        }

        // Показываем экран и замораживаем игру. Time.timeScale = 0 останавливает
        // физику и Update-логику, но UI продолжает работать (он независим от timeScale).
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (hudPanel != null) hudPanel.SetActive(false);

        Time.timeScale = 0f;

        // Курсор для удобства клика по кнопкам.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>Перезапуск через перезагрузку текущей сцены.</summary>
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>Возвращает в главное меню.</summary>
    public void Quit()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}