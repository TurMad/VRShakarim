using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SubtitleSequence", menuName = "VR/Subtitles/Subtitle Sequence")]
public class SubtitleSequenceSO : ScriptableObject
{
    public List<SubtitleLineData> lines = new();
}

[Serializable]
public class SubtitleLineData
{
    [TextArea(2, 6)]
    public string firstLanguageText;

    [TextArea(2, 6)]
    public string secondLanguageText;

    [Min(0.05f)]
    public float duration = 2f;
}