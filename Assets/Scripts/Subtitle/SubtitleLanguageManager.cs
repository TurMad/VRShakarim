using UnityEngine;

public class SubtitleLanguageManager : MonoBehaviour
{
    public static SubtitleLanguageManager Instance { get; private set; }

    [SerializeField] private SubtitleLanguage defaultLanguage = SubtitleLanguage.Russian;

    private SubtitleLanguage currentLanguage;

    public static SubtitleLanguage CurrentLanguage
    {
        get
        {
            EnsureInstance();
            return Instance.currentLanguage;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        currentLanguage = defaultLanguage;
        DontDestroyOnLoad(gameObject);
    }

    public static void SetLanguage(SubtitleLanguage language)
    {
        EnsureInstance();
        Instance.currentLanguage = language;
    }

    private static void EnsureInstance()
    {
        if (Instance != null)
            return;

        GameObject go = new GameObject("SubtitleLanguageManager");
        SubtitleLanguageManager manager = go.AddComponent<SubtitleLanguageManager>();
        Instance = manager;
        DontDestroyOnLoad(go);
    }
}