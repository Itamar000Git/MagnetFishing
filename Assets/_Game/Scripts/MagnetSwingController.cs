using UnityEngine;

/// <summary>
/// Swings this transform's local rotation smoothly left and right around its
/// resting orientation. Intended to be attached to the AimPivot GameObject in
/// Level01, so that Rope and Magnet (its children) swing along with it.
/// Rotation is driven from an internal phase accumulator (<see cref="swingTime"/>)
/// rather than the global clock, so the swing can be reset to a known phase
/// (rightmost) every time it resumes, instead of picking up wherever
/// Time.time happens to be.
/// </summary>
public class MagnetSwingController : MonoBehaviour
{
    [Tooltip("Maximum swing angle in degrees, symmetric around the resting forward direction. Placeholder value, not final.")]
    [SerializeField] private float maxSwingAngleDegrees = 40f;

    [Tooltip("Speed of the swing oscillation. Placeholder value, not final.")]
    [SerializeField] private float swingSpeed = 1.5f;

    /// <summary>Whether the idle swing is currently active. Set to false to freeze the aim (e.g. on launch).</summary>
    private bool isSwinging = true;

    // Local phase accumulator driving the swing, in place of Time.time. Using cos()
    // means swingTime == 0 evaluates to exactly +maxSwingAngleDegrees (rightmost),
    // matching the required "always resumes from rightmost" behavior.
    private float swingTime;

    public bool IsSwinging
    {
        get => isSwinging;
        set
        {
            isSwinging = value;

            if (isSwinging)
            {
                ResetToRightmost();
            }
        }
    }

    private void Awake()
    {
        ResetToRightmost();
    }

    private void Update()
    {
        if (!isSwinging)
        {
            return;
        }

        swingTime += Time.deltaTime;
        ApplySwingRotation();
    }

    /// <summary>
    /// Resets the swing phase to the rightmost extreme (+maxSwingAngleDegrees)
    /// and applies the resulting rotation immediately, so there is no one-frame
    /// lag between the reset and the visible pose.
    /// </summary>
    public void ResetToRightmost()
    {
        swingTime = 0f;
        ApplySwingRotation();
    }

    private void ApplySwingRotation()
    {
        float angle = Mathf.Cos(swingTime * swingSpeed) * maxSwingAngleDegrees;
        transform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}
