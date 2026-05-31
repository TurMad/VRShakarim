using System.Collections.Generic;
using UnityEngine;

public class InteractableHighlight : MonoBehaviour
{
    [SerializeField] private Renderer[] targetRenderers;
    [SerializeField] private Color pulseColor = Color.cyan;
    [SerializeField] private float minIntensity = 0f;
    [SerializeField] private float maxIntensity = 2f;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private bool playOnEnable = true;

    private readonly List<Material> runtimeMaterials = new();
    private readonly List<Color> defaultEmissionColors = new();
    private bool isPulsing;

    private void Awake()
    {
        if (targetRenderers == null || targetRenderers.Length == 0)
            targetRenderers = GetComponentsInChildren<Renderer>(true);

        CacheMaterials();
    }

    private void OnEnable()
    {
        if (playOnEnable)
            StartHighlight();
        else
            StopHighlight();
    }

    private void Update()
    {
        if (!isPulsing || runtimeMaterials.Count == 0)
            return;

        float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);

        for (int i = 0; i < runtimeMaterials.Count; i++)
        {
            if (runtimeMaterials[i] != null)
                runtimeMaterials[i].SetColor("_EmissionColor", pulseColor * intensity);
        }
    }

    private void CacheMaterials()
    {
        runtimeMaterials.Clear();
        defaultEmissionColors.Clear();

        for (int r = 0; r < targetRenderers.Length; r++)
        {
            if (targetRenderers[r] == null)
                continue;

            Material[] mats = targetRenderers[r].materials;
            for (int m = 0; m < mats.Length; m++)
            {
                if (mats[m] == null || !mats[m].HasProperty("_EmissionColor"))
                    continue;

                mats[m].EnableKeyword("_EMISSION");
                runtimeMaterials.Add(mats[m]);
                defaultEmissionColors.Add(mats[m].GetColor("_EmissionColor"));
            }
        }
    }

    public void StartHighlight()
    {
        isPulsing = true;
    }

    public void StopHighlight()
    {
        isPulsing = false;

        for (int i = 0; i < runtimeMaterials.Count; i++)
        {
            if (runtimeMaterials[i] != null)
                runtimeMaterials[i].SetColor("_EmissionColor", defaultEmissionColors[i]);
        }
    }
}