using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SubtitleManager : MonoBehaviour
{
    public static SubtitleManager Instance { get; private set; }

    [Header("Regular Subtitle UI")]
    [SerializeField] private CanvasGroup regularCanvasGroup;
    [SerializeField] private GameObject regularTextRoot;
    [SerializeField] private TMP_Text regularText;

    [Header("Instruction Subtitle UI")]
    [SerializeField] private CanvasGroup instructionCanvasGroup;
    [SerializeField] private GameObject instructionTextRoot;
    [SerializeField] private TMP_Text instructionText;

    [Header("Toggle Regular Subtitles By Controller")]
    [SerializeField] private InputActionReference[] toggleSubtitleActions;

    [Header("Common")]
    [SerializeField] private float fadeDuration = 0.25f;

    private Coroutine regularRoutine;
    private Coroutine instructionRoutine;
    private Coroutine toggleFadeRoutine;

    private bool regularVisible = true;
    private bool regularActive;
    private bool instructionActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitCanvasGroup(regularCanvasGroup);
        InitCanvasGroup(instructionCanvasGroup);

        ClearRegularText();
        ClearInstructionText();

        ApplyRegularVisibility();
        ApplyInstructionVisibility();
    }

    private void OnEnable()
    {
        SetInputActionsEnabled(true);
    }

    private void OnDisable()
    {
        SetInputActionsEnabled(false);
    }

    private void Update()
    {
        if (!regularActive)
            return;

        for (int i = 0; i < toggleSubtitleActions.Length; i++)
        {
            if (toggleSubtitleActions[i] == null)
                continue;

            if (toggleSubtitleActions[i].action.WasPressedThisFrame())
            {
                ToggleRegularSubtitles();
                return;
            }
        }
    }

    public void PlaySequence(SubtitleSequenceSO sequence)
    {
        if (sequence == null || sequence.lines == null || sequence.lines.Count == 0)
            return;

        StopRegularRoutine();
        regularRoutine = StartCoroutine(PlayRegularSequenceRoutine(sequence));
    }

    public void StopSequence()
    {
        StopRegularRoutine();
        regularRoutine = StartCoroutine(HideRegularRoutine());
    }

    public void ShowInstruction(InstructionSubtitleSO instruction)
    {
        if (instruction == null)
            return;

        StopInstructionRoutine();
        instructionRoutine = StartCoroutine(ShowInstructionRoutine(instruction));
    }

    public void HideInstruction()
    {
        StopInstructionRoutine();
        instructionRoutine = StartCoroutine(HideInstructionRoutine());
    }

    private IEnumerator PlayRegularSequenceRoutine(SubtitleSequenceSO sequence)
    {
        regularActive = true;
        ApplyRegularVisibility();

        if (regularVisible)
            yield return FadeCanvasGroup(regularCanvasGroup, regularCanvasGroup.alpha, 1f, fadeDuration);

        for (int i = 0; i < sequence.lines.Count; i++)
        {
            SubtitleLineData line = sequence.lines[i];

            if (regularText != null)
                regularText.text = GetLocalizedText(line);

            yield return new WaitForSeconds(line.duration);
        }

        yield return HideRegularRoutine();
    }

    private IEnumerator HideRegularRoutine()
    {
        yield return FadeCanvasGroup(regularCanvasGroup, regularCanvasGroup.alpha, 0f, fadeDuration);

        regularActive = false;
        ClearRegularText();
        ApplyRegularVisibility();

        regularRoutine = null;
    }

    private IEnumerator ShowInstructionRoutine(InstructionSubtitleSO instruction)
    {
        instructionActive = true;

        if (instructionText != null)
            instructionText.text = GetLocalizedText(instruction);

        ApplyInstructionVisibility();

        yield return FadeCanvasGroup(instructionCanvasGroup, instructionCanvasGroup.alpha, 1f, fadeDuration);

        instructionRoutine = null;
    }

    private IEnumerator HideInstructionRoutine()
    {
        yield return FadeCanvasGroup(instructionCanvasGroup, instructionCanvasGroup.alpha, 0f, fadeDuration);

        instructionActive = false;
        ClearInstructionText();
        ApplyInstructionVisibility();

        instructionRoutine = null;
    }

    private void ToggleRegularSubtitles()
    {
        regularVisible = !regularVisible;

        if (toggleFadeRoutine != null)
            StopCoroutine(toggleFadeRoutine);

        toggleFadeRoutine = StartCoroutine(ToggleRegularFadeRoutine());
    }

    private IEnumerator ToggleRegularFadeRoutine()
    {
        if (regularCanvasGroup == null)
            yield break;

        if (regularVisible)
        {
            ApplyRegularVisibility();
            yield return FadeCanvasGroup(regularCanvasGroup, regularCanvasGroup.alpha, 1f, fadeDuration);
        }
        else
        {
            yield return FadeCanvasGroup(regularCanvasGroup, regularCanvasGroup.alpha, 0f, fadeDuration);
            ApplyRegularVisibility();
        }

        toggleFadeRoutine = null;
    }

    private void ApplyRegularVisibility()
    {
        if (regularTextRoot != null)
            regularTextRoot.SetActive(regularActive && regularVisible);
    }

    private void ApplyInstructionVisibility()
    {
        if (instructionTextRoot != null)
            instructionTextRoot.SetActive(instructionActive);
    }

    private void StopRegularRoutine()
    {
        if (regularRoutine != null)
        {
            StopCoroutine(regularRoutine);
            regularRoutine = null;
        }
    }

    private void StopInstructionRoutine()
    {
        if (instructionRoutine != null)
        {
            StopCoroutine(instructionRoutine);
            instructionRoutine = null;
        }
    }

    private string GetLocalizedText(SubtitleLineData line)
    {
        switch (SubtitleLanguageManager.CurrentLanguage)
        {
            case SubtitleLanguage.Kazakh:
                return line.kazakhText;

            case SubtitleLanguage.English:
                return line.englishText;

            default:
                return line.russianText;
        }
    }

    private string GetLocalizedText(InstructionSubtitleSO instruction)
    {
        switch (SubtitleLanguageManager.CurrentLanguage)
        {
            case SubtitleLanguage.Kazakh:
                return instruction.kazakhText;

            case SubtitleLanguage.English:
                return instruction.englishText;

            default:
                return instruction.russianText;
        }
    }

    private void ClearRegularText()
    {
        if (regularText != null)
            regularText.text = string.Empty;
    }

    private void ClearInstructionText()
    {
        if (instructionText != null)
            instructionText.text = string.Empty;
    }

    private void InitCanvasGroup(CanvasGroup group)
    {
        if (group == null)
            return;

        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    private void SetInputActionsEnabled(bool value)
    {
        for (int i = 0; i < toggleSubtitleActions.Length; i++)
        {
            if (toggleSubtitleActions[i] == null)
                continue;

            if (value)
                toggleSubtitleActions[i].action.Enable();
            else
                toggleSubtitleActions[i].action.Disable();
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null)
            yield break;

        group.interactable = false;
        group.blocksRaycasts = false;

        float time = 0f;
        group.alpha = from;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = duration <= 0f ? 1f : Mathf.Clamp01(time / duration);
            group.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        group.alpha = to;
    }
}