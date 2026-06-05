using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class Module2GrabItem : MonoBehaviour
{
    [Header("Highlight")]
    [SerializeField] private InteractableHighlight highlight;

    [Header("Audio")]
    [SerializeField] private AudioClip interactionAudio;
    [SerializeField] private SubtitleSequenceSO interactionSubtitles;
    [SerializeField] private bool playAudioOnGrab;
    [SerializeField] private bool playAudioOnlyOnce = true;

    [Header("Return")]
    [SerializeField] private bool returnOnRelease = true;
    [SerializeField] private float returnDuration = 0.35f;

    private XRGrabInteractable grabInteractable;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private bool wasInteracted;
    private bool audioPlayed;
    private bool isGrabbed;
    private bool audioIsPlaying;
    private bool returnAfterAudio;

    private Coroutine returnRoutine;
    private Coroutine audioRoutine;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (highlight == null)
            highlight = GetComponent<InteractableHighlight>();

        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    private void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnReleased);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        returnAfterAudio = false;

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
        }

        TryPlayAudio();
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;

        if (audioIsPlaying)
        {
            returnAfterAudio = true;
            return;
        }

        SceneFlowManager.Instance.SetMoveTurnLocked(false);

        if (returnOnRelease)
            StartReturn();
    }

    private void TryPlayAudio()
    {
        if (!playAudioOnGrab || interactionAudio == null)
            return;

        if (playAudioOnlyOnce && audioPlayed)
            return;

        audioPlayed = true;

        if (audioRoutine != null)
            StopCoroutine(audioRoutine);

        audioRoutine = StartCoroutine(AudioRoutine());
    }

    private IEnumerator AudioRoutine()
    {
        audioIsPlaying = true;

        if (SubtitleManager.Instance != null && interactionSubtitles != null)
            SubtitleManager.Instance.PlaySequence(interactionSubtitles);

        SceneFlowManager.Instance.PlayAudio(interactionAudio);
        yield return SceneFlowManager.Instance.WaitForAudioFinished();

        if (SubtitleManager.Instance != null && interactionSubtitles != null)
            SubtitleManager.Instance.StopSequence();

        audioIsPlaying = false;
        audioRoutine = null;

        if (!isGrabbed)
        {
            SceneFlowManager.Instance.SetMoveTurnLocked(false);

            if (returnOnRelease && returnAfterAudio)
                StartReturn();
        }
    }

    private void StartReturn()
    {
        returnAfterAudio = false;

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
}