using System.Collections;
using TMPro;
using UnityEngine;

public class SubtitleManager : MonoBehaviour
{
    public static SubtitleManager Instance { get; private set; }

    [Header("Regular Subtitles")]
    [SerializeField] private CanvasGroup subtitleCanvasGroup;
    [SerializeField] private TMP_Text subtitleText;

    [Header("Instruction Subtitles")]
    [SerializeField] private CanvasGroup instructionCanvasGroup;
    [SerializeField] private TMP_Text instructionText;

    [Header("Common")]
    [SerializeField] private float fadeDuration = 0.25f;

    private Coroutine playRoutine;
    private Coroutine instructionRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (subtitleCanvasGroup != null)
        {
            subtitleCanvasGroup.alpha = 0f;
            subtitleCanvasGroup.interactable = false;
            subtitleCanvasGroup.blocksRaycasts = false;
        }

        if (instructionCanvasGroup != null)
        {
            instructionCanvasGroup.alpha = 0f;
            instructionCanvasGroup.interactable = false;
            instructionCanvasGroup.blocksRaycasts = false;
        }

        ClearRegularText();
        ClearInstructionText();
    }

    public void PlaySequence(SubtitleSequenceSO sequence)
    {
        if (sequence == null || sequence.lines == null || sequence.lines.Count == 0)
            return;

        if (playRoutine != null)
            StopCoroutine(playRoutine);

        playRoutine = StartCoroutine(PlaySequenceRoutine(sequence));
    }

    public void StopSequence()
    {
        if (playRoutine != null)
            StopCoroutine(playRoutine);

        playRoutine = StartCoroutine(HideRegularRoutine());
    }

    public void ShowInstruction(InstructionSubtitleSO instruction)
    {
        if (instruction == null || instructionCanvasGroup == null)
            return;

        if (instructionRoutine != null)
            StopCoroutine(instructionRoutine);

        instructionRoutine = StartCoroutine(ShowInstructionRoutine(instruction));
    }

    public void HideInstruction()
    {
        if (instructionRoutine != null)
            StopCoroutine(instructionRoutine);

        instructionRoutine = StartCoroutine(HideInstructionRoutine());
    }

    private IEnumerator PlaySequenceRoutine(SubtitleSequenceSO sequence)
    {
        yield return FadeCanvasGroup(subtitleCanvasGroup, subtitleCanvasGroup.alpha, 1f, fadeDuration);

        for (int i = 0; i < sequence.lines.Count; i++)
        {
            SubtitleLineData line = sequence.lines[i];

            if (subtitleText != null)
                subtitleText.text = GetLocalizedText(line);

            yield return new WaitForSeconds(line.duration);
        }

        yield return HideRegularRoutine();
    }

    private IEnumerator HideRegularRoutine()
    {
        yield return FadeCanvasGroup(subtitleCanvasGroup, subtitleCanvasGroup.alpha, 0f, fadeDuration);
        ClearRegularText();
        playRoutine = null;
    }

    private IEnumerator ShowInstructionRoutine(InstructionSubtitleSO instruction)
    {
        if (instructionText != null)
            instructionText.text = GetLocalizedText(instruction);

        yield return FadeCanvasGroup(instructionCanvasGroup, instructionCanvasGroup.alpha, 1f, fadeDuration);
        instructionRoutine = null;
    }

    private IEnumerator HideInstructionRoutine()
    {
        yield return FadeCanvasGroup(instructionCanvasGroup, instructionCanvasGroup.alpha, 0f, fadeDuration);
        ClearInstructionText();
        instructionRoutine = null;
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
        if (subtitleText != null)
            subtitleText.text = string.Empty;
    }

    private void ClearInstructionText()
    {
        if (instructionText != null)
            instructionText.text = string.Empty;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null)
            yield break;

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