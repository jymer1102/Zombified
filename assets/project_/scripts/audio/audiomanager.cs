using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource ambienceSource;

    [Header("Weapon Audio Clips")]
    public AudioClip pistolShotClip;
    public AudioClip arShotClip;
    public AudioClip knifeSlashClip;
    public AudioClip reloadClip;

    [Header("Grenade Audio Clips")]
    public AudioClip grenadeThrowClip;
    public AudioClip explosionClip;

    [Header("Zombie Audio Clips")]
    public AudioClip zombieGroanClip;
    public AudioClip zombieDeathClip;
    public AudioClip zombieAttackClip;

    [Header("Interaction Audio Clips")]
    public AudioClip pickupClip;
    public AudioClip purchaseSuccessClip;

    [Header("UI / Feedback Audio Clips")]
    public AudioClip hitMarkerClip;
    public AudioClip lowHealthHeartbeatClip;
    public AudioClip levelCompleteClip;
    public AudioClip gameOverClip;

    [Header("Ambient Horror Layer")]
    [Tooltip("Looping low ambient drone/horror bed, plays underneath everything else.")]
    public AudioClip ambientHorrorLoop;

    private float masterVolume = 1.0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadSavedVolume();
    }

    void Start()
    {
        // Kick off the ambient horror bed automatically once volume is loaded
        if (ambienceSource != null && ambientHorrorLoop != null)
        {
            ambienceSource.clip = ambientHorrorLoop;
            ambienceSource.loop = true;
            ambienceSource.volume = masterVolume * 0.4f; // ambience sits quieter than SFX/music
            ambienceSource.Play();
        }
    }

    /// <summary>
    /// Pulls the saved volume from SaveSystem (if it exists) and applies it to all audio sources.
    /// Called once on startup so settings actually persist between play sessions.
    /// </summary>
    void LoadSavedVolume()
    {
        if (SaveSystem.Instance != null)
        {
            masterVolume = SaveSystem.Instance.LoadVolume();
        }

        ApplyVolume(masterVolume);
    }

    /// <summary>
    /// Applies a new master volume to all active sources and saves it for next time.
    /// Call this from a settings slider's OnValueChanged event.
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyVolume(masterVolume);

        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveSettings(masterVolume, SaveSystem.Instance.LoadSensitivity());
        }
    }

    void ApplyVolume(float volume)
    {
        if (musicSource != null) musicSource.volume = volume;
        if (sfxSource != null) sfxSource.volume = volume;
        if (ambienceSource != null) ambienceSource.volume = volume * 0.4f;
    }

    public float GetMasterVolume()
    {
        return masterVolume;
    }

    // Plays global background music (ambient horror tracks)
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource == null || clip == null) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    // Plays standard flat 2D sound effects (UI clicks, gunshots, inventory swaps)
    public void Play2DSFX(AudioClip clip, float volume = 1f)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, volume * masterVolume);
    }

    // CRITICAL FOR PEAK GRAPHICS/AUDIO: Plays sound in 3D space
    // The sound gets louder or quieter depending on where the player stands relative to the source!
    public void Play3DSFX(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        // Dynamically spawns a temporary audio source that cleans itself up
        AudioSource.PlayClipAtPoint(clip, position, volume * masterVolume);
    }
}
