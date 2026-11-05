using UnityEngine;

/// <summary>
/// Connects Level01's gameplay events to the persistent AudioManager. The manager
/// is looked up per event because it outlives this scene's objects.
/// </summary>
public class GameplayAudioEvents : MonoBehaviour
{
    [SerializeField] private MagnetLaunchController launchController;
    [SerializeField] private LevelManager levelManager;

    private void OnEnable()
    {
        launchController.Launched += HandleLaunched;
        launchController.ObjectCaught += HandleObjectCaught;
        launchController.RetractionStarted += HandleRetractionStarted;
        launchController.RetractionEnded += HandleRetractionEnded;
        levelManager.StateChanged += HandleLevelStateChanged;
    }

    private void OnDisable()
    {
        launchController.Launched -= HandleLaunched;
        launchController.ObjectCaught -= HandleObjectCaught;
        launchController.RetractionStarted -= HandleRetractionStarted;
        launchController.RetractionEnded -= HandleRetractionEnded;
        levelManager.StateChanged -= HandleLevelStateChanged;
    }

    private void HandleLaunched()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLaunchSfx();
        }
    }

    private void HandleObjectCaught(CatchableObject caught)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCatchSfx();
        }
    }

    private void HandleRetractionStarted()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StartRetractLoop();
        }
    }

    private void HandleRetractionEnded()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopRetractLoop();
        }
    }

    private void HandleLevelStateChanged(LevelState state)
    {
        if (AudioManager.Instance != null && state != LevelState.Playing)
        {
            AudioManager.Instance.PlayLevelEnding(state == LevelState.Won);
        }
    }
}
