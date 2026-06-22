using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;

public class Module1InteractionManager : MonoBehaviour
{
    [Header("Items")]
    [SerializeField] private Module1GrabItem[] allItems;

    [Header("Background Music / Narration")]
    [SerializeField] private AudioSource backgroundNarrationAudioSource;

    [Header("Subtitles")]
    [SerializeField] private SubtitleSequenceSO finalSubtitles;

    [Header("Transition To Final View")]
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private Transform nextViewPoint;
    [SerializeField] private float delayBeforeTransition = 1.2f;
    [SerializeField] private float fadeToBlackDuration = 0.25f;
    [SerializeField] private float fadeFromBlackDuration = 0.25f;

    [Header("Final Sequence")]
    [SerializeField] private AudioClip finalAudioClip;
    [SerializeField] private float delayBeforeFinalAudio = 0.7f;
    [SerializeField] private float delayAfterFinalAudio = 0.7f;
    [SerializeField] private bool enableGrabItemsAfterFinal = true;

    private int interactedCount;

    private bool specialAudioPlaying;
    private bool transitionStarted;
    private bool pendingTransition;

    private bool resumeBackgroundAfterItemAudio;
    private bool backgroundPausedByLongGrabAudio;

    private Coroutine waitTransitionRoutine;

    public bool HasAnyGrabbedItems()
    {
        for (int i = 0; i < allItems.Length; i++)
        {
            if (allItems[i] != null && allItems[i].IsGrabbed)
                return true;
        }

        return false;
    }

    public void NotifyItemInteracted(Module1GrabItem item)
    {
        interactedCount++;

        if (interactedCount < allItems.Length)
            return;

        pendingTransition = true;
        TryStartPendingTransition();
    }

    public void PlaySpecialItemAudio(Module1GrabItem sourceItem, AudioClip clip)
    {
        if (specialAudioPlaying || transitionStarted || clip == null)
            return;

        StartCoroutine(SpecialAudioRoutine(sourceItem, clip));
    }

    private IEnumerator SpecialAudioRoutine(Module1GrabItem sourceItem, AudioClip clip)
    {
        specialAudioPlaying = true;

        if (sourceItem != null)
        {
            sourceItem.SetSpecialAudioState(true);
            sourceItem.OnSpecialAudioStarted();
        }

        SetAllGrabInteractablesEnabled(false, sourceItem);

        PauseBackgroundNarrationIfNeeded();

        SceneFlowManager.Instance.PlayAudio(clip);
        yield return SceneFlowManager.Instance.WaitForAudioFinished();

        ResumeBackgroundNarrationIfNeeded();

        SetAllGrabInteractablesEnabled(true, null);

        if (sourceItem != null)
        {
            sourceItem.OnSpecialAudioFinished();
            sourceItem.SetSpecialAudioState(false);
        }

        specialAudioPlaying = false;

        TryStartPendingTransition();
    }

    private void PauseBackgroundNarrationIfNeeded()
    {
        resumeBackgroundAfterItemAudio = false;

        if (backgroundNarrationAudioSource == null)
            return;

        if (backgroundNarrationAudioSource.isPlaying)
        {
            backgroundNarrationAudioSource.Pause();
            resumeBackgroundAfterItemAudio = true;
        }
    }

    private void ResumeBackgroundNarrationIfNeeded()
    {
        if (!resumeBackgroundAfterItemAudio)
            return;

        if (backgroundNarrationAudioSource != null)
            backgroundNarrationAudioSource.UnPause();

        resumeBackgroundAfterItemAudio = false;
    }

    public void PauseBackgroundForLongGrabAudio()
    {
        if (backgroundNarrationAudioSource == null)
            return;

        if (backgroundNarrationAudioSource.isPlaying)
        {
            backgroundNarrationAudioSource.Pause();
            backgroundPausedByLongGrabAudio = true;
        }
    }

    public void ResumeBackgroundForLongGrabAudio()
    {
        if (!backgroundPausedByLongGrabAudio)
            return;

        if (backgroundNarrationAudioSource != null)
            backgroundNarrationAudioSource.UnPause();

        backgroundPausedByLongGrabAudio = false;
    }

    private void TryStartPendingTransition()
    {
        if (!pendingTransition)
            return;

        if (transitionStarted)
            return;

        if (specialAudioPlaying)
            return;

        if (waitTransitionRoutine != null)
            return;

        waitTransitionRoutine = StartCoroutine(WaitBeforeTransitionRoutine());
    }

    private IEnumerator WaitBeforeTransitionRoutine()
    {
        float timer = 0f;

        while (timer < delayBeforeTransition)
        {
            if (specialAudioPlaying || HasAnyGrabbedItems())
            {
                timer = 0f;
                yield return null;
                continue;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        waitTransitionRoutine = null;
        yield return TransitionRoutine();
    }

    private IEnumerator TransitionRoutine()
    {
        transitionStarted = true;
        pendingTransition = false;

        // Background music НЕ стопаем.
        // Он должен продолжать играть на финальной части.

        SceneFlowManager.Instance.SetMoveTurnLocked(true);
        SceneFlowManager.Instance.SetXRLocked(true);

        SetAllGrabInteractablesEnabled(false, null);

        yield return SceneFlowManager.Instance.FadeToBlack(fadeToBlackDuration);

        MoveXROriginToPoint();

        yield return SceneFlowManager.Instance.FadeFromBlack(fadeFromBlackDuration);

        yield return new WaitForSeconds(delayBeforeFinalAudio);

        if (finalAudioClip != null)
        {
            if (SubtitleManager.Instance != null && finalSubtitles != null)
                SubtitleManager.Instance.PlaySequence(finalSubtitles);

            SceneFlowManager.Instance.PlayAudio(finalAudioClip);
            yield return SceneFlowManager.Instance.WaitForAudioFinished();

            if (SubtitleManager.Instance != null)
                SubtitleManager.Instance.StopSequence();
        }

        yield return new WaitForSeconds(delayAfterFinalAudio);

        // Больше НЕ переходим обратно в Intro.
        // Просто возвращаем управление, чтобы игрок мог смотреть помещение дальше.
        SceneFlowManager.Instance.SetXRLocked(false);
        SceneFlowManager.Instance.SetMoveTurnLocked(false);

        if (enableGrabItemsAfterFinal)
            SetAllGrabInteractablesEnabled(true, null);
    }

    private void SetAllGrabInteractablesEnabled(bool value, Module1GrabItem exceptItem)
    {
        for (int i = 0; i < allItems.Length; i++)
        {
            if (allItems[i] == null || allItems[i].GrabInteractable == null)
                continue;

            if (exceptItem != null && allItems[i] == exceptItem)
                continue;

            allItems[i].GrabInteractable.enabled = value;
        }
    }

    private void MoveXROriginToPoint()
    {
        if (xrOrigin == null || nextViewPoint == null)
            return;

        Transform cameraTransform = xrOrigin.Camera.transform;

        Vector3 cameraOffset = xrOrigin.transform.position - cameraTransform.position;
        cameraOffset.y = 0f;

        xrOrigin.transform.position = nextViewPoint.position + cameraOffset;

        Vector3 forward = nextViewPoint.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude > 0.001f)
            xrOrigin.transform.rotation = Quaternion.LookRotation(forward);
    }
}