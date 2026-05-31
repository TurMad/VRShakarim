using System.Collections;
using UnityEngine;

public class Module1IntroController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip introVoiceClip;
    [SerializeField] private float audioStartDelay = 0.5f;

    [Header("Subtitles")]
    [SerializeField] private SubtitleSequenceSO introSubtitles;

    [Header("Fade")]
    [SerializeField] private float fadeFromBlackDuration = 1f;

    [Header("After Intro Audio")]
    [SerializeField] private Behaviour[] interactablesToEnable;
    [SerializeField] private InteractableHighlight[] highlightsToStart;

    private IEnumerator Start()
    {
        SceneFlowManager.Instance.SetXRLocked(true);
        SceneFlowManager.Instance.SetMoveTurnLocked(true);

        yield return SceneFlowManager.Instance.FadeFromBlack(fadeFromBlackDuration);
        yield return new WaitForSeconds(audioStartDelay);

        if (SubtitleManager.Instance != null && introSubtitles != null)
            SubtitleManager.Instance.PlaySequence(introSubtitles);

        SceneFlowManager.Instance.PlayAudio(introVoiceClip);
        yield return SceneFlowManager.Instance.WaitForAudioFinished();

        if (SubtitleManager.Instance != null)
            SubtitleManager.Instance.StopSequence();

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

        SceneFlowManager.Instance.SetXRLocked(false);
        SceneFlowManager.Instance.SetMoveTurnLocked(false);
    }
}