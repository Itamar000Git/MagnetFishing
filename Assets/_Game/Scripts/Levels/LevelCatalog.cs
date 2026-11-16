using UnityEngine;

/// <summary>
/// Ordered set of LevelConfig assets for the campaign. LevelManager looks up
/// the active level number here each time it needs that level's rules.
/// </summary>
[CreateAssetMenu(fileName = "LevelCatalog", menuName = "Magnet Fishing/Level Catalog")]
public class LevelCatalog : ScriptableObject
{
    [SerializeField] private LevelConfig[] levels;

    /// <summary>Returns the LevelConfig whose LevelNumber matches, or null if none does.</summary>
    public LevelConfig GetLevel(int levelNumber)
    {
        if (levels == null)
        {
            return null;
        }

        foreach (LevelConfig config in levels)
        {
            if (config != null && config.LevelNumber == levelNumber)
            {
                return config;
            }
        }

        return null;
    }
}
