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
/// Also seeds ScoreManager from the campaign's level-start checkpoint at
/// Awake and, on a win, commits the final score back to the campaign's
/// cumulative score (see LevelProgressManager.EstablishLevelStartCheckpoint).
/// Also exposes the scene-load actions for the result panel's buttons.
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Tooltip("Fallback level length in seconds, used only if no LevelConfig resolves for the active level.")]
    [SerializeField] private float levelDuration = 60f;

    [Tooltip("Fallback score needed to win when time expires, used only if no LevelConfig resolves for the active level.")]
    [SerializeField] private int targetScore = 100;

    [Tooltip("The campaign's level definitions. The active level (from LevelProgressManager) is looked up here at Awake to set levelDuration/targetScore.")]
    [SerializeField] private LevelCatalog levelCatalog;

    [Tooltip("The level's ScoreManager. Seeded from the campaign checkpoint at Awake, read when the result is evaluated, and committed back to the campaign on a win.")]
    [SerializeField] private ScoreManager scoreManager;

    [Tooltip("The MagnetLaunchController on AimPivot. Launches are blocked through it when time expires.")]
    [SerializeField] private MagnetLaunchController launchController;

    [SerializeField] private string level01SceneName = "Level01";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private float elapsedTime;
    private bool activeLevelConfigResolved;

    public LevelState State { get; private set; } = LevelState.Playing;

    /// <summary>The level number resolved for this run (from LevelProgressManager at Awake).</summary>
    public int ActiveLevel { get; private set; }

    /// <summary>True when the catalog has a LevelConfig for ActiveLevel + 1.</summary>
    public bool HasNextLevel => levelCatalog != null && levelCatalog.GetLevel(ActiveLevel + 1) != null;

    /// <summary>
    /// True once, on a win, when the active level's own LevelConfig resolved
    /// successfully (not the inspector fallback) and the catalog has no level
    /// after it. Gates the campaign-ending screen instead of the normal result
    /// panel. Requiring activeLevelConfigResolved keeps a broken/missing
    /// catalog reference from being misread as "campaign complete": in that
    /// failure case HasNextLevel is also false, but for the wrong reason, so
    /// this would otherwise show the ending after any level.
    /// </summary>
    public bool IsCampaignComplete => State == LevelState.Won && activeLevelConfigResolved && !HasNextLevel;

    /// <summary>
    /// The score committed to the campaign by this run's result (see
    /// FinishLevel). Valid once State is no longer Playing; intended as the
    /// single source of truth for displaying — and later submitting — the
    /// final score.
    /// </summary>
    public int FinalCumulativeScore => LevelProgressManager.Instance != null ? LevelProgressManager.Instance.CumulativeScore : scoreManager.Score;

    /// <summary>Seconds left, never negative. Stays at 0 while a final retrieval finishes.</summary>
    public float RemainingTime => Mathf.Max(levelDuration - elapsedTime, 0f);

    public int TargetScore => targetScore;

    /// <summary>Holds the countdown and result check, such as while the How to Play overlay is open.</summary>
    public bool TimerPaused { get; set; }

    /// <summary>Raised once, when the level moves from Playing to Won or Lost.</summary>
    public event Action<LevelState> StateChanged;

    private void Awake()
    {
        if (scoreManager == null || launchController == null)
        {
            Debug.LogError("LevelManager needs ScoreManager and MagnetLaunchController references assigned. Timer disabled.", this);
            enabled = false;
            return;
        }

        ActiveLevel = LevelProgressManager.Instance != null ? LevelProgressManager.Instance.CurrentLevel : 1;
        ApplyActiveLevelConfig(ActiveLevel);

        // Seed the runtime score with this level's entry checkpoint, not raw
        // CumulativeScore directly: EstablishLevelStartCheckpoint only re-reads
        // CumulativeScore the first time a given level number is entered, so a
        // win's commit never leaks into that same level's own replays (Retry,
        // or Main Menu -> Play while still on the same level) — it only takes
        // effect once the active level actually changes.
        int checkpoint = LevelProgressManager.Instance != null
            ? LevelProgressManager.Instance.EstablishLevelStartCheckpoint(ActiveLevel)
            : 0;
        scoreManager.Initialize(checkpoint);
    }

    /// <summary>
    /// Resolves activeLevel's LevelConfig from levelCatalog and overwrites
    /// levelDuration/targetScore with its values. Leaves the inspector fallback
    /// values in place if no catalog or no matching config is available.
    /// </summary>
    private void ApplyActiveLevelConfig(int activeLevel)
    {
        LevelConfig config = levelCatalog != null ? levelCatalog.GetLevel(activeLevel) : null;
        activeLevelConfigResolved = config != null;

        if (config != null)
        {
            levelDuration = config.Duration;
            targetScore = config.CumulativeTargetScore;
        }
        else
        {
            Debug.LogWarning("LevelManager: no LevelConfig resolved for the active level; using inspector fallback values.", this);
        }
    }

    private void Update()
    {
        if (State != LevelState.Playing || TimerPaused)
        {
            return;
        }

        elapsedTime += Time.deltaTime;

        if (RemainingTime > 0f)
        {
            return;
        }

        // Timer just hit 0 this frame: no further launches, and any in-flight
        // cycle (extending, retracting, or carrying a caught-but-not-yet-awarded
        // object) is cancelled immediately with no reward, so Win/Loss below is
        // evaluated against exactly the score that existed at timeout.
        launchController.LaunchEnabled = false;
        launchController.CancelCycle();
        FinishLevel();
    }

    // Called exactly once per level: Update returns early once State is no
    // longer Playing, so this can never run a second time and double-commit.
    //
    // Only the current level's most recent attempt matters — there is no best
    // score and no first-completion protection. A win persists this attempt's
    // final score; a loss persists LevelStartScore instead, which deliberately
    // erases any result an earlier, now-superseded win on this same level left
    // behind. LevelStartScore itself never changes here: it only moves when
    // the active level itself changes (see EstablishLevelStartCheckpoint), so
    // every replay of this level — win or lose — keeps starting from the same
    // pre-level score.
    private void FinishLevel()
    {
        State = scoreManager.Score >= targetScore ? LevelState.Won : LevelState.Lost;

        if (LevelProgressManager.Instance != null)
        {
            int finalCumulativeScore = State == LevelState.Won
                ? scoreManager.Score
                : LevelProgressManager.Instance.LevelStartScore;
            LevelProgressManager.Instance.SetCumulativeScore(finalCumulativeScore);
        }

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

    /// <summary>
    /// Result panel button, shown only on a win when HasNextLevel is true:
    /// unlocks and advances the campaign to ActiveLevel + 1, then reloads the
    /// reusable gameplay scene so LevelContentSelector picks up the new level.
    /// </summary>
    public void NextLevel()
    {
        if (LevelProgressManager.Instance == null || !HasNextLevel)
        {
            Debug.LogWarning("LevelManager.NextLevel called with no next level available; ignoring.", this);
            return;
        }

        int nextLevel = ActiveLevel + 1;
        LevelProgressManager.Instance.UnlockLevel(nextLevel);
        LevelProgressManager.Instance.SetCurrentLevel(nextLevel);
        SceneManager.LoadScene(level01SceneName);
    }
}
