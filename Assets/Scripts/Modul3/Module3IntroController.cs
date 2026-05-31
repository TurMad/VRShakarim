using System.Collections;
using UnityEngine;

public class Module3IntroController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip introAudio;
    [SerializeField] private float delayBeforeIntroAudio = 0.4f;

    [Header("Subtitles")]
    [SerializeField] private SubtitleSequenceSO introSubtitles;

    [Header("Fade")]
    [SerializeField] private float fadeFromBlackDuration = 1f;

    [Header("Scroll Activation")]
    [SerializeField] private Behaviour[] scrollInteractablesToEnable;
    [SerializeField] private InteractableHighlight[] scrollHighlightsToStart;

    private void Awake()
    {
        for (int i = 0; i < scrollInteractablesToEnable.Length; i++)
        {
            if (scrollInteractablesToEnable[i] != null)
                scrollInteractablesToEnable[i].enabled = false;
        }
    }

    private IEnumerator Start()
    {
        SceneFlowManager.Instance.SetXRLocked(true);
        SceneFlowManager.Instance.SetMoveTurnLocked(true);

        yield return SceneFlowManager.Instance.FadeFromBlack(fadeFromBlackDuration);
        yield return new WaitForSeconds(delayBeforeIntroAudio);

        if (SubtitleManager.Instance != null && introSubtitles != null)
            SubtitleManager.Instance.PlaySequence(introSubtitles);

        SceneFlowManager.Instance.PlayAudio(introAudio);
        yield return SceneFlowManager.Instance.WaitForAudioFinished();

        if (SubtitleManager.Instance != null)
            SubtitleManager.Instance.StopSequence();

        for (int i = 0; i < scrollInteractablesToEnable.Length; i++)
        {
            if (scrollInteractablesToEnable[i] != null)
                scrollInteractablesToEnable[i].enabled = true;
        }

        for (int i = 0; i < scrollHighlightsToStart.Length; i++)
        {
            if (scrollHighlightsToStart[i] != null)
                scrollHighlightsToStart[i].StartHighlight();
        }

        // Контроллеры возвращаем, но Move/Turn остаются заблокированы
        SceneFlowManager.Instance.SetXRLocked(false);
        SceneFlowManager.Instance.SetMoveTurnLocked(true);
    }
}