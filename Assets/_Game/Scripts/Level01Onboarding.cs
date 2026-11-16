using System.Collections;
using UnityEngine;

/// <summary>
/// Shows the How to Play overlay the first time Level01 starts, holding the
/// timer and launch input until it closes. The swing and fish keep moving
/// behind the overlay, which is why this does not touch Time.timeScale.
/// </summary>
public class Level01Onboarding : MonoBehaviour
{
    [SerializeField] private OnboardingOverlay overlay;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private MagnetLaunchController launchController;

    // Runs in Awake so gameplay is already held on the level's first Update.
    private void Awake()
    {
        if (OnboardingOverlay.IsCompleted)
        {
            return;
        }

        SetGameplayHeld(true);
        overlay.Closed += HandleOverlayClosed;
        overlay.ShowOnboarding();
    }

    private void OnDestroy()
    {
        if (overlay != null)
        {
            overlay.Closed -= HandleOverlayClosed;
        }
    }

    private void HandleOverlayClosed()
    {
        overlay.Closed -= HandleOverlayClosed;
        StartCoroutine(ReleaseGameplayNextFrame());
    }

    // The input that pressed the closing button can still read as "pressed this
    // frame" (a very fast tap, or Space on a selected button). Waiting a frame
    // stops it from also launching the magnet.
    private IEnumerator ReleaseGameplayNextFrame()
    {
        yield return null;
        SetGameplayHeld(false);
    }

    private void SetGameplayHeld(bool held)
    {
        levelManager.TimerPaused = held;
        launchController.LaunchEnabled = !held;
    }
}
