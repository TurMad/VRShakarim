using UnityEngine;

public class Module3MusicSceneHook : MonoBehaviour
{
    public enum MusicMode
    {
        StartFromZero,
        ContinueCurrent,
        Stop
    }

    [SerializeField] private MusicMode musicMode;
    [SerializeField] private AudioClip module3Music;

    private void Start()
    {
        switch (musicMode)
        {
            case MusicMode.StartFromZero:
                Module3MusicManager.PlayFromStart(module3Music);
                break;

            case MusicMode.ContinueCurrent:
                Module3MusicManager.ContinueOrPlay(module3Music);
                break;

            case MusicMode.Stop:
                Module3MusicManager.StopMusic();
                break;
        }
    }
}