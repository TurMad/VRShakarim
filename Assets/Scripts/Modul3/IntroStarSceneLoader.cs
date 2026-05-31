using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
public class IntroStarSceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "Modul3";
    [SerializeField] private Volume fadeVolume;
    [SerializeField] private float fadeToBlackDuration = 0.6f;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;
    private bool isLoading;

    private void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
    }

    private void OnEnable()
    {
        interactable.selectEntered.AddListener(OnSelectEntered);
    }

    private void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnSelectEntered);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (isLoading)
            return;

        StartCoroutine(LoadSceneRoutine());
    }

    private IEnumerator LoadSceneRoutine()
    {
        isLoading = true;

        yield return FadeVolume(0f, 1f, fadeToBlackDuration);

        if (!string.IsNullOrWhiteSpace(sceneToLoad))
            SceneManager.LoadScene(sceneToLoad);
    }

    private IEnumerator FadeVolume(float from, float to, float duration)
    {
        if (fadeVolume == null)
        {
            if (!string.IsNullOrWhiteSpace(sceneToLoad))
                SceneManager.LoadScene(sceneToLoad);

            yield break;
        }

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
}