using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SceneIntroTransition : MonoBehaviour
{
    [Header("Fade Volume")]
    [SerializeField] private Volume fadeVolume;
    [SerializeField] private float fadeFromBlackDuration = 1f;
    [SerializeField] private float fadeToBlackDuration = 0.6f;

    [Header("Language UI")]
    [SerializeField] private CanvasGroup languageCanvasGroup;
    [SerializeField] private Button russianButton;
    [SerializeField] private Button kazakhButton;
    [SerializeField] private Button englishButton;

    [Header("Language Voice Hint")]
    [SerializeField] private AudioSource hintAudioSource;
    [SerializeField] private AudioClip chooseLanguageHintClip;
    [SerializeField] private float delayBeforeFirstHint = 0.5f;
    [SerializeField] private float delayBetweenHints = 3f;

    [Header("Scene")]
    [SerializeField] private string nextSceneName;

    [Header("Timing")]
    [SerializeField] private float delayBeforeCanvasShow = 1.5f;
    [SerializeField] private float canvasFadeDuration = 0.35f;

    [Header("Module 3 Star Intro")]
    [SerializeField] private bool useStarSelectionAfterLanguage;
    [SerializeField] private XRSimpleInteractable starInteractable;
    [SerializeField] private Behaviour[] starBehavioursToEnableAfterLanguage;
    [SerializeField] private InteractableHighlight[] starHighlightsToStartAfterLanguage;

    private bool isLoading;
    private bool languageSelected;
    private Coroutine hintRoutine;

    private void Awake()
    {
        if (fadeVolume != null)
            fadeVolume.weight = 1f;

        if (languageCanvasGroup != null)
        {
            languageCanvasGroup.alpha = 0f;
            languageCanvasGroup.interactable = false;
            languageCanvasGroup.blocksRaycasts = false;
        }

        if (russianButton != null)
            russianButton.onClick.AddListener(() => SelectLanguage(SubtitleLanguage.Russian));

        if (kazakhButton != null)
            kazakhButton.onClick.AddListener(() => SelectLanguage(SubtitleLanguage.Kazakh));

        if (englishButton != null)
            englishButton.onClick.AddListener(() => SelectLanguage(SubtitleLanguage.English));

        if (hintAudioSource != null)
        {
            hintAudioSource.playOnAwake = false;
            hintAudioSource.loop = false;
        }

        if (starInteractable != null)
        {
            starInteractable.selectEntered.AddListener(OnStarSelected);
            starInteractable.enabled = false;
        }

        for (int i = 0; i < starBehavioursToEnableAfterLanguage.Length; i++)
        {
            if (starBehavioursToEnableAfterLanguage[i] != null)
                starBehavioursToEnableAfterLanguage[i].enabled = false;
        }
    }

    private void OnDestroy()
    {
        if (starInteractable != null)
            starInteractable.selectEntered.RemoveListener(OnStarSelected);
    }

    private IEnumerator Start()
    {
        yield return FadeVolume(1f, 0f, fadeFromBlackDuration);

        yield return new WaitForSeconds(delayBeforeCanvasShow);

        if (languageCanvasGroup != null)
        {
            yield return FadeCanvasGroup(languageCanvasGroup, 0f, 1f, canvasFadeDuration);
            languageCanvasGroup.interactable = true;
            languageCanvasGroup.blocksRaycasts = true;
        }

        StartHintLoop();
    }

    private void SelectLanguage(SubtitleLanguage language)
    {
        if (isLoading || languageSelected)
            return;

        StartCoroutine(SelectLanguageRoutine(language));
    }

    private IEnumerator SelectLanguageRoutine(SubtitleLanguage language)
    {
        languageSelected = true;

        SubtitleLanguageManager.SetLanguage(language);

        StopHintLoop();
        SetButtonsInteractable(false);

        if (languageCanvasGroup != null)
        {
            languageCanvasGroup.interactable = false;
            languageCanvasGroup.blocksRaycasts = false;
            yield return FadeCanvasGroup(languageCanvasGroup, languageCanvasGroup.alpha, 0f, canvasFadeDuration);
        }

        if (useStarSelectionAfterLanguage)
        {
            EnableStarSelection();
            yield break;
        }

        yield return LoadNextSceneRoutine();
    }

    private void EnableStarSelection()
    {
        if (starInteractable != null)
            starInteractable.enabled = true;

        for (int i = 0; i < starBehavioursToEnableAfterLanguage.Length; i++)
        {
            if (starBehavioursToEnableAfterLanguage[i] != null)
                starBehavioursToEnableAfterLanguage[i].enabled = true;
        }

        for (int i = 0; i < starHighlightsToStartAfterLanguage.Length; i++)
        {
            if (starHighlightsToStartAfterLanguage[i] != null)
                starHighlightsToStartAfterLanguage[i].StartHighlight();
        }
    }

    private void OnStarSelected(SelectEnterEventArgs args)
    {
        if (!useStarSelectionAfterLanguage)
            return;

        if (!languageSelected || isLoading)
            return;

        StartCoroutine(LoadNextSceneRoutine());
    }

    private IEnumerator LoadNextSceneRoutine()
    {
        isLoading = true;

        if (starInteractable != null)
            starInteractable.enabled = false;

        yield return FadeVolume(fadeVolume != null ? fadeVolume.weight : 0f, 1f, fadeToBlackDuration);

        if (!string.IsNullOrWhiteSpace(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

    private void StartHintLoop()
    {
        if (hintAudioSource == null || chooseLanguageHintClip == null)
            return;

        if (hintRoutine != null)
            StopCoroutine(hintRoutine);

        hintRoutine = StartCoroutine(HintLoopRoutine());
    }

    private void StopHintLoop()
    {
        if (hintRoutine != null)
        {
            StopCoroutine(hintRoutine);
            hintRoutine = null;
        }

        if (hintAudioSource != null)
            hintAudioSource.Stop();
    }

    private IEnumerator HintLoopRoutine()
    {
        yield return new WaitForSeconds(delayBeforeFirstHint);

        while (!languageSelected)
        {
            hintAudioSource.Stop();
            hintAudioSource.clip = chooseLanguageHintClip;
            hintAudioSource.Play();

            while (hintAudioSource.isPlaying && !languageSelected)
                yield return null;

            if (languageSelected)
                break;

            yield return new WaitForSeconds(delayBetweenHints);
        }

        hintRoutine = null;
    }

    private void SetButtonsInteractable(bool value)
    {
        if (russianButton != null)
            russianButton.interactable = value;

        if (kazakhButton != null)
            kazakhButton.interactable = value;

        if (englishButton != null)
            englishButton.interactable = value;
    }

    private IEnumerator FadeVolume(float from, float to, float duration)
    {
        if (fadeVolume == null)
            yield break;

        float time = 0f;
        fadeVolume.weight = from;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = duration <= 0f ? 1f : Mathf.Clamp01(time / duration);
            fadeVolume.weight = Mathf.Lerp(from, to, t);
            yield return null;
        }

        fadeVolume.weight = to;
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