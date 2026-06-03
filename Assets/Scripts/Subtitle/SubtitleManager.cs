using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SubtitleManager : MonoBehaviour
{
    public static SubtitleManager Instance { get; private set; }

    [Header("Subtitle UI")]
    [SerializeField] private CanvasGroup subtitleCanvasGroup;
    [SerializeField] private GameObject subtitleTextRoot;
    [SerializeField] private TMP_Text subtitleText;

    [Header("Hide / Show Button")]
    [SerializeField] private Button toggleTextButton;
    [SerializeField] private Image toggleButtonImage;
    [SerializeField] private Color activeButtonColor = Color.white;
    [SerializeField] private Color inactiveButtonColor = new Color(0.55f, 0.55f, 0.55f, 1f);

    [Header("Common")]
    [SerializeField] private float fadeDuration = 0.25f;

    private Coroutine currentRoutine;
    private bool textVisible = true;
    private bool subtitleActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (toggleTextButton != null)
            toggleTextButton.onClick.AddListener(ToggleTextVisibility);

        if (subtitleCanvasGroup != null)
        {
            subtitleCanvasGroup.alpha = 0f;
            subtitleCanvasGroup.interactable = false;
            subtitleCanvasGroup.blocksRaycasts = false;
        }

        ClearText();
        ApplyTextVisibility();
        UpdateToggleButtonColor();
    }

    private void OnDestroy()
    {
        if (toggleTextButton != null)
            toggleTextButton.onClick.RemoveListener(ToggleTextVisibility);
    }

    public void PlaySequence(SubtitleSequenceSO sequence)
    {
        if (sequence == null || sequence.lines == null || sequence.lines.Count == 0)
            return;

        StopCurrentRoutine();
        currentRoutine = StartCoroutine(PlaySequenceRoutine(sequence));
    }

    public void StopSequence()
    {
        StopCurrentRoutine();
        currentRoutine = StartCoroutine(HideRoutine());
    }

    public void ShowInstruction(InstructionSubtitleSO instruction)
    {
        if (instruction == null)
            return;

        StopCurrentRoutine();
        currentRoutine = StartCoroutine(ShowInstructionRoutine(instruction));
    }

    public void HideInstruction()
    {
        StopCurrentRoutine();
        currentRoutine = StartCoroutine(HideRoutine());
    }

    private IEnumerator PlaySequenceRoutine(SubtitleSequenceSO sequence)
    {
        subtitleActive = true;
        ApplyTextVisibility();

        yield return FadeCanvasGroup(subtitleCanvasGroup, subtitleCanvasGroup.alpha, 1f, fadeDuration);

        for (int i = 0; i < sequence.lines.Count; i++)
        {
            SubtitleLineData line = sequence.lines[i];

            if (subtitleText != null)
                subtitleText.text = GetLocalizedText(line);

            yield return new WaitForSeconds(line.duration);
        }

        yield return HideRoutine();
    }

    private IEnumerator ShowInstructionRoutine(InstructionSubtitleSO instruction)
    {
        subtitleActive = true;

        if (subtitleText != null)
            subtitleText.text = GetLocalizedText(instruction);

        ApplyTextVisibility();

        yield return FadeCanvasGroup(subtitleCanvasGroup, subtitleCanvasGroup.alpha, 1f, fadeDuration);

        currentRoutine = null;
    }

    private IEnumerator HideRoutine()
    {
        yield return FadeCanvasGroup(subtitleCanvasGroup, subtitleCanvasGroup.alpha, 0f, fadeDuration);

        subtitleActive = false;
        ClearText();
        ApplyTextVisibility();

        currentRoutine = null;
    }

    private void ToggleTextVisibility()
    {
        textVisible = !textVisible;
        ApplyTextVisibility();
        UpdateToggleButtonColor();
    }

    private void ApplyTextVisibility()
    {
        if (subtitleTextRoot != null)
            subtitleTextRoot.SetActive(textVisible && subtitleActive);
    }

    private void UpdateToggleButtonColor()
    {
        if (toggleButtonImage == null)
            return;

        toggleButtonImage.color = textVisible ? activeButtonColor : inactiveButtonColor;
    }

    private void StopCurrentRoutine()
    {
        if (currentRoutine == null)
            return;

        StopCoroutine(currentRoutine);
        currentRoutine = null;
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

    private void ClearText()
    {
        if (subtitleText != null)
            subtitleText.text = string.Empty;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null)
            yield break;

        group.interactable = to > 0f;
        group.blocksRaycasts = to > 0f;

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

        group.interactable = to > 0f;
        group.blocksRaycasts = to > 0f;
    }
}