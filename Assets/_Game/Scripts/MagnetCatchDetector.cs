using UnityEngine;

/// <summary>
/// Sits on the Magnet GameObject and forwards 2D trigger contacts with
/// catchable objects up to MagnetLaunchController (on the parent AimPivot),
/// which owns all catch-validity decisions. Intentionally contains no logic
/// of its own beyond detecting and forwarding the contact.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MagnetCatchDetector : MonoBehaviour
{
    [Tooltip("Reference to the MagnetLaunchController that owns catch-state decisions. Auto-found on the parent if left empty.")]
    [SerializeField] private MagnetLaunchController launchController;

    private void Awake()
    {
        if (launchController == null)
        {
            launchController = GetComponentInParent<MagnetLaunchController>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // The catch collider may be a child of the catchable, like a fish's Hook.
        CatchableObject catchable = other.GetComponentInParent<CatchableObject>();

        if (catchable != null && launchController != null)
        {
            launchController.TryCatch(catchable);
        }
    }
}
