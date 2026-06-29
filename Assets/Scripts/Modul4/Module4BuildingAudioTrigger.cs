using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Module4BuildingAudioTrigger : MonoBehaviour
{
    [Header("Audio Inside Building")]
    [Tooltip("Сюда добавь второе и третье аудио из прежнего вступления.")]
    [SerializeField] private AudioClip[] buildingVoiceClips = new AudioClip[2];

    [Tooltip("Субтитры для второго и третьего аудио.")]
    [SerializeField] private SubtitleSequenceSO[] buildingSubtitles = new SubtitleSequenceSO[2];

    [SerializeField] private float delayBeforeFirstAudio = 0f;
    [SerializeField] private float delayBetweenAudios = 0.35f;

    [Header("Trigger Settings")]
    [SerializeField] private bool triggerOnlyOnce = true;
    [SerializeField] private bool disableColliderAfterTrigger = true;

    private Collider triggerCollider;
    private bool wasTriggered;

    private void Reset()
    {
        triggerCollider = GetComponent<Collider>();

        if (triggerCollider != null)
            triggerCollider.isTrigger = true;
    }

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();

        if (triggerCollider != null)
            triggerCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnlyOnce && wasTriggered)
            return;

        if (!IsPlayerCollider(other))
            return;

        wasTriggered = true;

        if (disableColliderAfterTrigger && triggerCollider != null)
            triggerCollider.enabled = false;

        StartCoroutine(PlayBuildingAudioRoutine());
    }

    private bool IsPlayerCollider(Collider other)
    {
        return other.GetComponentInParent<XROrigin>() != null;
    }

    private IEnumerator PlayBuildingAudioRoutine()
    {
        // Ничего не блокируем:
        // игрок может ходить, смотреть, открывать объекты и телепортироваться.

        if (delayBeforeFirstAudio > 0f)
            yield return new WaitForSeconds(delayBeforeFirstAudio);

        int clipCount = buildingVoiceClips != null ? buildingVoiceClips.Length : 0;

        for (int i = 0; i < clipCount; i++)
        {
            AudioClip currentClip = buildingVoiceClips[i];

            if (currentClip == null)
                continue;

            SubtitleSequenceSO currentSubtitles = null;

            if (buildingSubtitles != null && i < buildingSubtitles.Length)
                currentSubtitles = buildingSubtitles[i];

            if (SubtitleManager.Instance != null && currentSubtitles != null)
                SubtitleManager.Instance.PlaySequence(currentSubtitles);

            SceneFlowManager.Instance.PlayAudio(currentClip);
            yield return SceneFlowManager.Instance.WaitForAudioFinished();

            if (SubtitleManager.Instance != null)
                SubtitleManager.Instance.StopSequence();

            bool hasNextAudio = i < clipCount - 1;

            if (hasNextAudio && delayBetweenAudios > 0f)
                yield return new WaitForSeconds(delayBetweenAudios);
        }
    }
}