using UnityEngine;

[CreateAssetMenu(fileName = "InstructionSubtitle", menuName = "VR/Subtitles/Instruction Subtitle")]
public class InstructionSubtitleSO : ScriptableObject
{
    [TextArea(2, 6)]
    public string russianText;

    [TextArea(2, 6)]
    public string kazakhText;

    [TextArea(2, 6)]
    public string englishText;
}