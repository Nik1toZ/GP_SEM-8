using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Управляет отображением финального счёта и обработкой кнопок Restart / Quit.
/// </summary>
public class GameOverPanel : MonoBehaviour
{
    public Text finalScoreText;
    public Text highScoreText;
    public Text newRecordLabel; // показывается только если рекорд побит
    public Button restartButton;
    public Button quitButton;

    private void OnEnable()
    {
        // Каждый раз при показе панели обновляем тексты — потому что данные
        // в GameManager меняются между смертями.
        Refresh();

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestart);
        }
        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(OnQuit);
        }
    }

    private void Refresh()
    {
        if (GameManager.Instance == null) return;

        int dist = GameManager.Instance.CurrentDistance;
        int best = GameManager.Instance.HighScore;

        if (finalScoreText != null) finalScoreText.text = dist + " m";
        if (highScoreText != null) highScoreText.text = "Best: " + best + " m";

        // Показываем "New record!" если текущая дистанция совпадает с рекордом
        // И при этом не равна нулю (т.е. что-то всё-таки прошли).
        if (newRecordLabel != null)
        {
            newRecordLabel.gameObject.SetActive(dist > 0 && dist == best);
        }
    }

    private void OnRestart()
    {
        if (GameManager.Instance != null) GameManager.Instance.Restart();
    }

    private void OnQuit()
    {
        if (GameManager.Instance != null) GameManager.Instance.Quit();
    }
}