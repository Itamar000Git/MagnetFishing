using UnityEngine;
using TMPro;

/// <summary>
/// Displays LevelManager's target, timer and end-of-level result. Must sit on a
/// GameObject that stays active, not on the result panel it hides.
/// </summary>
public class LevelHUD : MonoBehaviour
{
    [Tooltip("The LevelManager to display.")]
    [SerializeField] private LevelManager levelManager;

    [SerializeField] private TextMeshProUGUI targetLabel;
    [SerializeField] private TextMeshProUGUI timerLabel;

    [Tooltip("Root of the result panel. Hidden while the level is Playing.")]
    [SerializeField] private GameObject resultPanel;

    [SerializeField] private TextMeshProUGUI resultLabel;

    // Rewriting the label only when the shown second changes avoids a string
    // allocation and a TMP rebuild every frame.
    private int displayedSeconds = -1;

    private void OnEnable()
    {
        if (levelManager == null)
        {
            Debug.LogError("LevelHUD needs a LevelManager reference assigned.", this);
            return;
        }

        levelManager.StateChanged += HandleStateChanged;

        if (targetLabel != null)
        {
            targetLabel.text = $"Target: {levelManager.TargetScore}";
        }

        RefreshTimer();
        HandleStateChanged(levelManager.State);
    }

    private void OnDisable()
    {
        if (levelManager != null)
        {
            levelManager.StateChanged -= HandleStateChanged;
        }
    }

    private void Update()
    {
        if (levelManager != null)
        {
            RefreshTimer();
        }
    }

    private void RefreshTimer()
    {
        int seconds = Mathf.CeilToInt(levelManager.RemainingTime);

        if (seconds == displayedSeconds)
        {
            return;
        }

        displayedSeconds = seconds;

        if (timerLabel != null)
        {
            timerLabel.text = $"Time: {seconds}";
        }
    }

    private void HandleStateChanged(LevelState state)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(state != LevelState.Playing);
        }

        if (resultLabel != null && state != LevelState.Playing)
        {
            resultLabel.text = state == LevelState.Won ? "LEVEL COMPLETE" : "LEVEL FAILED";
        }
    }
}
