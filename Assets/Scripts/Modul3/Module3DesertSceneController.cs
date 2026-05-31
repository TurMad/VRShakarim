using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Module3DesertSceneController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip introAudio;
    [SerializeField] private SubtitleSequenceSO introSubtitles;
    [SerializeField] private float delayBeforeIntroAudio = 0.4f;
    [SerializeField] private float delayBeforeExit = 0.6f;

    [Header("Fade")]
    [SerializeField] private float fadeFromBlackDuration = 1f;
    [SerializeField] private float fadeToBlackDuration = 0.8f;

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName;

    [Header("Movement")]
    [SerializeField] private PathFollower playerPathFollower;
    [SerializeField] private LocalLineMover[] caravanMovers;
    [SerializeField] private CameraSway cameraSway;

    [Header("FX")]
    [SerializeField] private ParticleSystem[] desertFxToPlayOnStart;

    private IEnumerator Start()
    {
        SceneFlowManager.Instance.SetXRLocked(true);
        SceneFlowManager.Instance.SetMoveTurnLocked(true);

        if (playerPathFollower != null)
            playerPathFollower.SetAutoStart(false);

        if (cameraSway != null)
            cameraSway.enabled = false;

        for (int i = 0; i < caravanMovers.Length; i++)
        {
            if (caravanMovers[i] != null)
                caravanMovers[i].SetAutoStart(false);
        }

        yield return SceneFlowManager.Instance.FadeFromBlack(fadeFromBlackDuration);

        for (int i = 0; i < desertFxToPlayOnStart.Length; i++)
        {
            if (desertFxToPlayOnStart[i] != null)
                desertFxToPlayOnStart[i].Play();
        }

        for (int i = 0; i < caravanMovers.Length; i++)
        {
            if (caravanMovers[i] != null)
                caravanMovers[i].PlayMove();
        }

        if (playerPathFollower != null)
            playerPathFollower.PlayMove();

        if (cameraSway != null)
            cameraSway.enabled = true;

        yield return new WaitForSeconds(delayBeforeIntroAudio);

        if (SubtitleManager.Instance != null && introSubtitles != null)
            SubtitleManager.Instance.PlaySequence(introSubtitles);

        SceneFlowManager.Instance.PlayAudio(introAudio);
        yield return SceneFlowManager.Instance.WaitForAudioFinished();

        if (SubtitleManager.Instance != null)
            SubtitleManager.Instance.StopSequence();

        yield return new WaitForSeconds(delayBeforeExit);

        yield return SceneFlowManager.Instance.FadeToBlack(fadeToBlackDuration);

        if (!string.IsNullOrWhiteSpace(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }
}