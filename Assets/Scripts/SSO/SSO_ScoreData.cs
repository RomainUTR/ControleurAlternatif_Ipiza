using UnityEngine;

[CreateAssetMenu(fileName = "SSO_ScoreData", menuName = "Data/SSO/SSO_ScoreData")]
public class SSO_ScoreData : ScriptableObject
{
    [Header("Scoring Action Data")]
    public int ScratchNote;
    public int HoldNote;
    public int SpamNote;
    public int BonusNote;
}