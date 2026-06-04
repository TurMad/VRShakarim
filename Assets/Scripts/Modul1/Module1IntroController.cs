using System.Collections;
using UnityEngine;

public class Module1IntroController : MonoBehaviour
{
    [Header("Intro Audio Sequence")]
    [SerializeField] private AudioClip[] introVoiceClips;
    [SerializeField] private SubtitleSequenceSO[] introSubtitles;
    [SerializeField] private float delayBeforeFirstAudio = 0.5f;
    [SerializeField] private float delayBetweenAudios = 0.4f;

    [Header("Instruction Audio")]
    [SerializeField] private AudioClip movementInstructionAudio;
    [SerializeField] private float delayBeforeInstructionAudio = 0.3f;

    [Header("Fade")]
    [SerializeField] private float fadeFromBlackDuration = 1f;

    [Header("After Intro")]
    [SerializeField] private Behaviour[] interactablesToEnable;
    [SerializeField] private InteractableHighlight[] highlightsToStart;

    [Header("Teleport Alternative")]
    [SerializeField] private SimpleXRTeleportByButton teleportByButton;

    private IEnumerator Start()
    {
        SceneFlowManager.Instance.SetXRLocked(true);
        SceneFlowManager.Instance.SetMoveTurnLocked(true);

        if (teleportByButton != null)
            teleportByButton.enabled = false;

        DisableAfterIntroObjects();

        yield return SceneFlowManager.Instance.FadeFromBlack(fadeFromBlackDuration);

        yield return new WaitForSeconds(delayBeforeFirstAudio);

        yield return PlayIntroAudioSequence();

        EnableAfterIntroObjects();

        // Возвращаем контроллеры
        SceneFlowManager.Instance.SetXRLocked(false);

        // Возвращаем обычный Move/Turn
        SceneFlowManager.Instance.SetMoveTurnLocked(false);

        // Включаем телепорт как альтернативу
        if (teleportByButton != null)
            teleportByButton.enabled = true;

        // Инструкция играет после разблокировки и ничего не блокирует
        if (movementInstructionAudio != null)
            StartCoroutine(PlayInstructionAudioWithoutBlocking());
    }

    private IEnumerator PlayIntroAudioSequence()
    {
        for (int i = 0; i < introVoiceClips.Length; i++)
        {
            AudioClip clip = introVoiceClips[i];

            if (clip == null)
                continue;

            SubtitleSequenceSO subtitle = null;

            if (introSubtitles != null && i < introSubtitles.Length)
                subtitle = introSubtitles[i];

            if (SubtitleManager.Instance != null && subtitle != null)
                SubtitleManager.Instance.PlaySequence(subtitle);

            SceneFlowManager.Instance.PlayAudio(clip);
            yield return SceneFlowManager.Instance.WaitForAudioFinished();

            if (SubtitleManager.Instance != null)
                SubtitleManager.Instance.StopSequence();

            if (i < introVoiceClips.Length - 1)
                yield return new WaitForSeconds(delayBetweenAudios);
        }
    }

    private IEnumerator PlayInstructionAudioWithoutBlocking()
    {
        yield return new WaitForSeconds(delayBeforeInstructionAudio);

        SceneFlowManager.Instance.PlayAudio(movementInstructionAudio);
    }

    private void DisableAfterIntroObjects()
    {
        for (int i = 0; i < interactablesToEnable.Length; i++)
        {
            if (interactablesToEnable[i] != null)
                interactablesToEnable[i].enabled = false;
        }
    }

    private void EnableAfterIntroObjects()
    {
        for (int i = 0; i < interactablesToEnable.Length; i++)
        {
            if (interactablesToEnable[i] != null)
                interactablesToEnable[i].enabled = true;
        }

        for (int i = 0; i < highlightsToStart.Length; i++)
        {
            if (highlightsToStart[i] != null)
                highlightsToStart[i].StartHighlight();
        }
    }
}