using UnityEngine;

/// <summary>
/// Swings this transform's local rotation smoothly left and right around its
/// resting orientation. Intended to be attached to the AimPivot GameObject in
/// Level01, so that Rope and Magnet (its children) swing along with it.
/// Rotation is driven from a continuous sine wave rather than accumulated
/// deltas, so toggling <see cref="isSwinging"/> off/on later (e.g. when the
/// magnet launches) will simply freeze/resume it without extra bookkeeping.
/// </summary>
public class MagnetSwingController : MonoBehaviour
{
    [Tooltip("Maximum swing angle in degrees, symmetric around the resting forward direction. Placeholder value, not final.")]
    [SerializeField] private float maxSwingAngleDegrees = 40f;

    [Tooltip("Speed of the swing oscillation. Placeholder value, not final.")]
    [SerializeField] private float swingSpeed = 1.5f;

    /// <summary>Whether the idle swing is currently active. Set to false to freeze the aim (e.g. on launch).</summary>
    private bool isSwinging = true;

    private Quaternion restingLocalRotation;

    public bool IsSwinging
    {
        get => isSwinging;
        set => isSwinging = value;
    }

    private void Awake()
    {
        restingLocalRotation = transform.localRotation;
    }

    private void Update()
    {
        if (!isSwinging)
        {
            return;
        }

        float angle = Mathf.Sin(Time.time * swingSpeed) * maxSwingAngleDegrees;
        transform.localRotation = restingLocalRotation * Quaternion.Euler(0f, 0f, angle);
    }
}
