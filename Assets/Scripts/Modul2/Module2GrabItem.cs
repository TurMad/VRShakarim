using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class Module2GrabItem : MonoBehaviour
{
    [Header("Highlight")]
    [SerializeField] private InteractableHighlight highlight;

    [Header("Audio")]
    [SerializeField] private AudioClip interactionAudio;
    [SerializeField] private bool playAudioOnGrab;
    [SerializeField] private bool playAudioOnlyOnce = true;

    [Header("Return")]
    [SerializeField] private bool returnOnRelease = true;
    [SerializeField] private float returnDuration = 0.35f;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private bool wasInteracted;
    private bool audioPlayed;
    private Coroutine returnRoutine;

    private void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

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

        if (!playAudioOnGrab || interactionAudio == null)
            return;

        if (playAudioOnlyOnce && audioPlayed)
            return;

        audioPlayed = true;
        SceneFlowManager.Instance.PlayAudio(interactionAudio);
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        SceneFlowManager.Instance.SetMoveTurnLocked(false);

        if (returnOnRelease)
            StartReturn();
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
            float t = Mathf.Clamp01(time / returnDuration);

            transform.position = Vector3.Lerp(fromPosition, startPosition, t);
            transform.rotation = Quaternion.Slerp(fromRotation, startRotation, t);

            yield return null;
        }

        transform.position = startPosition;
        transform.rotation = startRotation;

        returnRoutine = null;
    }
}