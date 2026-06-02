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
    public string russianText;

    [TextArea(2, 6)]
    public string kazakhText;

    [TextArea(2, 6)]
    public string englishText;

    [Min(0.05f)]
    public float duration = 2f;
}