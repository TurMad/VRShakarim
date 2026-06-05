using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class Module2BookInteractable : MonoBehaviour
{
    [Header("View")]
    [SerializeField] private Transform viewPoint;
    [SerializeField] private float moveToViewDuration = 0.35f;
    [SerializeField] private float returnDuration = 0.35f;

    [Header("Highlight")]
    [SerializeField] private InteractableHighlight highlight;

    [Header("Instruction")]
    [SerializeField] private InstructionSubtitleSO bookInstructionSubtitle;

    [Header("Pages UI")]
    [SerializeField] private CanvasGroup pagesCanvasGroup;
    [SerializeField] private Image leftPageImage;
    [SerializeField] private Image rightPageImage;
    [SerializeField] private Sprite[] pageSprites;
    [SerializeField] private float pagesFadeDuration = 0.15f;

    [Header("Animator")]
    [SerializeField] private Animator bookAnimator;
    [SerializeField] private string openTriggerName = "Open";
    [SerializeField] private string closeTriggerName = "Close";
    [SerializeField] private string nextPageTriggerName = "Next";
    [SerializeField] private string previousPageTriggerName = "Previous";
    [SerializeField] private float delayAfterOpenAnimation = 0.6f;
    [SerializeField] private float pageAnimationDelay = 0.45f;

    [Header("Input")]
    [SerializeField] private InputActionReference rightTriggerNextPageAction;
    [SerializeField] private InputActionReference leftTriggerPreviousPageAction;
    [SerializeField] private InputActionReference xReturnAction;

    private XRGrabInteractable grabInteractable;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private int currentLeftPageIndex;
    private bool isBookActive;
    private bool isChangingPage;
    private bool isReturning;
    private bool wasInteracted;

    private Coroutine moveRoutine;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (highlight == null)
            highlight = GetComponent<InteractableHighlight>();

        startPosition = transform.position;
        startRotation = transform.rotation;

        currentLeftPageIndex = 0;

        HidePagesInstant();
        ApplyPages();
    }

    private void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrabbed);

        EnableAction(rightTriggerNextPageAction, true);
        EnableAction(leftTriggerPreviousPageAction, true);
        EnableAction(xReturnAction, true);
    }

    private void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);

        EnableAction(rightTriggerNextPageAction, false);
        EnableAction(leftTriggerPreviousPageAction, false);
        EnableAction(xReturnAction, false);
    }

    private void Update()
    {
        if (!isBookActive || isChangingPage || isReturning)
            return;

        if (rightTriggerNextPageAction != null && rightTriggerNextPageAction.action.WasPressedThisFrame())
            NextPage();

        if (leftTriggerPreviousPageAction != null && leftTriggerPreviousPageAction.action.WasPressedThisFrame())
            PreviousPage();

        if (xReturnAction != null && xReturnAction.action.WasPressedThisFrame())
            CloseBook();
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        if (isBookActive || isReturning)
            return;

        StartCoroutine(OpenBookRoutine());
    }

    private IEnumerator OpenBookRoutine()
    {
        isBookActive = true;

        SceneFlowManager.Instance.SetMoveTurnLocked(true);

        if (!wasInteracted)
        {
            wasInteracted = true;

            if (highlight != null)
                highlight.StopHighlight();
        }

        grabInteractable.enabled = false;

        currentLeftPageIndex = 0;
        ApplyPages();
        HidePagesInstant();

        if (viewPoint != null)
            yield return MoveToTargetRoutine(viewPoint.position, viewPoint.rotation, moveToViewDuration);

        if (bookAnimator != null && !string.IsNullOrEmpty(openTriggerName))
            bookAnimator.SetTrigger(openTriggerName);

        yield return new WaitForSeconds(delayAfterOpenAnimation);

        if (SubtitleManager.Instance != null && bookInstructionSubtitle != null)
            SubtitleManager.Instance.ShowInstruction(bookInstructionSubtitle);

        yield return FadePages(0f, 1f);
    }

    private void NextPage()
    {
        if (currentLeftPageIndex + 2 >= pageSprites.Length)
            return;

        StartCoroutine(ChangePageRoutine(true));
    }

    private void PreviousPage()
    {
        if (currentLeftPageIndex - 2 < 0)
            return;

        StartCoroutine(ChangePageRoutine(false));
    }

    private IEnumerator ChangePageRoutine(bool next)
    {
        isChangingPage = true;

        yield return FadePages(pagesCanvasGroup != null ? pagesCanvasGroup.alpha : 1f, 0f);

        if (bookAnimator != null)
        {
            string triggerName = next ? nextPageTriggerName : previousPageTriggerName;

            if (!string.IsNullOrEmpty(triggerName))
                bookAnimator.SetTrigger(triggerName);
        }

        yield return new WaitForSeconds(pageAnimationDelay);

        if (next)
            currentLeftPageIndex += 2;
        else
            currentLeftPageIndex -= 2;

        ApplyPages();

        yield return FadePages(0f, 1f);

        isChangingPage = false;
    }

    private void CloseBook()
    {
        if (isReturning)
            return;

        StartCoroutine(CloseBookRoutine());
    }

    private IEnumerator CloseBookRoutine()
    {
        isReturning = true;
        isBookActive = false;

        if (SubtitleManager.Instance != null)
            SubtitleManager.Instance.HideInstruction();

        yield return FadePages(pagesCanvasGroup != null ? pagesCanvasGroup.alpha : 1f, 0f);

        if (bookAnimator != null && !string.IsNullOrEmpty(closeTriggerName))
            bookAnimator.SetTrigger(closeTriggerName);

        yield return MoveToTargetRoutine(startPosition, startRotation, returnDuration);

        transform.position = startPosition;
        transform.rotation = startRotation;

        grabInteractable.enabled = true;

        SceneFlowManager.Instance.SetMoveTurnLocked(false);

        isReturning = false;
    }

    private void ApplyPages()
    {
        if (leftPageImage != null)
        {
            if (pageSprites != null && currentLeftPageIndex >= 0 && currentLeftPageIndex < pageSprites.Length)
            {
                leftPageImage.enabled = true;
                leftPageImage.sprite = pageSprites[currentLeftPageIndex];
            }
            else
            {
                leftPageImage.enabled = false;
            }
        }

        if (rightPageImage != null)
        {
            int rightIndex = currentLeftPageIndex + 1;

            if (pageSprites != null && rightIndex >= 0 && rightIndex < pageSprites.Length)
            {
                rightPageImage.enabled = true;
                rightPageImage.sprite = pageSprites[rightIndex];
            }
            else
            {
                rightPageImage.enabled = false;
            }
        }
    }

    private IEnumerator MoveToTargetRoutine(Vector3 targetPosition, Quaternion targetRotation, float duration)
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        Vector3 fromPosition = transform.position;
        Quaternion fromRotation = transform.rotation;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = duration <= 0f ? 1f : Mathf.Clamp01(time / duration);

            transform.position = Vector3.Lerp(fromPosition, targetPosition, t);
            transform.rotation = Quaternion.Slerp(fromRotation, targetRotation, t);

            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;
    }

    private IEnumerator FadePages(float from, float to)
    {
        if (pagesCanvasGroup == null)
            yield break;

        float time = 0f;
        pagesCanvasGroup.alpha = from;

        while (time < pagesFadeDuration)
        {
            time += Time.deltaTime;
            float t = pagesFadeDuration <= 0f ? 1f : Mathf.Clamp01(time / pagesFadeDuration);
            pagesCanvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        pagesCanvasGroup.alpha = to;
    }

    private void HidePagesInstant()
    {
        if (pagesCanvasGroup == null)
            return;

        pagesCanvasGroup.alpha = 0f;
        pagesCanvasGroup.interactable = false;
        pagesCanvasGroup.blocksRaycasts = false;
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