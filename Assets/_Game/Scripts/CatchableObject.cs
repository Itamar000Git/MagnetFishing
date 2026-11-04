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

    [Tooltip("Debug/display identity for this catchable instance (e.g. \"MetalScrap\", \"Bicycle\"). Purely for identification/logging, not used by gameplay logic yet.")]
    [SerializeField] private string objectName = "Catchable Object";

    public float Weight => weight;
    public int RewardValue => rewardValue;
    public string ObjectName => objectName;

    private bool isCollected;
    public bool IsCollected => isCollected;

    private bool isCaught;

    /// <summary>True from the moment the magnet catches this object.</summary>
    public bool IsCaught => isCaught;

    /// <summary>
    /// Marks this object as caught and disables its colliders, including those
    /// on children such as a fish's Hook, so a carried object can't produce
    /// further contacts. Called by MagnetLaunchController when a catch is accepted.
    /// </summary>
    public void MarkCaught()
    {
        isCaught = true;

        foreach (Collider2D objectCollider in GetComponentsInChildren<Collider2D>())
        {
            objectCollider.enabled = false;
        }
    }

    /// <summary>
    /// Marks this object as collected and removes it from visible gameplay.
    /// Deactivating (rather than destroying) keeps it simple to inspect/debug
    /// and leaves room for a future scoring system to read its data afterward
    /// if needed. Safe to call more than once; only takes effect the first time.
    /// </summary>
    public void MarkCollected()
    {
        if (isCollected)
        {
            return;
        }

        isCollected = true;
        gameObject.SetActive(false);
    }
}
