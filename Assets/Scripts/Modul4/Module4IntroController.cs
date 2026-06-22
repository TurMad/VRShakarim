using System.Collections;
using UnityEngine;

public class Module4IntroController : MonoBehaviour
{
    [Header("Intro Audio Sequence")]
    [SerializeField] private AudioClip[] introVoiceClips = new AudioClip[3];
    [SerializeField] private SubtitleSequenceSO[] introSubtitles = new SubtitleSequenceSO[3];
    [SerializeField] private float delayBeforeFirstAudio = 0.5f;
    [SerializeField] private float delayBetweenAudios = 0.4f;

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

        SetInteractablesEnabled(false);

        if (teleportByButton != null)
            teleportByButton.enabled = false;

        yield return SceneFlowManager.Instance.FadeFromBlack(fadeFromBlackDuration);

        yield return new WaitForSeconds(delayBeforeFirstAudio);

        yield return PlayIntroAudioSequence();

        SetInteractablesEnabled(true);
        StartHighlights();

        SceneFlowManager.Instance.SetXRLocked(false);
        SceneFlowManager.Instance.SetMoveTurnLocked(false);

        if (teleportByButton != null)
            teleportByButton.enabled = true;
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