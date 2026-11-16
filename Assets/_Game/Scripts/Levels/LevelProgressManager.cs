using UnityEngine;

/// <summary>
/// The game's single persistent campaign-progress tracker. A prefab instance
/// sits in every scene so any scene can be played directly; the first one
/// survives scene loads and later copies remove themselves (same pattern as
/// AudioManager). Backed by PlayerPrefs so progress survives app restarts.
///
/// CumulativeScore is the committed campaign score (only a successful level
/// completion writes it). LevelStartScore is the separate level-entry
/// checkpoint a level seeds its runtime score from — see
/// EstablishLevelStartCheckpoint for why the two cannot share one field.
///
/// Runs its Awake before other scripts' (DefaultExecutionOrder -100):
/// Unity does not guarantee Awake order across root GameObjects in the same
/// scene, and LevelManager reads Instance.CurrentLevel from its own Awake
/// when Level01 is opened directly (not via MainMenu) — without this, that
/// read could race and silently fall back to level 1.
/// </summary>
[DefaultExecutionOrder(-100)]
public class LevelProgressManager : MonoBehaviour
{
    private const string CurrentLevelKey = "Progress.CurrentLevel";
    private const string HighestUnlockedLevelKey = "Progress.HighestUnlockedLevel";
    private const string CumulativeScoreKey = "Progress.CumulativeScore";
    private const string LevelStartScoreKey = "Progress.LevelStartScore";
    private const string LevelStartScoreLevelKey = "Progress.LevelStartScoreLevel";

    private const int DefaultLevel = 1;
    private const int DefaultCumulativeScore = 0;

    // No real level has this number, so the first-ever EstablishLevelStartCheckpoint
    // call always treats itself as entering a new level.
    private const int NoCheckpointLevel = -1;

    public static LevelProgressManager Instance { get; private set; }

    public int CurrentLevel { get; private set; }
    public int HighestUnlockedLevel { get; private set; }
    public int CumulativeScore { get; private set; }

    /// <summary>The score the active level checkpointed at when it was last freshly entered.</summary>
    public int LevelStartScore { get; private set; }

    private int levelStartScoreLevel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CurrentLevel = PlayerPrefs.GetInt(CurrentLevelKey, DefaultLevel);
        HighestUnlockedLevel = PlayerPrefs.GetInt(HighestUnlockedLevelKey, DefaultLevel);
        CumulativeScore = PlayerPrefs.GetInt(CumulativeScoreKey, DefaultCumulativeScore);
        LevelStartScore = PlayerPrefs.GetInt(LevelStartScoreKey, DefaultCumulativeScore);
        levelStartScoreLevel = PlayerPrefs.GetInt(LevelStartScoreLevelKey, NoCheckpointLevel);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void SetCurrentLevel(int levelNumber)
    {
        CurrentLevel = levelNumber;
        PlayerPrefs.SetInt(CurrentLevelKey, CurrentLevel);
        PlayerPrefs.Save();
    }

    /// <summary>Raises the highest unlocked level; never lowers it.</summary>
    public void UnlockLevel(int levelNumber)
    {
        if (levelNumber <= HighestUnlockedLevel)
        {
            return;
        }

        HighestUnlockedLevel = levelNumber;
        PlayerPrefs.SetInt(HighestUnlockedLevelKey, HighestUnlockedLevel);
        PlayerPrefs.Save();
    }

    public void SetCumulativeScore(int score)
    {
        CumulativeScore = score;
        PlayerPrefs.SetInt(CumulativeScoreKey, CumulativeScore);
        PlayerPrefs.Save();
    }

    public void AddCumulativeScore(int amount)
    {
        SetCumulativeScore(CumulativeScore + amount);
    }

    /// <summary>
    /// Returns the checkpoint a level should start from. The first time this is
    /// called for a given levelNumber (i.e. it differs from the level the stored
    /// checkpoint belongs to), the checkpoint is captured fresh from CumulativeScore
    /// and persisted. Calling it again for the same levelNumber — e.g. every Retry,
    /// including replays after already winning that level — returns the same
    /// stored value without re-capturing, so a win's commit to CumulativeScore
    /// never leaks into that level's own replays; it only takes effect once the
    /// active level actually changes (e.g. advancing to a future Level 2).
    /// </summary>
    public int EstablishLevelStartCheckpoint(int levelNumber)
    {
        if (levelStartScoreLevel != levelNumber)
        {
            LevelStartScore = CumulativeScore;
            levelStartScoreLevel = levelNumber;
            PlayerPrefs.SetInt(LevelStartScoreKey, LevelStartScore);
            PlayerPrefs.SetInt(LevelStartScoreLevelKey, levelStartScoreLevel);
            PlayerPrefs.Save();
        }

        return LevelStartScore;
    }

    /// <summary>Clears all progression PlayerPrefs and resets to defaults. For testing.</summary>
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(CurrentLevelKey);
        PlayerPrefs.DeleteKey(HighestUnlockedLevelKey);
        PlayerPrefs.DeleteKey(CumulativeScoreKey);
        PlayerPrefs.DeleteKey(LevelStartScoreKey);
        PlayerPrefs.DeleteKey(LevelStartScoreLevelKey);
        PlayerPrefs.Save();

        CurrentLevel = DefaultLevel;
        HighestUnlockedLevel = DefaultLevel;
        CumulativeScore = DefaultCumulativeScore;
        LevelStartScore = DefaultCumulativeScore;
        levelStartScoreLevel = NoCheckpointLevel;
    }
}
