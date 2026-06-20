using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Weapon Audio Clips")]
    public AudioClip pistolShotClip;
    public AudioClip arShotClip;
    public AudioClip knifeSlashClip;
    public AudioClip reloadClip;

    [Header("Zombie Audio Clips")]
    public AudioClip zombieGroanClip;
    public AudioClip zombieDeathClip;

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
        }
    }

    // Plays global background music (ambient horror tracks)
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource == null || clip == null) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    // Plays standard flat 2D sound effects (UI clicks, inventory swaps)
    public void Play2DSFX(AudioClip clip, float volume = 1f)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    // CRITICAL FOR PEAK GRAPHICS/AUDIO: Plays sound in 3D space
    // The sound gets louder or quieter depending on where the player stands relative to the source!
    public void Play3DSFX(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        // Dynamically spawns a temporary audio source that cleans itself up
        AudioSource.PlayClipAtPoint(clip, position, volume);
    }
}
