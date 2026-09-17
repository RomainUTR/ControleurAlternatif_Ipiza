using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "RSO_Score", menuName = "Data/RSO/RSO_Score")]
public class RSO_Score : ScriptableObject
{
    [Header("Runtime Value")]
    public float InitialScoreValue;
    
    [ReadOnly]
    public float RuntimeScore;

    private void OnEnable()
    {
        RuntimeScore = InitialScoreValue;
    }
}