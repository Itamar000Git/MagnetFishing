using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The game's single persistent audio player. A prefab instance sits in every
/// scene so any scene can be played directly; the first one survives scene
/// loads and later copies remove themselves. Music, SFX and the retract loop
/// each have one AudioSource, so tracks can never overlap.
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Serializable]
    private struct SoundEffect
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
    }

    private const string MusicEnabledKey = "Audio.MusicEnabled";
    private const string SfxEnabledKey = "Audio.SfxEnabled";
    private const string MusicTrackKey = "Audio.MusicTrack";

    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource retractLoopSource;

    [Header("Music")]
    [SerializeField] private AudioClip[] musicTracks;

    [Tooltip("Track used until the player has selected one.")]
    [SerializeField] private int defaultMusicTrack;

    [Tooltip("Rough mix level that keeps music behind gameplay sounds. Not final.")]
    [Range(0f, 1f)]
    [SerializeField] private float musicMixVolume = 0.2f;

    [Tooltip("Fraction of the normal music volume kept while a level-ending sound plays.")]
    [Range(0f, 1f)]
    [SerializeField] private float levelEndMusicScale = 0.25f;

    [Tooltip("Seconds taken to fade music down at level end, or back up afterwards.")]
    [SerializeField] private float musicFadeSeconds = 0.3f;

    [Header("SFX (volumes are a rough mix, not final)")]
    [SerializeField] private SoundEffect launchSfx;
    [SerializeField] private SoundEffect catchSfx;
    [SerializeField] private SoundEffect retractLoopSfx;
    [SerializeField] private SoundEffect levelCompleteSfx;
    [SerializeField] private SoundEffect levelFailedSfx;

    private float musicVolume = 1f;
    private float sfxVolume = 1f;
    private float musicScale = 1f;
    private float musicScaleTarget = 1f;

    public bool MusicEnabled { get; private set; } = true;
    public bool SfxEnabled { get; private set; } = true;
    public int CurrentMusicTrack { get; private set; }
    public int MusicTrackCount => musicTracks != null ? musicTracks.Length : 0;

    /// <summary>Player music volume from 0 to 1, applied on top of the mix level.</summary>
    public float MusicVolume => musicVolume;

    /// <summary>Player SFX volume from 0 to 1, applied on top of each sound's mix level.</summary>
    public float SfxVolume => sfxVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += HandleSceneLoaded;

        MusicEnabled = PlayerPrefs.GetInt(MusicEnabledKey, 1) == 1;
        SfxEnabled = PlayerPrefs.GetInt(SfxEnabledKey, 1) == 1;

        musicSource.loop = true;
        retractLoopSource.loop = true;
        retractLoopSource.clip = retractLoopSfx.clip;
        RefreshMusicVolume();
        RefreshRetractLoopVolume();

        StartTrack(PlayerPrefs.GetInt(MusicTrackKey, defaultMusicTrack));
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            Instance = null;
        }
    }

    private void Update()
    {
        if (Mathf.Approximately(musicScale, musicScaleTarget))
        {
            return;
        }

        float step = Time.unscaledDeltaTime / Mathf.Max(musicFadeSeconds, 0.01f);
        musicScale = Mathf.MoveTowards(musicScale, musicScaleTarget, step);
        RefreshMusicVolume();
    }

    /// <summary>
    /// Selects a track and remembers it. Selecting the track already playing
    /// leaves it running, so it carries on uninterrupted. An out-of-range index,
    /// such as a saved track that no longer exists, falls back to the first.
    /// </summary>
    public void PlayMusic(int trackIndex)
    {
        if (MusicTrackCount == 0)
        {
            return;
        }

        StartTrack(trackIndex);
        PlayerPrefs.SetInt(MusicTrackKey, CurrentMusicTrack);
        PlayerPrefs.Save();
    }

    public void NextMusicTrack()
    {
        StepMusicTrack(1);
    }

    public void PreviousMusicTrack()
    {
        StepMusicTrack(-1);
    }

    /// <summary>Turning music off stops it; turning it back on restarts the current track.</summary>
    public void SetMusicEnabled(bool enabled)
    {
        MusicEnabled = enabled;
        PlayerPrefs.SetInt(MusicEnabledKey, enabled ? 1 : 0);
        PlayerPrefs.Save();

        if (!enabled)
        {
            musicSource.Stop();
        }
        else if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        RefreshMusicVolume();
    }

    public void SetSfxEnabled(bool enabled)
    {
        SfxEnabled = enabled;
        PlayerPrefs.SetInt(SfxEnabledKey, enabled ? 1 : 0);
        PlayerPrefs.Save();

        if (!enabled)
        {
            sfxSource.Stop();
            retractLoopSource.Stop();
        }
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        RefreshRetractLoopVolume();
    }

    public void PlayLaunchSfx()
    {
        PlaySfx(launchSfx);
    }

    public void PlayCatchSfx()
    {
        PlaySfx(catchSfx);
    }

    public void StartRetractLoop()
    {
        if (SfxEnabled && retractLoopSource.clip != null && !retractLoopSource.isPlaying)
        {
            retractLoopSource.Play();
        }
    }

    public void StopRetractLoop()
    {
        retractLoopSource.Stop();
    }

    /// <summary>Fades the music down and plays the ending sound. Undone by the next scene load.</summary>
    public void PlayLevelEnding(bool won)
    {
        StopRetractLoop();
        musicScaleTarget = levelEndMusicScale;
        PlaySfx(won ? levelCompleteSfx : levelFailedSfx);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sfxSource.Stop();
        StopRetractLoop();
        musicScaleTarget = 1f;
    }

    private void StartTrack(int trackIndex)
    {
        if (MusicTrackCount == 0)
        {
            return;
        }

        CurrentMusicTrack = trackIndex >= 0 && trackIndex < MusicTrackCount ? trackIndex : 0;
        AudioClip track = musicTracks[CurrentMusicTrack];

        if (musicSource.clip != track)
        {
            musicSource.Stop();
            musicSource.clip = track;
        }

        if (MusicEnabled && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    private void StepMusicTrack(int direction)
    {
        if (MusicTrackCount > 0)
        {
            PlayMusic((CurrentMusicTrack + direction + MusicTrackCount) % MusicTrackCount);
        }
    }

    private void PlaySfx(SoundEffect sound)
    {
        if (SfxEnabled && sound.clip != null)
        {
            sfxSource.PlayOneShot(sound.clip, sound.volume * sfxVolume);
        }
    }

    private void RefreshMusicVolume()
    {
        musicSource.volume = musicMixVolume * musicVolume * musicScale;
    }

    private void RefreshRetractLoopVolume()
    {
        retractLoopSource.volume = retractLoopSfx.volume * sfxVolume;
    }
}
