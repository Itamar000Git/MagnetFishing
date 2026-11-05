using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main Menu settings panel for music and SFX. It holds no audio state: controls
/// show the persistent AudioManager's values on open and forward changes to it.
/// The manager is looked up per use because the scene's own instance is a
/// duplicate that removes itself when returning from another scene.
/// </summary>
public class AudioSettingsPanel : MonoBehaviour
{
    [Serializable]
    private struct ChannelControls
    {
        public Toggle enabledToggle;
        public Image toggleBackground;
        public Text toggleLabel;
        public Slider volumeSlider;
    }

    [SerializeField] private ChannelControls music;
    [SerializeField] private ChannelControls sfx;
    [SerializeField] private Color toggleOnColor = new Color(0.2f, 0.55f, 0.3f);
    [SerializeField] private Color toggleOffColor = new Color(0.35f, 0.35f, 0.4f);

    // Awake runs once, on first open, so reopening cannot add listeners twice.
    private void Awake()
    {
        music.enabledToggle.onValueChanged.AddListener(HandleMusicToggled);
        music.volumeSlider.onValueChanged.AddListener(HandleMusicVolumeChanged);
        sfx.enabledToggle.onValueChanged.AddListener(HandleSfxToggled);
        sfx.volumeSlider.onValueChanged.AddListener(HandleSfxVolumeChanged);
    }

    private void OnEnable()
    {
        AudioManager audio = AudioManager.Instance;
        if (audio != null)
        {
            ShowChannel(music, audio.MusicEnabled, audio.MusicVolume);
            ShowChannel(sfx, audio.SfxEnabled, audio.SfxVolume);
        }
    }

    private void OnDisable()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SaveSettings();
        }
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void HandleMusicToggled(bool isOn)
    {
        ShowToggleState(music, isOn);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicEnabled(isOn);
        }
    }

    private void HandleMusicVolumeChanged(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(volume);
        }
    }

    private void HandleSfxToggled(bool isOn)
    {
        ShowToggleState(sfx, isOn);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSfxEnabled(isOn);
        }
    }

    private void HandleSfxVolumeChanged(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSfxVolume(volume);
        }
    }

    private void ShowChannel(ChannelControls channel, bool enabled, float volume)
    {
        channel.enabledToggle.SetIsOnWithoutNotify(enabled);
        channel.volumeSlider.SetValueWithoutNotify(volume);
        ShowToggleState(channel, enabled);
    }

    private void ShowToggleState(ChannelControls channel, bool isOn)
    {
        channel.toggleBackground.color = isOn ? toggleOnColor : toggleOffColor;
        channel.toggleLabel.text = isOn ? "ON" : "OFF";
    }
}
