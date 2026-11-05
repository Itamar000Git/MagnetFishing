using UnityEngine;

/// <summary>
/// Briefly punches the scale of a caught object. Only the caught object is
/// scaled: its colliders are already disabled by then, and the Magnet transform
/// is left alone because it carries the catch trigger.
/// </summary>
public class CatchPunchFeedback : MonoBehaviour
{
    [SerializeField] private MagnetLaunchController launchController;

    [Tooltip("Scale multiplier at the peak of the punch.")]
    [SerializeField] private float peakScale = 1.12f;

    [Tooltip("Total punch length in seconds.")]
    [SerializeField] private float duration = 0.16f;

    private Transform target;
    private Vector3 targetBaseScale;
    private float elapsed;

    private void OnEnable()
    {
        if (launchController != null)
        {
            launchController.ObjectCaught += HandleObjectCaught;
        }
    }

    private void OnDisable()
    {
        if (launchController != null)
        {
            launchController.ObjectCaught -= HandleObjectCaught;
        }

        EndPunch();
    }

    private void Update()
    {
        if (target == null)
        {
            return;
        }

        elapsed += Time.deltaTime;

        if (elapsed >= duration)
        {
            EndPunch();
            return;
        }

        float multiplier = Mathf.Lerp(1f, peakScale, Mathf.Sin(Mathf.PI * elapsed / duration));
        target.localScale = targetBaseScale * multiplier;
    }

    private void HandleObjectCaught(CatchableObject caught)
    {
        EndPunch();

        target = caught.transform;
        targetBaseScale = target.localScale;
        elapsed = 0f;
    }

    private void EndPunch()
    {
        if (target != null)
        {
            target.localScale = targetBaseScale;
            target = null;
        }
    }
}
