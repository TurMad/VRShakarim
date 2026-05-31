using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class Module1GrabItem : MonoBehaviour
{
    [SerializeField] private Module1InteractionManager manager;
    [SerializeField] private InteractableHighlight highlight;
    [SerializeField] private AudioClip interactionAudio;
    [SerializeField] private bool playAudioOnGrab;
    [SerializeField] private bool playAudioOnlyOnce = true;
    [SerializeField] private float returnDuration = 0.35f;
    
    [SerializeField] private DombraMusicParticles specialAudioParticles;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private bool wasInteracted;
    private bool audioPlayed;
    private bool specialAudioActive;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private Coroutine returnRoutine;

    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable GrabInteractable => grabInteractable;
    public bool WasInteracted => wasInteracted;
    public bool IsGrabbed => grabInteractable != null && grabInteractable.isSelected;

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

            if (manager != null)
                manager.NotifyItemInteracted(this);
        }

        if (!playAudioOnGrab || interactionAudio == null || manager == null)
            return;

        if (playAudioOnlyOnce && audioPlayed)
            return;

        audioPlayed = true;
        manager.PlaySpecialItemAudio(this, interactionAudio);
    }

    private void OnReleased(SelectExitEventArgs args)
    {
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

    private void StartReturn()
    {
        if (returnRoutine != null)
            StopCoroutine(returnRoutine);

        returnRoutine = StartCoroutine(ReturnToStartRoutine());
    }

    private IEnumerator ReturnToStartRoutine()
    {
        Vector3 fromPos = transform.position;
        Quaternion fromRot = transform.rotation;

        float time = 0f;

        while (time < returnDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / returnDuration);

            transform.position = Vector3.Lerp(fromPos, startPosition, t);
            transform.rotation = Quaternion.Slerp(fromRot, startRotation, t);

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