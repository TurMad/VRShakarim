using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class Module1GrabItem : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private Module1InteractionManager manager;
    [SerializeField] private InteractableHighlight highlight;

    [Header("Audio")]
    [SerializeField] private AudioClip interactionAudio;
    [SerializeField] private bool playAudioOnGrab;
    [SerializeField] private bool playAudioOnlyOnce = true;

    [Header("Long Grab Audio")]
    [Tooltip("Для домбры/длинной музыки. Аудио играет только пока предмет в руке. При отпускании ставится на паузу.")]
    [SerializeField] private bool playAudioOnlyWhileGrabbed;
    [SerializeField] private AudioSource longGrabAudioSource;

    [Header("Return")]
    [SerializeField] private float returnDuration = 0.35f;

    [Header("Special Effects")]
    [SerializeField] private DombraMusicParticles specialAudioParticles;

    private XRGrabInteractable grabInteractable;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private bool wasInteracted;
    private bool audioPlayed;
    private bool specialAudioActive;

    private float longGrabAudioTime;
    private bool longGrabAudioActive;
    private bool longGrabAudioPausedByRelease;

    private Coroutine returnRoutine;

    public XRGrabInteractable GrabInteractable => grabInteractable;
    public bool WasInteracted => wasInteracted;
    public bool IsGrabbed => grabInteractable != null && grabInteractable.isSelected;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (highlight == null)
            highlight = GetComponent<InteractableHighlight>();

        if (longGrabAudioSource == null)
            longGrabAudioSource = GetComponent<AudioSource>();

        if (playAudioOnlyWhileGrabbed && longGrabAudioSource == null)
            longGrabAudioSource = gameObject.AddComponent<AudioSource>();

        if (longGrabAudioSource != null)
        {
            longGrabAudioSource.playOnAwake = false;
            longGrabAudioSource.loop = false;
        }

        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void OnEnable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }

        StopLongGrabAudioCompletely();
    }

    private void Update()
    {
        if (!playAudioOnlyWhileGrabbed)
            return;

        if (longGrabAudioSource == null)
            return;

        if (!longGrabAudioActive)
            return;

        // Если длинное аудио само закончилось, пока предмет всё еще в руке.
        if (IsGrabbed && !longGrabAudioSource.isPlaying && !longGrabAudioPausedByRelease)
        {
            longGrabAudioActive = false;
            longGrabAudioTime = 0f;
            audioPlayed = true;

            if (specialAudioParticles != null)
                specialAudioParticles.StopEffect();

            if (manager != null)
                manager.ResumeBackgroundForLongGrabAudio();
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        SceneFlowManager.Instance.SetMoveTurnLocked(true);

        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        if (!wasInteracted)
        {
            wasInteracted = true;

            if (highlight != null)
                highlight.StopHighlight();

            if (manager != null)
                manager.NotifyItemInteracted(this);
        }

        if (!playAudioOnGrab || interactionAudio == null)
            return;

        if (playAudioOnlyWhileGrabbed)
        {
            PlayLongGrabAudio();
            return;
        }

        if (manager == null)
            return;

        if (playAudioOnlyOnce && audioPlayed)
            return;

        audioPlayed = true;
        manager.PlaySpecialItemAudio(this, interactionAudio);
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        if (playAudioOnlyWhileGrabbed)
        {
            PauseLongGrabAudio();

            if (manager == null || !manager.HasAnyGrabbedItems())
                SceneFlowManager.Instance.SetMoveTurnLocked(false);

            StartReturn();
            return;
        }

        if (!specialAudioActive)
        {
            if (manager == null || !manager.HasAnyGrabbedItems())
                SceneFlowManager.Instance.SetMoveTurnLocked(false);

            StartReturn();
        }
    }

    public void SetSpecialAudioState(bool active)
    {
        specialAudioActive = active;

        if (!specialAudioActive)
        {
            if (!IsGrabbed)
            {
                if (manager == null || !manager.HasAnyGrabbedItems())
                    SceneFlowManager.Instance.SetMoveTurnLocked(false);

                StartReturn();
            }
        }
    }

    private void PlayLongGrabAudio()
    {
        if (interactionAudio == null || longGrabAudioSource == null)
            return;

        if (playAudioOnlyOnce && audioPlayed && longGrabAudioTime <= 0f)
            return;

        if (manager != null)
            manager.PauseBackgroundForLongGrabAudio();

        longGrabAudioPausedByRelease = false;

        longGrabAudioSource.clip = interactionAudio;
        longGrabAudioSource.loop = false;

        float maxTime = Mathf.Max(0f, interactionAudio.length - 0.01f);
        longGrabAudioSource.time = Mathf.Clamp(longGrabAudioTime, 0f, maxTime);

        longGrabAudioSource.Play();
        longGrabAudioActive = true;

        if (specialAudioParticles != null)
            specialAudioParticles.PlayEffect();
    }

    private void PauseLongGrabAudio()
    {
        if (longGrabAudioSource == null)
            return;

        if (longGrabAudioSource.clip == interactionAudio)
            longGrabAudioTime = longGrabAudioSource.time;

        if (longGrabAudioSource.isPlaying)
            longGrabAudioSource.Pause();

        longGrabAudioPausedByRelease = true;
        longGrabAudioActive = false;

        if (specialAudioParticles != null)
            specialAudioParticles.StopEffect();

        if (manager != null)
            manager.ResumeBackgroundForLongGrabAudio();
    }

    private void StopLongGrabAudioCompletely()
    {
        if (longGrabAudioSource != null)
            longGrabAudioSource.Stop();

        longGrabAudioTime = 0f;
        longGrabAudioActive = false;
        longGrabAudioPausedByRelease = false;

        if (specialAudioParticles != null)
            specialAudioParticles.StopEffect();

        if (manager != null)
            manager.ResumeBackgroundForLongGrabAudio();
    }

    private void StartReturn()
    {
        if (returnRoutine != null)
            StopCoroutine(returnRoutine);

        returnRoutine = StartCoroutine(ReturnToStartRoutine());
    }

    private IEnumerator ReturnToStartRoutine()
    {
        Vector3 fromPosition = transform.position;
        Quaternion fromRotation = transform.rotation;

        float time = 0f;

        while (time < returnDuration)
        {
            time += Time.deltaTime;
            float t = returnDuration <= 0f ? 1f : Mathf.Clamp01(time / returnDuration);

            transform.position = Vector3.Lerp(fromPosition, startPosition, t);
            transform.rotation = Quaternion.Slerp(fromRotation, startRotation, t);

            yield return null;
        }

        transform.position = startPosition;
        transform.rotation = startRotation;

        returnRoutine = null;
    }

    public void OnSpecialAudioStarted()
    {
        if (specialAudioParticles != null)
            specialAudioParticles.PlayEffect();
    }

    public void OnSpecialAudioFinished()
    {
        if (specialAudioParticles != null)
            specialAudioParticles.StopEffect();
    }
}