using System.Collections;
using UnityEngine;

public class Module1InteractionManager : MonoBehaviour
{
    [Header("All Interactive Items")]
    [SerializeField] private Module1GrabItem[] allItems;

    [Header("Background Music / Narration")]
    [SerializeField] private AudioSource backgroundNarrationAudioSource;

    private bool specialAudioPlaying;

    private bool resumeBackgroundAfterItemAudio;
    private bool backgroundPausedByLongGrabAudio;

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
        // После взаимодействия со всеми предметами больше нет финальной части.
        // Нет перехода, Next View Point, финального аудио и загрузки другой сцены.
    }

    public void PlaySpecialItemAudio(Module1GrabItem sourceItem, AudioClip clip)
    {
        if (specialAudioPlaying || clip == null)
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
}