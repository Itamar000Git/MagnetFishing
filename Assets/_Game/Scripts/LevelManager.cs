using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum LevelState
{
    Playing,
    Won,
    Lost
}

/// <summary>
/// Runs Level01's timed rules: counts down from levelDuration, then compares
/// the ScoreManager's score to targetScore. Reaching the target does not end
/// the level early; the player keeps collecting until time expires.
/// Also exposes the scene-load actions for the result panel's buttons.
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Tooltip("Temporary tuning value, not final: level length in seconds.")]
    [SerializeField] private float levelDuration = 60f;

    [Tooltip("Temporary tuning value, not final: score needed to win when time expires.")]
    [SerializeField] private int targetScore = 50;

    [Tooltip("The level's ScoreManager. Read when the result is evaluated; this class keeps no score of its own.")]
    [SerializeField] private ScoreManager scoreManager;

    [Tooltip("The MagnetLaunchController on AimPivot. Launches are blocked through it when time expires.")]
    [SerializeField] private MagnetLaunchController launchController;

    [SerializeField] private string level01SceneName = "Level01";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private float elapsedTime;
    private bool timeExpired;

    public LevelState State { get; private set; } = LevelState.Playing;

    /// <summary>Seconds left, never negative. Stays at 0 while a final retrieval finishes.</summary>
    public float RemainingTime => Mathf.Max(levelDuration - elapsedTime, 0f);

    public int TargetScore => targetScore;

    /// <summary>Raised once, when the level moves from Playing to Won or Lost.</summary>
    public event Action<LevelState> StateChanged;

    private void Awake()
    {
        if (scoreManager == null || launchController == null)
        {
            Debug.LogError("LevelManager needs ScoreManager and MagnetLaunchController references assigned. Timer disabled.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (State != LevelState.Playing)
        {
            return;
        }

        if (!timeExpired)
        {
            elapsedTime += Time.deltaTime;

            if (RemainingTime > 0f)
            {
                return;
            }

            timeExpired = true;
            launchController.LaunchEnabled = false;
        }

        // Design choice: a launch already in flight at time zero runs its normal
        // cycle (catch or max distance, retract, award) before the result is
        // decided, so an object caught in time still counts. State stays Playing
        // and the timer stays at 0 during this window. IsIdle only becomes true
        // after the award, so the score read below includes the final catch.
        if (launchController.IsIdle)
        {
            FinishLevel();
        }
    }

    private void FinishLevel()
    {
        State = scoreManager.Score >= targetScore ? LevelState.Won : LevelState.Lost;
        StateChanged?.Invoke(State);
    }

    /// <summary>Result panel button: reloads Level01.</summary>
    public void Retry()
    {
        SceneManager.LoadScene(level01SceneName);
    }

    /// <summary>Result panel button: returns to the Main Menu.</summary>
    public void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
