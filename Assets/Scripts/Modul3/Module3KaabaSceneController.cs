using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Module3KaabaSceneController : MonoBehaviour
{
    [Header("XR")]
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private Transform pathStartViewPoint;

    [Header("Audio 1")]
    [SerializeField] private AudioClip introAudio1;
    [SerializeField] private SubtitleSequenceSO introSubtitles1;
    [SerializeField] private float delayBeforeAudio1 = 0.5f;

    [Header("Audio 2")]
    [SerializeField] private AudioClip introAudio2;
    [SerializeField] private SubtitleSequenceSO introSubtitles2;
    [SerializeField] private float delayBetweenAudio1And2 = 0.6f;

    [Header("Audio 3 After Teleport")]
    [SerializeField] private AudioClip pathIntroAudio;
    [SerializeField] private SubtitleSequenceSO pathIntroSubtitles;
    [SerializeField] private float delayBeforePathIntroAudio = 0.6f;

    [Header("Fade")]
    [SerializeField] private float fadeFromBlackDuration = 1f;
    [SerializeField] private float fadeToPathDuration = 0.7f;
    [SerializeField] private float fadeFromPathDuration = 0.7f;
    [SerializeField] private float finalFadeDuration = 25f;

    [Header("Path Movement")]
    [SerializeField] private KaabaPathMovement playerPathMovement;
    [SerializeField] private CameraSway cameraSway;
    [SerializeField] private InputActionReference[] forwardMoveActions;
    [SerializeField] private float forwardThreshold = 0.5f;

    [Header("After One Lap")]
    [SerializeField] private InstructionSubtitleSO finalInstructionSubtitle;
    [SerializeField] private InputActionReference[] finalStartActions;
    [SerializeField] private AudioClip finalAudioWithoutSubtitles;
    [SerializeField] private string introSceneName = "IntroScene";

    private bool pathInputEnabled;
    private bool finalInstructionShown;
    private bool finalStarted;
    private bool lastMovingState;

    private void Awake()
    {
        if (playerPathMovement != null)
        {
            playerPathMovement.DisableMovement();
            playerPathMovement.ResetPath();
        }

        if (cameraSway != null)
            cameraSway.enabled = false;
    }

    private void OnEnable()
    {
        EnableInputActions(forwardMoveActions, true);
        EnableInputActions(finalStartActions, true);
    }

    private void OnDisable()
    {
        EnableInputActions(forwardMoveActions, false);
        EnableInputActions(finalStartActions, false);
    }

    private IEnumerator Start()
    {
        SceneFlowManager.Instance.SetXRLocked(true);
        SceneFlowManager.Instance.SetMoveTurnLocked(true);

        yield return SceneFlowManager.Instance.FadeFromBlack(fadeFromBlackDuration);

        yield return new WaitForSeconds(delayBeforeAudio1);
        yield return PlayAudioWithSubtitles(introAudio1, introSubtitles1);

        yield return new WaitForSeconds(delayBetweenAudio1And2);
        yield return PlayAudioWithSubtitles(introAudio2, introSubtitles2);

        yield return SceneFlowManager.Instance.FadeToBlack(fadeToPathDuration);

        MoveXROriginToPoint();

        yield return SceneFlowManager.Instance.FadeFromBlack(fadeFromPathDuration);

        yield return new WaitForSeconds(delayBeforePathIntroAudio);
        yield return PlayAudioWithSubtitles(pathIntroAudio, pathIntroSubtitles);

        SceneFlowManager.Instance.SetXRLocked(false);
        SceneFlowManager.Instance.SetMoveTurnLocked(true);

        if (playerPathMovement != null)
            playerPathMovement.EnableMovement();

        pathInputEnabled = true;
    }

    private void Update()
    {
        if (!pathInputEnabled || finalStarted)
            return;

        bool forwardPressed = IsForwardPressed();

        if (playerPathMovement != null)
            playerPathMovement.SetManualMoveInput(forwardPressed);

        UpdateCameraSway();

        if (!finalInstructionShown && playerPathMovement != null && playerPathMovement.CompletedLaps >= 1)
        {
            finalInstructionShown = true;

            if (SubtitleManager.Instance != null && finalInstructionSubtitle != null)
                SubtitleManager.Instance.ShowInstruction(finalInstructionSubtitle);
        }

        if (finalInstructionShown && IsAnyFinalStartPressed())
        {
            StartCoroutine(FinalRoutine());
        }
    }

    private IEnumerator FinalRoutine()
    {
        finalStarted = true;
        pathInputEnabled = false;

        if (SubtitleManager.Instance != null)
            SubtitleManager.Instance.HideInstruction();

        SceneFlowManager.Instance.SetXRLocked(true);
        SceneFlowManager.Instance.SetMoveTurnLocked(true);

        if (playerPathMovement != null)
        {
            playerPathMovement.SetManualMoveInput(false);
            playerPathMovement.SetAutoMove(true);
        }

        if (cameraSway != null)
            cameraSway.enabled = true;

        if (finalAudioWithoutSubtitles != null)
        {
            SceneFlowManager.Instance.PlayAudio(finalAudioWithoutSubtitles);
        }

        yield return SceneFlowManager.Instance.FadeToBlack(finalFadeDuration);

        Module3MusicManager.StopMusic();

        if (!string.IsNullOrWhiteSpace(introSceneName))
            SceneManager.LoadScene(introSceneName);
    }

    private IEnumerator PlayAudioWithSubtitles(AudioClip clip, SubtitleSequenceSO subtitles)
    {
        if (clip == null)
            yield break;

        if (SubtitleManager.Instance != null && subtitles != null)
            SubtitleManager.Instance.PlaySequence(subtitles);

        SceneFlowManager.Instance.PlayAudio(clip);
        yield return SceneFlowManager.Instance.WaitForAudioFinished();

        if (SubtitleManager.Instance != null)
            SubtitleManager.Instance.StopSequence();
    }

    private bool IsForwardPressed()
    {
        for (int i = 0; i < forwardMoveActions.Length; i++)
        {
            if (forwardMoveActions[i] == null)
                continue;

            Vector2 value = forwardMoveActions[i].action.ReadValue<Vector2>();

            if (value.y > forwardThreshold)
                return true;
        }

        return false;
    }

    private bool IsAnyFinalStartPressed()
    {
        for (int i = 0; i < finalStartActions.Length; i++)
        {
            if (finalStartActions[i] == null)
                continue;

            if (finalStartActions[i].action.WasPressedThisFrame())
                return true;
        }

        return false;
    }

    private void UpdateCameraSway()
    {
        if (cameraSway == null || playerPathMovement == null)
            return;

        bool movingNow = playerPathMovement.IsMovingNow;

        if (movingNow == lastMovingState)
            return;

        cameraSway.enabled = movingNow;
        lastMovingState = movingNow;
    }

    private void MoveXROriginToPoint()
    {
        if (xrOrigin == null || pathStartViewPoint == null)
            return;

        Transform cameraTransform = xrOrigin.Camera.transform;

        Vector3 cameraOffset = xrOrigin.transform.position - cameraTransform.position;
        cameraOffset.y = 0f;

        xrOrigin.transform.position = pathStartViewPoint.position + cameraOffset;

        Vector3 forward = pathStartViewPoint.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude > 0.001f)
            xrOrigin.transform.rotation = Quaternion.LookRotation(forward);
    }

    private void EnableInputActions(InputActionReference[] actions, bool value)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            if (actions[i] == null)
                continue;

            if (value)
                actions[i].action.Enable();
            else
                actions[i].action.Disable();
        }
    }
}