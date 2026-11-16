using System;
using UnityEngine;

/// <summary>
/// Activates the one scene-authored content root whose catchableSetId matches
/// the active level's LevelConfig, and deactivates the rest. The "smallest
/// clean system" for per-level content: each level's objects live under their
/// own root in the single reusable gameplay scene, and this just picks which
/// root is visible. Falls back to Level 1's catchableSetId (or the literal
/// "Level1Default") if the active level's config can't be resolved, so
/// content is never left fully inactive.
/// </summary>
public class LevelContentSelector : MonoBehaviour
{
    [Serializable]
    private struct ContentSet
    {
        public string catchableSetId;
        public GameObject root;
    }

    private const string FallbackCatchableSetId = "Level1Default";
    private const int FallbackLevelNumber = 1;

    [SerializeField] private LevelCatalog levelCatalog;
    [SerializeField] private ContentSet[] contentSets;

    private void Awake()
    {
        WarnAboutDuplicateContentSetIds();

        string activeSetId = ResolveActiveCatchableSetId();

        if (contentSets == null)
        {
            return;
        }

        foreach (ContentSet set in contentSets)
        {
            if (set.root != null)
            {
                set.root.SetActive(set.catchableSetId == activeSetId);
            }
        }
    }

    /// <summary>
    /// Development-time sanity check only: warns if two entries share a
    /// catchableSetId, which would make Awake's SetActive loop activate both
    /// (or neither deterministically) content roots for that id. Does not
    /// change ResolveActiveCatchableSetId's existing fallback behavior.
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    private void WarnAboutDuplicateContentSetIds()
    {
        if (contentSets == null)
        {
            return;
        }

        var seen = new System.Collections.Generic.HashSet<string>();
        foreach (ContentSet set in contentSets)
        {
            if (!string.IsNullOrEmpty(set.catchableSetId) && !seen.Add(set.catchableSetId))
            {
                Debug.LogWarning($"LevelContentSelector: duplicate catchableSetId \"{set.catchableSetId}\" in contentSets; only one of the matching roots' activation is meaningful.", this);
            }
        }
    }

    private string ResolveActiveCatchableSetId()
    {
        int activeLevel = LevelProgressManager.Instance != null ? LevelProgressManager.Instance.CurrentLevel : FallbackLevelNumber;
        LevelConfig config = levelCatalog != null ? levelCatalog.GetLevel(activeLevel) : null;

        if (config != null)
        {
            return config.CatchableSetId;
        }

        Debug.LogWarning("LevelContentSelector: no LevelConfig resolved for the active level; falling back to Level 1 content.", this);

        LevelConfig fallbackConfig = levelCatalog != null ? levelCatalog.GetLevel(FallbackLevelNumber) : null;
        return fallbackConfig != null ? fallbackConfig.CatchableSetId : FallbackCatchableSetId;
    }
}
