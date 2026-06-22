using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class Module4DoorInteractable : MonoBehaviour
{
    [Header("Door")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openTriggerName = "Open";

    [Header("Highlight")]
    [SerializeField] private InteractableHighlight highlight;

    [Header("Disable After Open")]
    [SerializeField] private Collider[] collidersToDisable;
    [SerializeField] private Behaviour[] behavioursToDisableAfterOpen;

    [Header("Optional Trigger Actions")]
    [SerializeField] private InputActionReference[] triggerActions;

    private XRSimpleInteractable simpleInteractable;

    private bool isHovered;
    private bool isOpened;

    private void Awake()
    {
        simpleInteractable = GetComponent<XRSimpleInteractable>();

        if (highlight == null)
            highlight = GetComponent<InteractableHighlight>();
    }

    private void OnEnable()
    {
        if (simpleInteractable != null)
        {
            simpleInteractable.selectEntered.AddListener(OnDoorSelected);
            simpleInteractable.activated.AddListener(OnDoorActivated);
            simpleInteractable.hoverEntered.AddListener(OnDoorHoverEntered);
            simpleInteractable.hoverExited.AddListener(OnDoorHoverExited);
        }

        SetInputActionsEnabled(true);
    }

    private void OnDisable()
    {
        if (simpleInteractable != null)
        {
            simpleInteractable.selectEntered.RemoveListener(OnDoorSelected);
            simpleInteractable.activated.RemoveListener(OnDoorActivated);
            simpleInteractable.hoverEntered.RemoveListener(OnDoorHoverEntered);
            simpleInteractable.hoverExited.RemoveListener(OnDoorHoverExited);
        }

        SetInputActionsEnabled(false);
    }

    private void Update()
    {
        if (isOpened || !isHovered)
            return;

        for (int i = 0; i < triggerActions.Length; i++)
        {
            if (triggerActions[i] == null)
                continue;

            if (triggerActions[i].action.WasPressedThisFrame())
            {
                OpenDoor();
                return;
            }
        }
    }

    private void OnDoorSelected(SelectEnterEventArgs args)
    {
        OpenDoor();
    }

    private void OnDoorActivated(ActivateEventArgs args)
    {
        OpenDoor();
    }

    private void OnDoorHoverEntered(HoverEnterEventArgs args)
    {
        isHovered = true;
    }

    private void OnDoorHoverExited(HoverExitEventArgs args)
    {
        isHovered = false;
    }

    private void OpenDoor()
    {
        if (isOpened)
            return;

        isOpened = true;

        if (highlight != null)
            highlight.StopHighlight();

        if (doorAnimator != null && !string.IsNullOrWhiteSpace(openTriggerName))
            doorAnimator.SetTrigger(openTriggerName);

        for (int i = 0; i < collidersToDisable.Length; i++)
        {
            if (collidersToDisable[i] != null)
                collidersToDisable[i].enabled = false;
        }

        for (int i = 0; i < behavioursToDisableAfterOpen.Length; i++)
        {
            if (behavioursToDisableAfterOpen[i] != null)
                behavioursToDisableAfterOpen[i].enabled = false;
        }

        if (simpleInteractable != null)
            simpleInteractable.enabled = false;
    }

    private void SetInputActionsEnabled(bool value)
    {
        for (int i = 0; i < triggerActions.Length; i++)
        {
            if (triggerActions[i] == null)
                continue;

            if (value)
                triggerActions[i].action.Enable();
            else
                triggerActions[i].action.Disable();
        }
    }
}