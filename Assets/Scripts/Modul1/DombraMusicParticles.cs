using System.Collections;
using UnityEngine;

public class DombraMusicParticles : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] particleSystems;
    [SerializeField] private float startDelay = 0.25f;

    private Coroutine playRoutine;

    public void PlayEffect()
    {
        StopEffect();

        playRoutine = StartCoroutine(PlayRoutine());
    }

    public void StopEffect()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }

        for (int i = 0; i < particleSystems.Length; i++)
        {
            if (particleSystems[i] != null)
                particleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    private IEnumerator PlayRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        for (int i = 0; i < particleSystems.Length; i++)
        {
            if (particleSystems[i] != null)
                particleSystems[i].Play();
        }

        playRoutine = null;
    }
}