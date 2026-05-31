using UnityEngine;

public class CameraSway : MonoBehaviour
{
    [SerializeField] private Transform targetCamera;
    [SerializeField] private float positionAmount = 0.015f;
    [SerializeField] private float rotationAmount = 0.6f;
    [SerializeField] private float speed = 0.9f;

    private Vector3 baseLocalPos;
    private Vector3 baseLocalEuler;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = transform;

        baseLocalPos = targetCamera.localPosition;
        baseLocalEuler = targetCamera.localEulerAngles;
    }

    private void OnEnable()
    {
        if (targetCamera == null)
            return;

        baseLocalPos = targetCamera.localPosition;
        baseLocalEuler = targetCamera.localEulerAngles;
    }

    private void OnDisable()
    {
        if (targetCamera == null)
            return;

        targetCamera.localPosition = baseLocalPos;
        targetCamera.localEulerAngles = baseLocalEuler;
    }

    private void Update()
    {
        float t = Time.time * speed;

        Vector3 posOffset = new Vector3(
            Mathf.Sin(t * 1.1f) * positionAmount,
            Mathf.Sin(t * 1.7f) * positionAmount * 0.7f,
            0f);

        Vector3 rotOffset = new Vector3(
            Mathf.Sin(t * 1.3f) * rotationAmount,
            Mathf.Sin(t * 0.8f) * rotationAmount,
            Mathf.Sin(t * 1.1f) * rotationAmount * 0.6f);

        targetCamera.localPosition = baseLocalPos + posOffset;
        targetCamera.localEulerAngles = baseLocalEuler + rotOffset;
    }
}