using DG.Tweening;
using UnityEngine;

public class LoopScalePulse : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float endScale = 1.25f;
    [SerializeField] private float duration = 0.5f;

    private Vector3 startScale;
    private Tween pulseTween;

    private void Awake()
    {
        if (target == null)
            target = transform;

        startScale = target.localScale;
    }

    private void OnEnable()
    {
        StartPulse();
    }

    private void OnDisable()
    {
        StopPulse();
    }

    public void StartPulse()
    {
        StopPulse();

        target.localScale = startScale;
        pulseTween = target.DOScale(startScale * endScale, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void StopPulse()
    {
        if (pulseTween != null && pulseTween.IsActive())
            pulseTween.Kill();

        if (target != null)
            target.localScale = startScale;
    }
}