using System.Collections;
using UnityEngine;

public class Module1IntroController : MonoBehaviour
{
    [Header("One Intro Audio With Subtitles")]
    [SerializeField] private AudioClip introVoiceClip;
    [SerializeField] private SubtitleSequenceSO introSubtitle;
    [SerializeField] private float delayBeforeIntroAudio = 0.5f;

    [Header("Fade")]
    [SerializeField] private float fadeFromBlackAtStartDuration = 1f;
    [SerializeField] private float fadeToBlackAfterIntroDuration = 0.5f;
    [SerializeField] private float fadeFromBlackAfterSwapDuration = 0.5f;

    [Header("Objects To Swap While Screen Is Black")]
    [Tooltip("Персонажи и старые предметы. Они видны во время вступительного аудио и выключаются после него.")]
    [SerializeField] private GameObject[] objectsToDisableAfterIntro;

    [Tooltip("Новые предметы для исследования. Они выключены в начале и включаются после вступительного аудио.")]
    [SerializeField] private GameObject[] objectsToEnableAfterIntro;

    [Header("Interaction")]
    [Tooltip("XRGrabInteractable, Module1GrabItem и другие компоненты, которые должны включиться только после инструкции.")]
    [SerializeField] private Behaviour[] interactablesToEnable;

    [SerializeField] private InteractableHighlight[] highlightsToStart;
    [SerializeField] private float delayBeforeHighlights = 0.35f;

    [Header("Instruction Audio")]
    [SerializeField] private AudioClip movementInstructionAudio;
    [SerializeField] private float delayBeforeInstructionAudio = 0.3f;

    [Header("Teleport Alternative")]
    [SerializeField] private SimpleXRTeleportByButton teleportByButton;

    private IEnumerator Start()
    {
        SceneFlowManager.Instance.SetXRLocked(true);
        SceneFlowManager.Instance.SetMoveTurnLocked(true);

        if (teleportByButton != null)
            teleportByButton.enabled = false;

        DisableInteractionComponents();
        SetObjectsActive(objectsToEnableAfterIntro, false);

        yield return SceneFlowManager.Instance.FadeFromBlack(fadeFromBlackAtStartDuration);

        yield return new WaitForSeconds(delayBeforeIntroAudio);

        yield return PlayIntroAudio();

        // После интро скрываем персонажей и старые предметы.
        yield return SceneFlowManager.Instance.FadeToBlack(fadeToBlackAfterIntroDuration);

        SetObjectsActive(objectsToDisableAfterIntro, false);
        SetObjectsActive(objectsToEnableAfterIntro, true);

        // Даём Unity один кадр, чтобы активированные объекты успели включиться.
        yield return null;

        yield return SceneFlowManager.Instance.FadeFromBlack(fadeFromBlackAfterSwapDuration);

        // Игрок уже может ходить, крутиться и телепортироваться.
        SceneFlowManager.Instance.SetXRLocked(false);
        SceneFlowManager.Instance.SetMoveTurnLocked(false);

        if (teleportByButton != null)
            teleportByButton.enabled = true;

        // Инструкция проигрывается, но движение не блокируется.
        yield return PlayInstructionAudio();

        // Небольшая пауза нужна, чтобы недавно включённые объекты
        // корректно активировались до запуска хайлайтов.
        yield return new WaitForSeconds(delayBeforeHighlights);

        EnableInteractionComponents();
        StartHighlights();
    }

    private IEnumerator PlayIntroAudio()
    {
        if (introVoiceClip == null)
            yield break;

        if (SubtitleManager.Instance != null && introSubtitle != null)
            SubtitleManager.Instance.PlaySequence(introSubtitle);

        SceneFlowManager.Instance.PlayAudio(introVoiceClip);
        yield return SceneFlowManager.Instance.WaitForAudioFinished();

        if (SubtitleManager.Instance != null)
            SubtitleManager.Instance.StopSequence();
    }

    private IEnumerator PlayInstructionAudio()
    {
        if (movementInstructionAudio == null)
            yield break;

        yield return new WaitForSeconds(delayBeforeInstructionAudio);

        SceneFlowManager.Instance.PlayAudio(movementInstructionAudio);
        yield return SceneFlowManager.Instance.WaitForAudioFinished();
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

    private void SetObjectsActive(GameObject[] objects, bool value)
    {
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
                objects[i].SetActive(value);
        }
    }
}