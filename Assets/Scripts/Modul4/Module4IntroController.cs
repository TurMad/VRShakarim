using System.Collections;
using UnityEngine;

public class Module4IntroController : MonoBehaviour
{
    [Header("First Intro Audio")]
    [SerializeField] private AudioClip firstIntroVoiceClip;
    [SerializeField] private SubtitleSequenceSO firstIntroSubtitles;
    [SerializeField] private float delayBeforeFirstIntroAudio = 0.5f;

    [Header("Fade")]
    [SerializeField] private float fadeFromBlackDuration = 1f;

    [Header("Door Becomes Available After First Audio")]
    [Tooltip("Добавь сюда Module4DoorInteractable и XRSimpleInteractable двери.")]
    [SerializeField] private Behaviour[] interactablesToEnable;

    [SerializeField] private InteractableHighlight[] highlightsToStart;
    [SerializeField] private float delayBeforeHighlights = 0.25f;

    [Header("Optional Instruction Audio")]
    [Tooltip("Необязательно. Аудио играет после разблокировки игрока и не блокирует движение.")]
    [SerializeField] private AudioClip instructionAudio;
    [SerializeField] private float delayBeforeInstructionAudio = 0.25f;

    [Header("Teleport Alternative")]
    [SerializeField] private SimpleXRTeleportByButton teleportByButton;

    private IEnumerator Start()
    {
        SceneFlowManager.Instance.SetXRLocked(true);
        SceneFlowManager.Instance.SetMoveTurnLocked(true);

        if (teleportByButton != null)
            teleportByButton.enabled = false;

        DisableInteractionComponents();
        StopHighlights();

        yield return SceneFlowManager.Instance.FadeFromBlack(fadeFromBlackDuration);

        yield return new WaitForSeconds(delayBeforeFirstIntroAudio);

        yield return PlayAudioWithSubtitles(firstIntroVoiceClip, firstIntroSubtitles);

        // После первого аудио игрок получает полную свободу.
        SceneFlowManager.Instance.SetXRLocked(false);
        SceneFlowManager.Instance.SetMoveTurnLocked(false);

        if (teleportByButton != null)
            teleportByButton.enabled = true;

        EnableInteractionComponents();

        // Даем активированным объектам один кадр, чтобы они корректно включились.
        yield return null;
        yield return new WaitForSeconds(delayBeforeHighlights);

        StartHighlights();

        // Инструкция не блокирует игрока.
        if (instructionAudio != null)
            StartCoroutine(PlayInstructionAudioRoutine());
    }

    private IEnumerator PlayAudioWithSubtitles(AudioClip audioClip, SubtitleSequenceSO subtitles)
    {
        if (audioClip == null)
            yield break;

        if (SubtitleManager.Instance != null && subtitles != null)
            SubtitleManager.Instance.PlaySequence(subtitles);

        SceneFlowManager.Instance.PlayAudio(audioClip);
        yield return SceneFlowManager.Instance.WaitForAudioFinished();

        if (SubtitleManager.Instance != null)
            SubtitleManager.Instance.StopSequence();
    }

    private IEnumerator PlayInstructionAudioRoutine()
    {
        yield return new WaitForSeconds(delayBeforeInstructionAudio);

        SceneFlowManager.Instance.PlayAudio(instructionAudio);
    }

    private void DisableInteractionComponents()
    {
        for (int i = 0; i < interactablesToEnable.Length; i++)
        {
            if (interactablesToEnable[i] != null)
                interactablesToEnable[i].enabled = false;
        }
    }

    private void EnableInteractionComponents()
    {
        for (int i = 0; i < interactablesToEnable.Length; i++)
        {
            if (interactablesToEnable[i] != null)
                interactablesToEnable[i].enabled = true;
        }
    }

    private void StartHighlights()
    {
        for (int i = 0; i < highlightsToStart.Length; i++)
        {
            if (highlightsToStart[i] != null)
                highlightsToStart[i].StartHighlight();
        }
    }

    private void StopHighlights()
    {
        for (int i = 0; i < highlightsToStart.Length; i++)
        {
            if (highlightsToStart[i] != null)
                highlightsToStart[i].StopHighlight();
        }
    }
}