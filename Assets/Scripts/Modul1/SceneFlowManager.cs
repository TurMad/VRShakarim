using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class SceneFlowManager : MonoBehaviour
{
    public static SceneFlowManager Instance { get; private set; }

    [Header("Full XR Lock")]
    [SerializeField] private Behaviour[] fullLockDisableComponents;
    [SerializeField] private GameObject[] fullLockHideObjects;

    [Header("Move / Turn Lock")]
    [SerializeField] private Behaviour[] moveTurnDisableComponents;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Fade")]
    [SerializeField] private Volume fadeVolume;

    public bool IsAudioPlaying => audioSource != null && audioSource.isPlaying;

    public float AudioProgress
    {
        get
        {
            if (audioSource == null || audioSource.clip == null || audioSource.clip.length <= 0f)
                return 0f;

            return Mathf.Clamp01(audioSource.time / audioSource.clip.length);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetXRLocked(bool locked)
    {
        for (int i = 0; i < fullLockDisableComponents.Length; i++)
        {
            if (fullLockDisableComponents[i] != null)
                fullLockDisableComponents[i].enabled = !locked;
        }

        for (int i = 0; i < fullLockHideObjects.Length; i++)
        {
            if (fullLockHideObjects[i] != null)
                fullLockHideObjects[i].SetActive(!locked);
        }
    }

    public void SetMoveTurnLocked(bool locked)
    {
        for (int i = 0; i < moveTurnDisableComponents.Length; i++)
        {
            if (moveTurnDisableComponents[i] != null)
                moveTurnDisableComponents[i].enabled = !locked;
        }
    }

    public void PlayAudio(AudioClip clip, float volume = 1f)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.Stop();
        audioSource.loop = false;
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
    }

    public void StopAudio()
    {
        if (audioSource != null)
            audioSource.Stop();
    }

    public IEnumerator WaitForAudioFinished()
    {
        while (audioSource != null && audioSource.isPlaying)
            yield return null;
    }

    public IEnumerator FadeToBlack(float duration)
    {
        yield return FadeVolume(0f, 1f, duration);
    }

    public IEnumerator FadeFromBlack(float duration)
    {
        yield return FadeVolume(1f, 0f, duration);
    }

    public void SetObjectsActive(GameObject[] objects, bool value)
    {
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
                objects[i].SetActive(value);
        }
    }

    private IEnumerator FadeVolume(float from, float to, float duration)
    {
        if (fadeVolume == null)
            yield break;

        float time = 0f;
        fadeVolume.weight = from;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = duration <= 0f ? 1f : Mathf.Clamp01(time / duration);
            fadeVolume.weight = Mathf.Lerp(from, to, t);
            yield return null;
        }

        fadeVolume.weight = to;
    }
}