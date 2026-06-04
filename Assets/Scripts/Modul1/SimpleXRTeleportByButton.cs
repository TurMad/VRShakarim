using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleXRTeleportByButton : MonoBehaviour
{
    [Header("XR")]
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private Transform rayOrigin;

    [Header("Input")]
    [SerializeField] private InputActionReference teleportAction;

    [Header("Raycast")]
    [SerializeField] private LayerMask teleportLayerMask;
    [SerializeField] private float maxDistance = 20f;

    [Header("Visual")]
    [SerializeField] private GameObject teleportIndicator;

    private bool hasValidPoint;
    private Vector3 targetPoint;

    private void OnEnable()
    {
        if (teleportAction != null)
            teleportAction.action.Enable();

        if (teleportIndicator != null)
            teleportIndicator.SetActive(false);
    }

    private void OnDisable()
    {
        if (teleportAction != null)
            teleportAction.action.Disable();

        if (teleportIndicator != null)
            teleportIndicator.SetActive(false);

        hasValidPoint = false;
    }

    private void Update()
    {
        UpdateRaycast();

        if (hasValidPoint && teleportAction != null && teleportAction.action.WasPressedThisFrame())
            TeleportToTarget();
    }

    private void UpdateRaycast()
    {
        hasValidPoint = false;

        if (rayOrigin == null)
            return;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, teleportLayerMask))
        {
            hasValidPoint = true;
            targetPoint = hit.point;

            if (teleportIndicator != null)
            {
                teleportIndicator.SetActive(true);
                teleportIndicator.transform.position = targetPoint;
                teleportIndicator.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }
        }
        else
        {
            if (teleportIndicator != null)
                teleportIndicator.SetActive(false);
        }
    }

    private void TeleportToTarget()
    {
        if (xrOrigin == null || xrOrigin.Camera == null)
            return;

        Transform cameraTransform = xrOrigin.Camera.transform;

        Vector3 cameraOffset = xrOrigin.transform.position - cameraTransform.position;
        cameraOffset.y = 0f;

        xrOrigin.transform.position = targetPoint + cameraOffset;
    }
}