using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles Main Menu button actions: Play, Settings (stub), Exit.
/// Wired to Button.onClick via the Inspector on the MenuContent GameObject.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string level01SceneName = "Level01";

    public void OnPlayButtonPressed()
    {
        SceneManager.LoadScene(level01SceneName);
    }

    public void OnSettingsButtonPressed()
    {
        // Settings menu is not implemented yet. Stub kept so the button
        // is safely wired and verifiable by QA without throwing errors.
        Debug.Log("Settings button pressed. Settings menu not implemented yet.");
    }

    public void OnExitButtonPressed()
    {
#if UNITY_EDITOR
        // Application.Quit() does nothing in the Editor, so log instead
        // to keep this path testable in Play Mode.
        Debug.Log("Exit button pressed. Application.Quit() is a no-op in the Editor.");
#else
        Application.Quit();
#endif
    }
}
