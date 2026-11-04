using UnityEngine;
using TMPro;

/// <summary>
/// Displays the current score from ScoreManager on a TextMeshProUGUI label.
/// Subscribes to ScoreManager.ScoreChanged and updates immediately.
/// </summary>
public class ScoreHUD : MonoBehaviour
{
    [Tooltip("The ScoreManager to display. Auto-found via FindFirstObjectByType if left empty.")]
    [SerializeField] private ScoreManager scoreManager;

    [Tooltip("The TextMeshProUGUI label to update. Auto-found on this GameObject if left empty.")]
    [SerializeField] private TextMeshProUGUI label;

    private void Awake()
    {
        if (scoreManager == null)
        {
            scoreManager = FindFirstObjectByType<ScoreManager>();
        }

        if (label == null)
        {
            label = GetComponent<TextMeshProUGUI>();
        }
    }

    private void OnEnable()
    {
        if (scoreManager != null)
        {
            scoreManager.ScoreChanged += HandleScoreChanged;
            HandleScoreChanged(scoreManager.Score);
        }
    }

    private void OnDisable()
    {
        if (scoreManager != null)
        {
            scoreManager.ScoreChanged -= HandleScoreChanged;
        }
    }

    private void HandleScoreChanged(int newScore)
    {
        if (label != null)
        {
            label.text = $"Score: {newScore}";
        }
    }
}
