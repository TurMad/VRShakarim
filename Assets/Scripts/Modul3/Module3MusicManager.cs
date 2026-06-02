using UnityEngine;

public class Module3MusicManager : MonoBehaviour
{
    public static Module3MusicManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;

    private AudioClip currentClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public static void PlayFromStart(AudioClip clip)
    {
        if (clip == null)
            return;

        EnsureInstance();
        Instance.InternalPlayFromStart(clip);
    }

    public static void ContinueOrPlay(AudioClip clip)
    {
        if (clip == null)
            return;

        EnsureInstance();
        Instance.InternalContinueOrPlay(clip);
    }

    public static void StopMusic()
    {
        if (Instance == null || Instance.audioSource == null)
            return;

        Instance.audioSource.Stop();
        Instance.currentClip = null;
    }

    private void InternalPlayFromStart(AudioClip clip)
    {
        if (audioSource == null)
            return;

        currentClip = clip;
        audioSource.clip = clip;
        audioSource.time = 0f;
        audioSource.loop = true;
        audioSource.Play();
    }

    private void InternalContinueOrPlay(AudioClip clip)
    {
        if (audioSource == null)
            return;

        if (audioSource.isPlaying && currentClip == clip)
            return;

        if (currentClip == clip && !audioSource.isPlaying)
        {
            audioSource.UnPause();
            if (!audioSource.isPlaying)
                audioSource.Play();
            return;
        }

        currentClip = clip;
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }

    private static void EnsureInstance()
    {
        if (Instance != null)
            return;

        GameObject go = new GameObject("Module3MusicManager");
        AudioSource source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        go.AddComponent<Module3MusicManager>();
    }
}