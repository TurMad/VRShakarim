using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;

public class Module2IntroController : MonoBehaviour
{
    [Header("XR")]
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private Transform gameplayViewPoint;

    [Header("Intro Audio Sequence")]
    [SerializeField] private AudioClip[] introVoiceClips = new AudioClip[9];
    [SerializeField] private SubtitleSequenceSO[] introSubtitles = new SubtitleSequenceSO[9];
    [SerializeField] private float delayBeforeFirstAudio = 0.5f;
    [SerializeField] private float delayBetweenAudios = 0.4f;

    [Header("Fade")]
    [SerializeField] private float fadeFromBlackDuration = 1f;
    [SerializeField] private int fadeAfterAudioIndex = 6;
    [SerializeField] private float middleFadeToBlackDuration = 0.25f;
    [SerializeField] private float middleFadeFromBlackDuration = 0.25f;
    [SerializeField] private float finalFadeToBlackDuration = 0.35f;
    [SerializeField] private float finalFadeFromBlackDuration = 0.35f;

    [Header("After Intro")]
    [SerializeField] private Behaviour[] interactablesToEnable;
    [SerializeField] private InteractableHighlight[] highlightsToStart;

    [Header("Instruction Audio")]
    [SerializeField] private AudioClip movementInstructionAudio;
    [SerializeField] private float delayBeforeInstructionAudio = 0.3f;

    private IEnumerator Start()
    {
        SceneFlowManager.Instance.SetXRLocked(true);
        SceneFlowManager.Instance.SetMoveTurnLocked(true);

        SetInteractablesEnabled(false);

        yield return SceneFlowManager.Instance.FadeFromBlack(fadeFromBlackDuration);
        yield return new WaitForSeconds(delayBeforeFirstAudio);

        yield return PlayIntroAudioSequence();

        yield return SceneFlowManager.Instance.FadeToBlack(finalFadeToBlackDuration);

        MoveXROriginToPoint();

        yield return SceneFlowManager.Instance.FadeFromBlack(finalFadeFromBlackDuration);

        SetInteractablesEnabled(true);
        StartHighlights();

        SceneFlowManager.Instance.SetXRLocked(false);
        SceneFlowManager.Instance.SetMoveTurnLocked(false);

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

            if (i == fadeAfterAudioIndex)
            {
                yield return SceneFlowManager.Instance.FadeToBlack(middleFadeToBlackDuration);
                yield return SceneFlowManager.Instance.FadeFromBlack(middleFadeFromBlackDuration);
            }

            if (i < introVoiceClips.Length - 1)
                yield return new WaitForSeconds(delayBetweenAudios);
        }
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

    private void MoveXROriginToPoint()
    {
        if (xrOrigin == null || gameplayViewPoint == null)
            return;

        Transform cameraTransform = xrOrigin.Camera.transform;

        Vector3 cameraOffset = xrOrigin.transform.position - cameraTransform.position;
        cameraOffset.y = 0f;

        xrOrigin.transform.position = gameplayViewPoint.position + cameraOffset;

        Vector3 forward = gameplayViewPoint.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude > 0.001f)
            xrOrigin.transform.rotation = Quaternion.LookRotation(forward);
    }
}