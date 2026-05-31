using System.Collections;
using TMPro;
using UnityEngine;

public class SubtitleManager : MonoBehaviour
{
    public static SubtitleManager Instance { get; private set; }

    [SerializeField] private CanvasGroup subtitleCanvasGroup;
    [SerializeField] private TMP_Text firstLanguageText;
    [SerializeField] private TMP_Text secondLanguageText;
    [SerializeField] private float fadeDuration = 0.25f;

    private Coroutine playRoutine;

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

        ClearTexts();
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

        playRoutine = StartCoroutine(HideRoutine());
    }

    private IEnumerator PlaySequenceRoutine(SubtitleSequenceSO sequence)
    {
        yield return FadeCanvasGroup(subtitleCanvasGroup, subtitleCanvasGroup.alpha, 1f, fadeDuration);

        for (int i = 0; i < sequence.lines.Count; i++)
        {
            SubtitleLineData line = sequence.lines[i];

            if (firstLanguageText != null)
                firstLanguageText.text = line.firstLanguageText;

            if (secondLanguageText != null)
                secondLanguageText.text = line.secondLanguageText;

            yield return new WaitForSeconds(line.duration);
        }

        yield return HideRoutine();
    }

    private IEnumerator HideRoutine()
    {
        yield return FadeCanvasGroup(subtitleCanvasGroup, subtitleCanvasGroup.alpha, 0f, fadeDuration);
        ClearTexts();
        playRoutine = null;
    }

    private void ClearTexts()
    {
        if (firstLanguageText != null)
            firstLanguageText.text = string.Empty;

        if (secondLanguageText != null)
            secondLanguageText.text = string.Empty;
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