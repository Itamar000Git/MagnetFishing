using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles launch input and the full Idle -> Extending -> Retracting -> Idle
/// cycle. Attached alongside MagnetSwingController on AimPivot. On launch
/// input, freezes the swing (locking the current aim angle) and extends
/// Magnet outward along its own local -Y axis, stretching Rope to visually
/// follow it each frame. On reaching max extension, the magnet retracts back
/// to its resting position and the idle swing resumes automatically.
/// Catch detection is not implemented here yet.
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

    private MagnetSwingController swingController;
    private MagnetState currentState = MagnetState.Idle;

    private Vector3 magnetRestingLocalPosition;
    private Vector3 ropeRestingLocalScale;
    private Vector3 ropeRestingLocalPosition;

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
    }

    private void Update()
    {
        switch (currentState)
        {
            case MagnetState.Idle:
                if (LaunchInputTriggered())
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
    }

    private void UpdateRetracting()
    {
        if (magnet == null)
        {
            return;
        }

        Vector3 magnetLocalPosition = magnet.localPosition;
        magnetLocalPosition.y = Mathf.MoveTowards(magnetLocalPosition.y, magnetRestingLocalPosition.y, retractSpeed * Time.deltaTime);
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
