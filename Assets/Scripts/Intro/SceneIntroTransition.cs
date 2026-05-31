using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class SceneIntroTransition : MonoBehaviour
{
    [Header("Fade Volume")]
    [SerializeField] private Volume fadeVolume;
    [SerializeField] private float fadeFromBlackDuration = 1f;
    [SerializeField] private float fadeToBlackDuration = 0.6f;

    [Header("UI")]
    [SerializeField] private CanvasGroup introCanvasGroup;
    [SerializeField] private TMP_Text introText;

    [Header("Content")]
    [TextArea(2, 10)]
    [SerializeField] private string textToShow;
    [SerializeField] private string nextSceneName;

    [Header("Input")]
    [SerializeField] private InputActionReference[] continueActions;

    [Header("Timing")]
    [SerializeField] private float delayBeforeIntroUI = 2f;
    [SerializeField] private float introUIFadeDuration = 0.4f;

    private bool canContinue;
    private bool isLoading;

    private void Awake()
    {
        if (introText != null)
            introText.text = textToShow;

        if (introCanvasGroup != null)
        {
            introCanvasGroup.alpha = 0f;
            introCanvasGroup.interactable = false;
            introCanvasGroup.blocksRaycasts = false;
        }

        if (fadeVolume != null)
            fadeVolume.weight = 1f;
    }

    private void OnEnable()
    {
        for (int i = 0; i < continueActions.Length; i++)
        {
            if (continueActions[i] != null)
                continueActions[i].action.Enable();
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < continueActions.Length; i++)
        {
            if (continueActions[i] != null)
                continueActions[i].action.Disable();
        }
    }

    private IEnumerator Start()
    {
        yield return FadeVolume(1f, 0f, fadeFromBlackDuration);

        yield return new WaitForSeconds(delayBeforeIntroUI);

        if (introCanvasGroup != null)
            yield return FadeCanvasGroup(introCanvasGroup, 0f, 1f, introUIFadeDuration);

        canContinue = true;
    }

    private void Update()
    {
        if (!canContinue || isLoading)
            return;

        for (int i = 0; i < continueActions.Length; i++)
        {
            if (continueActions[i] != null && continueActions[i].action.WasPressedThisFrame())
            {
                StartCoroutine(LoadNextSceneRoutine());
                return;
            }
        }
    }

    private IEnumerator LoadNextSceneRoutine()
    {
        isLoading = true;
        canContinue = false;

        yield return FadeVolume(fadeVolume != null ? fadeVolume.weight : 0f, 1f, fadeToBlackDuration);

        if (!string.IsNullOrWhiteSpace(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
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