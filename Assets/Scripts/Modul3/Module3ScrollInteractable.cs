using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
public class Module3ScrollInteractable : MonoBehaviour
{
    [Header("Scroll")]
    [SerializeField] private InteractableHighlight highlight;

    [Header("Map UI")]
    [SerializeField] private CanvasGroup mapCanvasGroup;
    [SerializeField] private float delayBeforeMapShow = 0.2f;
    [SerializeField] private float mapFadeDuration = 0.35f;

    [Header("Instruction Audio")]
    [SerializeField] private AudioClip instructionAudio;
    [SerializeField] private float delayBeforeInstructionAudio = 0.3f;

    [Header("Next")]
    [SerializeField] private Module3MapController mapController;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;
    private bool opened;

    private void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();

        if (highlight == null)
            highlight = GetComponent<InteractableHighlight>();

        if (mapCanvasGroup != null)
        {
            mapCanvasGroup.alpha = 0f;
            mapCanvasGroup.interactable = false;
            mapCanvasGroup.blocksRaycasts = false;
        }
    }

    private void OnEnable()
    {
        interactable.selectEntered.AddListener(OnSelected);
    }

    private void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnSelected);
    }

    private void OnSelected(SelectEnterEventArgs args)
    {
        if (opened)
            return;

        opened = true;
        StartCoroutine(OpenMapRoutine());
    }

    private IEnumerator OpenMapRoutine()
    {
        if (highlight != null)
            highlight.StopHighlight();

        SceneFlowManager.Instance.SetMoveTurnLocked(true);

        interactable.enabled = false;

        // ВАЖНО: дать XR Toolkit закончить select в этом кадре
        yield return null;

        SceneFlowManager.Instance.SetXRLocked(true);

        yield return new WaitForSeconds(delayBeforeMapShow);

        if (mapCanvasGroup != null)
        {
            yield return FadeCanvasGroup(mapCanvasGroup, 0f, 1f, mapFadeDuration);
            mapCanvasGroup.interactable = true;
            mapCanvasGroup.blocksRaycasts = true;
        }

        yield return new WaitForSeconds(delayBeforeInstructionAudio);

        SceneFlowManager.Instance.PlayAudio(instructionAudio);
        yield return SceneFlowManager.Instance.WaitForAudioFinished();

        SceneFlowManager.Instance.SetXRLocked(false);

        // Move/Turn пока оставляем заблокированным, как ты и хотел
        SceneFlowManager.Instance.SetMoveTurnLocked(true);

        if (mapController != null)
            mapController.BeginMapSequence();
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
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