using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles Main Menu button actions: Play, Settings, Exit.
/// Wired to Button.onClick via the Inspector on the MenuContent GameObject.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string level01SceneName = "Level01";
    [SerializeField] private AudioSettingsPanel settingsPanel;

    public void OnPlayButtonPressed()
    {
        SceneManager.LoadScene(level01SceneName);
    }

    public void OnSettingsButtonPressed()
    {
        settingsPanel.Open();
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
