using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class Module2BookInteractable : MonoBehaviour
{
    [Header("Highlight")]
    [SerializeField] private InteractableHighlight highlight;

    [Header("Instruction")]
    [SerializeField] private InstructionSubtitleSO bookInstructionSubtitle;

    [Header("Pages")]
    [SerializeField] private GameObject[] pageObjects;
    [SerializeField] private int startPageIndex;

    [Header("Animator")]
    [SerializeField] private Animator bookAnimator;
    [SerializeField] private string nextPageTriggerName = "NextPage";
    [SerializeField] private string previousPageTriggerName = "PreviousPage";

    [Header("Input")]
    [SerializeField] private InputActionReference rightTriggerNextPageAction;
    [SerializeField] private InputActionReference leftTriggerPreviousPageAction;
    [SerializeField] private InputActionReference xReturnAction;

    [Header("Return")]
    [SerializeField] private float returnDuration = 0.35f;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private int currentPageIndex;
    private bool isGrabbed;
    private bool wasInteracted;
    private bool isReturning;

    private Coroutine returnRoutine;

    private void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        if (highlight == null)
            highlight = GetComponent<InteractableHighlight>();

        startPosition = transform.position;
        startRotation = transform.rotation;

        currentPageIndex = Mathf.Clamp(startPageIndex, 0, Mathf.Max(0, pageObjects.Length - 1));
        ApplyPage();
    }

    private void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);

        EnableAction(rightTriggerNextPageAction, true);
        EnableAction(leftTriggerPreviousPageAction, true);
        EnableAction(xReturnAction, true);
    }

    private void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnReleased);

        EnableAction(rightTriggerNextPageAction, false);
        EnableAction(leftTriggerPreviousPageAction, false);
        EnableAction(xReturnAction, false);
    }

    private void Update()
    {
        if (!isGrabbed || isReturning)
            return;

        if (rightTriggerNextPageAction != null && rightTriggerNextPageAction.action.WasPressedThisFrame())
            NextPage();

        if (leftTriggerPreviousPageAction != null && leftTriggerPreviousPageAction.action.WasPressedThisFrame())
            PreviousPage();

        if (xReturnAction != null && xReturnAction.action.WasPressedThisFrame())
            ReturnBookToStart();
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        SceneFlowManager.Instance.SetMoveTurnLocked(true);

        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        if (!wasInteracted)
        {
            wasInteracted = true;

            if (highlight != null)
                highlight.StopHighlight();
        }

        if (SubtitleManager.Instance != null && bookInstructionSubtitle != null)
            SubtitleManager.Instance.ShowInstruction(bookInstructionSubtitle);
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;

        if (!isReturning)
            SceneFlowManager.Instance.SetMoveTurnLocked(false);
    }

    private void NextPage()
    {
        if (currentPageIndex >= pageObjects.Length - 1)
            return;

        currentPageIndex++;

        if (bookAnimator != null && !string.IsNullOrEmpty(nextPageTriggerName))
            bookAnimator.SetTrigger(nextPageTriggerName);

        ApplyPage();
    }

    private void PreviousPage()
    {
        if (currentPageIndex <= 0)
            return;

        currentPageIndex--;

        if (bookAnimator != null && !string.IsNullOrEmpty(previousPageTriggerName))
            bookAnimator.SetTrigger(previousPageTriggerName);

        ApplyPage();
    }

    private void ApplyPage()
    {
        for (int i = 0; i < pageObjects.Length; i++)
        {
            if (pageObjects[i] != null)
                pageObjects[i].SetActive(i == currentPageIndex);
        }
    }

    private void ReturnBookToStart()
    {
        isReturning = true;
        isGrabbed = false;

        if (SubtitleManager.Instance != null)
            SubtitleManager.Instance.HideInstruction();

        SceneFlowManager.Instance.SetMoveTurnLocked(false);

        if (grabInteractable != null)
            grabInteractable.enabled = false;

        if (returnRoutine != null)
            StopCoroutine(returnRoutine);

        returnRoutine = StartCoroutine(ReturnToStartRoutine());
    }

    private IEnumerator ReturnToStartRoutine()
    {
        Vector3 fromPosition = transform.position;
        Quaternion fromRotation = transform.rotation;

        float time = 0f;

        while (time < returnDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / returnDuration);

            transform.position = Vector3.Lerp(fromPosition, startPosition, t);
            transform.rotation = Quaternion.Slerp(fromRotation, startRotation, t);

            yield return null;
        }

        transform.position = startPosition;
        transform.rotation = startRotation;

        isReturning = false;
        returnRoutine = null;

        if (grabInteractable != null)
            grabInteractable.enabled = true;
    }

    private void EnableAction(InputActionReference actionReference, bool value)
    {
        if (actionReference == null)
            return;

        if (value)
            actionReference.action.Enable();
        else
            actionReference.action.Disable();
    }
}