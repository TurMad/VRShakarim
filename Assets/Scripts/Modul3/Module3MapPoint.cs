using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Module3MapPoint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Links")]
    [SerializeField] private Module3MapController mapController;
    [SerializeField] private Button pointButton;

    [Header("Info")]
    [SerializeField] private CanvasGroup infoGroup;
    [SerializeField] private float infoFadeDuration = 0.2f;
    [SerializeField] private float delayAfterAudio = 0.3f;

    [Header("Audio + Subtitles")]
    [SerializeField] private AudioClip pointAudio;
    [SerializeField] private SubtitleSequenceSO pointSubtitles;

    [Header("Special Scene Point")]
    [SerializeField] private bool specialScenePoint;
    [SerializeField] private string sceneToLoad;
    [SerializeField] private float fadeToSceneDuration = 0.5f;

    [Header("Transport")]
    [SerializeField] private RectTransform transportRect;
    [SerializeField] private CanvasGroup transportGroup;
    [SerializeField] private RectTransform[] movePoints;
    [SerializeField] private float totalTravelDuration = 1f;
    [SerializeField] private float transportFadeDuration = 0.15f;
    [SerializeField] private AudioSource transportLoopAudioSource;

    [Header("Route Glow")]
    [SerializeField] private CanvasGroup[] routeObjects;

    private bool isAvailable;
    private bool isBusy;
    private Coroutine infoRoutine;
    private bool[] routeActivated;

    private void Awake()
    {
        if (pointButton != null)
            pointButton.onClick.AddListener(OnPointClicked);

        if (infoGroup != null)
        {
            infoGroup.alpha = 0f;
            infoGroup.interactable = false;
            infoGroup.blocksRaycasts = false;
        }

        if (transportGroup != null)
            transportGroup.alpha = 0f;

        routeActivated = new bool[routeObjects.Length];

        for (int i = 0; i < routeObjects.Length; i++)
        {
            if (routeObjects[i] == null)
                continue;

            routeObjects[i].alpha = 0f;
            routeActivated[i] = routeObjects[i].gameObject.activeSelf;
        }

        if (transportLoopAudioSource != null)
        {
            transportLoopAudioSource.loop = true;
            transportLoopAudioSource.playOnAwake = false;
        }
    }

    public void SetAvailable(bool value)
    {
        isAvailable = value;

        if (pointButton != null)
        {
            pointButton.gameObject.SetActive(value);
            pointButton.interactable = value;
        }

        if (!value && infoGroup != null)
        {
            infoGroup.alpha = 0f;
            infoGroup.interactable = false;
            infoGroup.blocksRaycasts = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isAvailable || isBusy || infoGroup == null)
            return;

        StartInfoFade(1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isAvailable || isBusy || infoGroup == null)
            return;

        StartInfoFade(0f);
    }

    private void OnPointClicked()
    {
        if (!isAvailable || isBusy)
            return;

        StartCoroutine(PointRoutine());
    }

    private IEnumerator PointRoutine()
    {
        isBusy = true;

        SceneFlowManager.Instance.SetXRLocked(true);
        SceneFlowManager.Instance.SetMoveTurnLocked(true);

        if (pointButton != null)
            pointButton.interactable = false;

        if (infoRoutine != null)
        {
            StopCoroutine(infoRoutine);
            infoRoutine = null;
        }

        // Инфо-блок после клика должен остаться видимым
        if (infoGroup != null)
        {
            infoGroup.alpha = 1f;
            infoGroup.interactable = false;
            infoGroup.blocksRaycasts = false;
        }

        yield return AnimateRouteAndTransportRoutine();

        if (specialScenePoint)
        {
            yield return SceneFlowManager.Instance.FadeToBlack(fadeToSceneDuration);

            if (!string.IsNullOrWhiteSpace(sceneToLoad))
                SceneManager.LoadScene(sceneToLoad);

            yield break;
        }

        if (SubtitleManager.Instance != null && pointSubtitles != null)
            SubtitleManager.Instance.PlaySequence(pointSubtitles);

        SceneFlowManager.Instance.PlayAudio(pointAudio);
        yield return SceneFlowManager.Instance.WaitForAudioFinished();

        if (SubtitleManager.Instance != null)
            SubtitleManager.Instance.StopSequence();

        yield return new WaitForSeconds(delayAfterAudio);

        if (infoGroup != null)
            yield return FadeCanvasGroup(infoGroup, infoGroup.alpha, 0f, infoFadeDuration);

        isBusy = false;

        if (mapController != null)
            mapController.NotifyPointFinished(this);

        // Контроллеры возвращаем, но Move/Turn остаются заблокированы
        SceneFlowManager.Instance.SetXRLocked(false);
        SceneFlowManager.Instance.SetMoveTurnLocked(true);
    }

    private IEnumerator AnimateRouteAndTransportRoutine()
    {
        if (transportRect == null || transportGroup == null || movePoints == null || movePoints.Length < 2)
            yield break;

        float duration = Mathf.Max(0.01f, totalTravelDuration);

        for (int i = 0; i < routeObjects.Length; i++)
        {
            if (routeObjects[i] == null)
                continue;

            routeObjects[i].alpha = 0f;
            routeActivated[i] = routeObjects[i].gameObject.activeSelf;
        }

        transportRect.position = movePoints[0].position;
        transportGroup.alpha = 1f;

        if (transportLoopAudioSource != null)
            transportLoopAudioSource.Play();

        int routeCount = routeObjects != null ? routeObjects.Length : 0;
        int segmentCount = movePoints.Length - 1;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);

            UpdateTransportPosition(progress, segmentCount);
            UpdateRouteObjects(progress, routeCount);

            yield return null;
        }

        UpdateTransportPosition(1f, segmentCount);
        UpdateRouteObjects(1f, routeCount);

        if (transportLoopAudioSource != null)
            transportLoopAudioSource.Stop();

        yield return FadeCanvasGroup(transportGroup, transportGroup.alpha, 0f, transportFadeDuration);
    }

    private void UpdateTransportPosition(float progress, int segmentCount)
    {
        if (segmentCount <= 0)
            return;

        float pathProgress = progress * segmentCount;
        int segmentIndex = Mathf.Min(Mathf.FloorToInt(pathProgress), segmentCount - 1);
        float segmentT = pathProgress - segmentIndex;

        Vector3 startPos = movePoints[segmentIndex].position;
        Vector3 endPos = movePoints[segmentIndex + 1].position;

        transportRect.position = Vector3.Lerp(startPos, endPos, segmentT);
    }

    private void UpdateRouteObjects(float progress, int routeCount)
    {
        if (routeCount == 0)
            return;

        float scaled = progress * routeCount;

        for (int i = 0; i < routeCount; i++)
        {
            if (routeObjects[i] == null)
                continue;

            if (!routeActivated[i] && scaled > i)
            {
                routeObjects[i].gameObject.SetActive(true);
                routeActivated[i] = true;
                routeObjects[i].alpha = 0f;
            }

            if (routeActivated[i])
            {
                float alpha = Mathf.Clamp01(scaled - i);
                routeObjects[i].alpha = alpha;
            }
        }
    }

    private void StartInfoFade(float target)
    {
        if (infoRoutine != null)
            StopCoroutine(infoRoutine);

        infoRoutine = StartCoroutine(FadeCanvasGroup(infoGroup, infoGroup.alpha, target, infoFadeDuration));
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