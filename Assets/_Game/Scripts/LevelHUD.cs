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

    [Tooltip("Shows the active level number, e.g. \"LEVEL 1\". Source of truth is LevelManager.ActiveLevel.")]
    [SerializeField] private TextMeshProUGUI levelLabel;

    [Tooltip("Root of the result panel. Hidden while the level is Playing.")]
    [SerializeField] private GameObject resultPanel;

    [SerializeField] private TextMeshProUGUI resultLabel;

    [Tooltip("Shown only on a win when LevelManager.HasNextLevel is true.")]
    [SerializeField] private GameObject nextLevelButton;

    [Tooltip("Shown instead of resultPanel when LevelManager.IsCampaignComplete is true.")]
    [SerializeField] private GameObject campaignEndingPanel;

    [SerializeField] private TextMeshProUGUI finalScoreLabel;

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

        if (levelLabel != null)
        {
            levelLabel.text = $"LEVEL {levelManager.ActiveLevel}";
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
        bool campaignComplete = levelManager.IsCampaignComplete;

        if (campaignEndingPanel != null)
        {
            campaignEndingPanel.SetActive(campaignComplete);
        }

        if (campaignComplete)
        {
            // The campaign ending replaces the normal result panel entirely,
            // so leave resultPanel/resultLabel/nextLevelButton untouched below.
            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }

            if (finalScoreLabel != null)
            {
                finalScoreLabel.text = $"FINAL SCORE: {levelManager.FinalCumulativeScore}";
            }

            return;
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(state != LevelState.Playing);
        }

        if (resultLabel != null && state != LevelState.Playing)
        {
            resultLabel.text = state == LevelState.Won ? "LEVEL COMPLETE" : "LEVEL FAILED";
        }

        if (nextLevelButton != null)
        {
            nextLevelButton.SetActive(state == LevelState.Won && levelManager.HasNextLevel);
        }
    }
}
