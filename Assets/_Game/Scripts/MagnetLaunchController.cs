using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles launch input and the full Idle -> Extending -> Retracting -> Idle
/// cycle. Attached alongside MagnetSwingController on AimPivot. On launch
/// input, freezes the swing (locking the current aim angle) and extends
/// Magnet outward along its own local -Y axis, stretching Rope to visually
/// follow it each frame. On reaching max extension, the magnet retracts back
/// to its resting position and the idle swing resumes automatically.
/// Owns catch-state decisions: MagnetCatchDetector (on the Magnet child)
/// forwards trigger contacts here via TryCatch, and this controller decides
/// whether a catch is valid and carries the caught object through retraction.
/// </summary>
[RequireComponent(typeof(MagnetSwingController))]
public class MagnetLaunchController : MonoBehaviour
{
    private enum MagnetState
    {
        Idle,
        Extending,
        Retracting
    }

    [Tooltip("Reference to the Rope child transform. Auto-found by name if left empty.")]
    [SerializeField] private Transform rope;

    [Tooltip("Reference to the Magnet child transform. Auto-found by name if left empty.")]
    [SerializeField] private Transform magnet;

    [Tooltip("Placeholder tuning value: how fast the magnet extends outward, in local units per second. Not final.")]
    [SerializeField] private float extensionSpeed = 3f;

    [Tooltip("Placeholder safety guard, temporary for the current level's water depth: maximum distance (local units) the magnet may extend from the pivot. Not finalized.")]
    [SerializeField] private float maxExtensionDistance = 6.9f;

    [Tooltip("Placeholder tuning value: how fast the magnet retracts back toward the pivot, in local units per second. Not final.")]
    [SerializeField] private float retractSpeed = 4f;

    [Tooltip("Placeholder tuning value, not final: object weight at or below this retracts at full (unscaled) retractSpeed.")]
    [SerializeField] private float minWeightForSpeedScaling = 0.5f;

    [Tooltip("Placeholder tuning value, not final: object weight at or above this retracts at the slowest speed (see heavyObjectSpeedMultiplier).")]
    [SerializeField] private float maxWeightForSpeedScaling = 5f;

    [Tooltip("Placeholder tuning value, not final: fraction of retractSpeed used for the heaviest supported object (e.g. 0.35 = 35% of normal speed). Clamped to a small positive floor at runtime.")]
    [SerializeField] private float heavyObjectSpeedMultiplier = 0.35f;

    [Tooltip("Placeholder tuning value, not final: absolute floor on the final computed retract speed so no combination of the weight-scaling fields can ever stall the retraction.")]
    [SerializeField] private float minEffectiveRetractSpeed = 0.5f;

    [Tooltip("Reference to the level's ScoreManager. Auto-found via FindFirstObjectByType if left empty.")]
    [SerializeField] private ScoreManager scoreManager;

    private MagnetSwingController swingController;
    private MagnetState currentState = MagnetState.Idle;
    private CatchableObject caughtObject;
    private float currentRetractSpeed;

    private Vector3 magnetRestingLocalPosition;
    private Vector3 ropeRestingLocalScale;
    private Vector3 ropeRestingLocalPosition;

    /// <summary>
    /// Gates launch input only. Clearing this does not interrupt a launch
    /// already in progress: the current cycle still runs to completion.
    /// </summary>
    public bool LaunchEnabled { get; set; } = true;

    /// <summary>
    /// True when no launch cycle is in progress. ReturnToIdle awards the caught
    /// object's score before entering Idle, so once this reads true after a
    /// retrieval, that retrieval's score is already in ScoreManager.
    /// </summary>
    public bool IsIdle => currentState == MagnetState.Idle;

    private void Awake()
    {
        swingController = GetComponent<MagnetSwingController>();

        if (rope == null)
        {
            rope = transform.Find("Rope");
        }

        if (magnet == null)
        {
            magnet = transform.Find("Magnet");
        }

        if (magnet != null)
        {
            magnetRestingLocalPosition = magnet.localPosition;
        }

        if (rope != null)
        {
            ropeRestingLocalScale = rope.localScale;
            ropeRestingLocalPosition = rope.localPosition;
        }

        if (scoreManager == null)
        {
            scoreManager = FindFirstObjectByType<ScoreManager>();
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case MagnetState.Idle:
                if (LaunchEnabled && LaunchInputTriggered())
                {
                    BeginExtending();
                }
                break;

            case MagnetState.Extending:
                UpdateExtending();
                break;

            case MagnetState.Retracting:
                UpdateRetracting();
                break;
        }
    }

