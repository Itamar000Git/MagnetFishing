using UnityEngine;

/// <summary>
/// Data asset describing one level's rules. LevelManager resolves the active
/// level's instance of this at Awake and applies its duration/target.
/// </summary>
[CreateAssetMenu(fileName = "LevelConfig", menuName = "Magnet Fishing/Level Config")]
public class LevelConfig : ScriptableObject
{
    [SerializeField] private int levelNumber = 1;
    [SerializeField] private string displayName = "Level 1";
    [SerializeField] private float duration = 60f;
    [SerializeField] private int cumulativeTargetScore = 100;
    [SerializeField] private bool hazardsEnabled = false;

    [Tooltip("Identifies which scene-authored catchable/fish set is active for this level. Not consumed by any spawner yet; a future level-content system reads this to pick which objects to enable.")]
    [SerializeField] private string catchableSetId = "Level1Default";

    public int LevelNumber => levelNumber;
    public string DisplayName => displayName;
    public float Duration => duration;
    public int CumulativeTargetScore => cumulativeTargetScore;
    public bool HazardsEnabled => hazardsEnabled;
    public string CatchableSetId => catchableSetId;
}
