using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SSO_ScoreData", menuName = "Data/SSO/SSO_ScoreData")]
public class SSO_ScoreData : ScriptableObject
{
    [Header("Scoring Action Data")]
    public int ScratchNote;
    public int HoldNote;
    public int SpamNote;
    public int BonusNote;

    [InfoBox("Pour chaque unité du combo, ça ajoute la valeur suivante au multiplicateur")]
    public float MultiplierByComboUnit;

    [InfoBox("Pour chaque note pendant le spam en plus de l'objectif fixé")]
    public int SpamNoteBonus;
}