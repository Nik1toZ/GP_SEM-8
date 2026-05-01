using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Отображает текущую дистанцию и рекорд во время игры.
/// </summary>
public class HUD : MonoBehaviour
{
    public Text distanceText;
    public Text highScoreText;

    private void Update()
    {
        if (GameManager.Instance == null) return;

        if (distanceText != null)
        {
            distanceText.text = GameManager.Instance.CurrentDistance + " m";
        }
        if (highScoreText != null)
        {
            highScoreText.text = "Best: " + GameManager.Instance.HighScore + " m";
        }
    }
}