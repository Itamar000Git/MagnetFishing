using UnityEngine;

/// <summary>
/// Marks a GameObject as something the magnet can catch and carries its
/// placeholder data. Attach to any object the magnet should pick up (e.g. a
/// test target, later: fish hooks, scrap metal, treasure).
/// </summary>
public class CatchableObject : MonoBehaviour
{
    [Tooltip("Placeholder for a future retraction-speed system: heavier objects should retract slower. Not wired up yet.")]
    [SerializeField] private float weight = 1f;

    [Tooltip("Placeholder for a future scoring system: value awarded when this object is collected. Not wired up yet.")]
    [SerializeField] private int rewardValue = 10;

    public float Weight => weight;
    public int RewardValue => rewardValue;
}
