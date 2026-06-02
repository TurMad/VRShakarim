using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Module3KaabaSceneController : MonoBehaviour
{
    [Header("Audio 1")]
    [SerializeField] private AudioClip introAudio1;
    [SerializeField] private SubtitleSequenceSO introSubtitles1;
    [SerializeField] private float delayBeforeAudio1 = 0.4f;

    [Header("Audio 2")]
    [SerializeField] private AudioClip introAudio2;
    [SerializeField] private SubtitleSequenceSO introSubtitles2;
    [SerializeField] private float delayBetweenAudio1And2 = 0.5f;

    [Header("Audio 3")]
    [SerializeField] private AudioClip introAudio3;
    [SerializeField] private SubtitleSequenceSO introSubtitles3;
    [SerializeField] private float delayBetweenAudio2And3 = 0.5f;

    [Header("After Intro")]
    [SerializeField] private float delayBeforeEnableControllers = 0.5f;
    [SerializeField] private InstructionSubtitleSO moveInstructionSubtitle;

    [Header("Fade")]
    [SerializeField] private float fadeFromBlackDuration = 1f;
    [SerializeField] private float fadeToBlackDuration = 0.8f;

    [Header("Forward Input")]
    [SerializeField] private InputActionReference[] forwardMoveActions;
    [SerializeField] private float forwardThreshold = 0.5f;

    [Header("Pilgrims")]
    [SerializeField] private Behaviour[] pilgrimMovementScriptsToEnable;
    [SerializeField] private Animator[] pilgrimAnimators;
    [SerializeField] private string walkTriggerName = "Walk";

    [Header("Player Path")]
    [SerializeField] private PathMovement playerPathFollower;

    [Header("Camera Sway")]
    [SerializeField] private CameraSway cameraSway;

    [Header("Finish Logic")]
    [SerializeField] private int requiredFullLaps = 7;
    [SerializeField] private int transitionPointIndexOnNextLap = 2;
    [SerializeField] private float delayBeforeFinishFade = 0.4f;
    [SerializeField] private string introSceneName;

    private bool waitingForForwardInput;
    private bool movementStarted;
    private bool finishStarted;

    private void Awake()
    {
        SetBehavioursEnabled(pilgrimMovementScriptsToEnable, false);

        if (playerPathFollower != null)
        {
            playerPathFollower.SetAutoStart(false);
            playerPathFollower.ResetPathState();
        }

        if (cameraSway != null)
            cameraSway.enabled = false;
    }

    private void OnEnable()
    {
        for (int i = 0; i < forwardMoveActions.Length; i++)
        {
            if (forwardMoveActions[i] != null)
                forwardMoveActions[i].action.Enable();
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < forwardMoveActions.Length; i++)
        {
            if (forwardMoveActions[i] != null)
                forwardMoveActions[i].action.Disable();
        }
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

        yield return new WaitForSeconds(delayBetweenAudio2And3);
        yield return PlayAudioWithSubtitles(introAudio3, introSubtitles3);

        yield return new WaitForSeconds(delayBeforeEnableControllers);

        // Возвращаем только визуал/контроллеры
        SceneFlowManager.Instance.SetXRLocked(false);

        // Move/Turn остаются заблокированы
        SceneFlowManager.Instance.SetMoveTurnLocked(true);

        if (SubtitleManager.Instance != null && moveInstructionSubtitle != null)
            SubtitleManager.Instance.ShowInstruction(moveInstructionSubtitle);

        waitingForForwardInput = true;
    }

    private void Update()
    {
        if (waitingForForwardInput && !movementStarted)
        {
            for (int i = 0; i < forwardMoveActions.Length; i++)
            {
                if (forwardMoveActions[i] == null)
                    continue;

                Vector2 value = forwardMoveActions[i].action.ReadValue<Vector2>();
                if (value.y > forwardThreshold)
                {
                    StartMovementSequence();
                    break;
                }
            }
        }

        if (movementStarted && !finishStarted && playerPathFollower != null)
        {
            // Уже сделали 7 полных кругов и пошли на следующий круг, дошли до нужной точки
            if (playerPathFollower.CompletedLaps >= requiredFullLaps &&
                playerPathFollower.CurrentPointIndex >= transitionPointIndexOnNextLap)
            {
                StartCoroutine(FinishModuleRoutine());
            }
        }
    }

    private void StartMovementSequence()
    {
        movementStarted = true;
        waitingForForwardInput = false;

        if (SubtitleManager.Instance != null)
            SubtitleManager.Instance.HideInstruction();

        SetBehavioursEnabled(pilgrimMovementScriptsToEnable, true);

        for (int i = 0; i < pilgrimAnimators.Length; i++)
        {
            if (pilgrimAnimators[i] != null && !string.IsNullOrEmpty(walkTriggerName))
                pilgrimAnimators[i].SetTrigger(walkTriggerName);
        }

        if (playerPathFollower != null)
            playerPathFollower.PlayMove();

        if (cameraSway != null)
            cameraSway.enabled = true;
    }

    private IEnumerator FinishModuleRoutine()
    {
        finishStarted = true;

        yield return new WaitForSeconds(delayBeforeFinishFade);

        SceneFlowManager.Instance.SetXRLocked(true);
        SceneFlowManager.Instance.SetMoveTurnLocked(true);

        yield return SceneFlowManager.Instance.FadeToBlack(fadeToBlackDuration);

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

    private void SetBehavioursEnabled(Behaviour[] behaviours, bool value)
    {
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] != null)
                behaviours[i].enabled = value;
        }
    }
}