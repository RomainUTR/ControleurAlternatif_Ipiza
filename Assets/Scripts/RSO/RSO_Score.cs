using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "RSO_Score", menuName = "Data/RSO/RSO_Score")]
public class RSO_Score : ScriptableObject
{
    [Header("Initial Value")]
    public float InitialScoreValue;
    public int InitialComboValue;
    public float InitialMultiplierValue;

    [Header("Runtime Value")]
    [ReadOnly] public float RuntimeScore;
    [ReadOnly] public int RuntimeCombo;
    [ReadOnly] public float RuntimeMultiplier;

    public void ResetData()
    {
        RuntimeScore = InitialScoreValue;
        RuntimeCombo = InitialComboValue;
        RuntimeMultiplier = InitialMultiplierValue;
    }
}