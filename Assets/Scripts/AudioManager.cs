using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sound Effects")]
    public AudioClip takeDamageClip;
    public AudioClip pickupKeyClip;
    public AudioClip openDoorClip;
    public AudioClip winClip;
    public AudioClip deathClip;

    [Header("Ambient")]
    public AudioClip ambientClip;

    [Header("Volume")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float ambientVolume = 0.4f;

    private AudioSource sfxSource;
    private AudioSource ambientSource;

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

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        ambientSource = gameObject.AddComponent<AudioSource>();
        ambientSource.playOnAwake = false;
        ambientSource.loop = true;
        ambientSource.volume = ambientVolume;
    }

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (ambientClip != null)
        {
            ambientSource.clip = ambientClip;
            ambientSource.Play();
        }
    }

    public static void PlayTakeDamage()
    {
        if (Instance != null && Instance.takeDamageClip != null)
            Instance.sfxSource.PlayOneShot(Instance.takeDamageClip, Instance.sfxVolume);
    }

    public static void PlayPickupKey()
    {
        if (Instance != null && Instance.pickupKeyClip != null)
            Instance.sfxSource.PlayOneShot(Instance.pickupKeyClip, Instance.sfxVolume);
    }

    public static void PlayOpenDoor()
    {
        if (Instance != null && Instance.openDoorClip != null)
            Instance.sfxSource.PlayOneShot(Instance.openDoorClip, Instance.sfxVolume);
    }

    public static void PlayWin()
    {
        if (Instance != null && Instance.winClip != null)
        {
            Instance.ambientSource.Stop();
            Instance.sfxSource.PlayOneShot(Instance.winClip, Instance.sfxVolume);
        }
    }

    public static void PlayDeath()
    {
        if (Instance != null && Instance.deathClip != null)
        {
            Instance.ambientSource.Stop();
            Instance.sfxSource.PlayOneShot(Instance.deathClip, Instance.sfxVolume);
        }
    }
}