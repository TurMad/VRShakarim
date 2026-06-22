using System.Collections;
using UnityEngine;

public class Module2IntroController : MonoBehaviour
{
    [Header("Character")]
    [SerializeField] private GameObject characterRoot;
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private string characterStartTriggerName = "start";

    [Header("Intro Audio Sequence")]
    [SerializeField] private AudioClip[] introVoiceClips = new AudioClip[9];
    [SerializeField] private SubtitleSequenceSO[] introSubtitles = new SubtitleSequenceSO[9];
    [SerializeField] private float delayBeforeFirstAudio = 0.5f;
    [SerializeField] private float delayBetweenAudios = 0.4f;

    [Header("Fade")]
    [SerializeField] private float fadeFromBlackDuration = 1f;
    [SerializeField] private float finalFadeToBlackDuration = 0.6f;
    [SerializeField] private float finalFadeFromBlackDuration = 0.6f;

    [Header("After Intro")]
    [SerializeField] private Behaviour[] interactablesToEnable;
    [SerializeField] private InteractableHighlight[] highlightsToStart;

    [Header("Instruction Audio")]
    [SerializeField] private AudioClip movementInstructionAudio;
    [SerializeField] private float delayBeforeInstructionAudio = 0.3f;

    [Header("Teleport Alternative")]
    [SerializeField] private SimpleXRTeleportByButton teleportByButton;

    private bool characterAnimationStarted;

    private IEnumerator Start()
    {
        SceneFlowManager.Instance.SetXRLocked(true);
        SceneFlowManager.Instance.SetMoveTurnLocked(true);

        SetInteractablesEnabled(false);

        if (teleportByButton != null)
            teleportByButton.enabled = false;

        yield return SceneFlowManager.Instance.FadeFromBlack(fadeFromBlackDuration);

        yield return new WaitForSeconds(delayBeforeFirstAudio);

        yield return PlayIntroAudioSequence();

        yield return SceneFlowManager.Instance.FadeToBlack(finalFadeToBlackDuration);

        if (characterRoot != null)
            characterRoot.SetActive(false);

        yield return SceneFlowManager.Instance.FadeFromBlack(finalFadeFromBlackDuration);

        SetInteractablesEnabled(true);
        StartHighlights();

        SceneFlowManager.Instance.SetXRLocked(false);
        SceneFlowManager.Instance.SetMoveTurnLocked(false);

        if (teleportByButton != null)
            teleportByButton.enabled = true;

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

            if (!characterAnimationStarted)
            {
                characterAnimationStarted = true;
                PlayCharacterStartAnimation();
            }

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

    private void PlayCharacterStartAnimation()
    {
        if (characterAnimator == null)
            return;

        if (string.IsNullOrWhiteSpace(characterStartTriggerName))
            return;

        characterAnimator.SetTrigger(characterStartTriggerName);
    }

    private IEnumerator PlayInstructionAudioWithoutBlocking()
    {
        yield return new WaitForSeconds(delayBeforeInstructionAudio);
        SceneFlowManager.Instance.PlayAudio(movementInstructionAudio);
    }

    private void SetInteractablesEnabled(bool value)
    {
        for (int i = 0; i < interactablesToEnable.Length; i++)
        {
            if (interactablesToEnable[i] != null)
                interactablesToEnable[i].enabled = value;
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
}