    private bool LaunchInputTriggered()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            return true;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            return true;
        }

        return false;
    }

    private void BeginExtending()
    {
        currentState = MagnetState.Extending;
        swingController.IsSwinging = false;
    }

    private void UpdateExtending()
    {
        if (magnet == null)
        {
            return;
        }

        Vector3 magnetLocalPosition = magnet.localPosition;
        float currentDistance = Mathf.Abs(magnetLocalPosition.y);

        if (currentDistance < maxExtensionDistance)
        {
            magnetLocalPosition.y -= extensionSpeed * Time.deltaTime;

            if (Mathf.Abs(magnetLocalPosition.y) >= maxExtensionDistance)
            {
                magnetLocalPosition.y = -maxExtensionDistance;
            }

            magnet.localPosition = magnetLocalPosition;
        }

        UpdateRopeStretch();

        if (Mathf.Abs(magnet.localPosition.y) >= maxExtensionDistance)
        {
            BeginRetracting();
        }
    }

    private void BeginRetracting()
    {
        currentState = MagnetState.Retracting;
        currentRetractSpeed = ComputeEffectiveRetractSpeed();
    }

    /// <summary>
    /// Maps caught object weight to a retract speed. Weight is clamped to
    /// [minWeightForSpeedScaling, maxWeightForSpeedScaling] and linearly
    /// interpolated to a multiplier between 1.0 (full speed, light objects)
    /// and heavyObjectSpeedMultiplier (slow, heavy objects), then floored so
    /// retraction can never stall. Computed once per retraction, not per
    /// frame, since weight doesn't change mid-retraction.
    /// </summary>
    private float ComputeEffectiveRetractSpeed()
    {
        if (caughtObject == null)
        {
            return retractSpeed;
        }

        float clampedWeight = Mathf.Clamp(caughtObject.Weight, minWeightForSpeedScaling, maxWeightForSpeedScaling);
        float t = Mathf.InverseLerp(minWeightForSpeedScaling, maxWeightForSpeedScaling, clampedWeight);
        float safeMultiplier = Mathf.Max(heavyObjectSpeedMultiplier, 0.01f);
        float multiplier = Mathf.Lerp(1f, safeMultiplier, t);
        float effectiveSpeed = retractSpeed * multiplier;

        return Mathf.Max(effectiveSpeed, minEffectiveRetractSpeed);
    }

    /// <summary>
    /// Called by MagnetCatchDetector when the magnet's trigger touches a
    /// CatchableObject. Only takes effect while actively extending and only
    /// if nothing is currently being carried (this single null-check also
    /// prevents catching a second object while retracting, since caughtObject
    /// is already non-null by then).
    /// </summary>
    public void TryCatch(CatchableObject obj)
    {
        if (currentState != MagnetState.Extending || caughtObject != null || obj.IsCollected)
        {
            return;
        }

        caughtObject = obj;

        // Reparenting with world position preserved keeps the caught object
        // moving with the magnet during retraction with zero drift, with no
        // need for manual per-frame position syncing.
        obj.transform.SetParent(magnet, true);

        obj.MarkCaught();

        BeginRetracting();
    }

    private void UpdateRetracting()
    {
        if (magnet == null)
        {
            return;
        }

        Vector3 magnetLocalPosition = magnet.localPosition;
        magnetLocalPosition.y = Mathf.MoveTowards(magnetLocalPosition.y, magnetRestingLocalPosition.y, currentRetractSpeed * Time.deltaTime);
        magnet.localPosition = magnetLocalPosition;

        UpdateRopeStretch();

        if (Mathf.Approximately(magnetLocalPosition.y, magnetRestingLocalPosition.y))
        {
            ReturnToIdle();
        }
    }

    private void ReturnToIdle()
    {
        if (magnet != null)
        {
            magnet.localPosition = magnetRestingLocalPosition;
        }

        if (rope != null)
        {
            rope.localScale = ropeRestingLocalScale;
            rope.localPosition = ropeRestingLocalPosition;
        }

        if (caughtObject != null)
        {
            if (scoreManager != null)
            {
                scoreManager.AddScore(caughtObject.RewardValue);
            }

            caughtObject.MarkCollected();
            caughtObject = null;
        }

        currentState = MagnetState.Idle;
        swingController.IsSwinging = true;
    }

    private void UpdateRopeStretch()
    {
        if (rope == null || magnet == null)
        {
            return;
        }

        float length = Mathf.Abs(magnet.localPosition.y);

        Vector3 ropeScale = rope.localScale;
        ropeScale.y = length;
        rope.localScale = ropeScale;

        Vector3 ropePosition = rope.localPosition;
        ropePosition.y = magnet.localPosition.y / 2f;
        rope.localPosition = ropePosition;
    }
}
