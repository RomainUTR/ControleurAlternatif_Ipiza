using Sirenix.OdinInspector;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    //[Header("Settings")]

    //[Header("References")]

    [Header("Input")]
    [SerializeField] private RSE_RequestScoring RequestScoring;

    [Header("Output")]
    [SerializeField, InlineEditor] private RSO_Score Score;

    private void OnEnable()
    {
        if (RequestScoring != null)
        {
            RequestScoring.OnEventRaised += AddScore;
        }
    }

    private void OnDisable()
    {
        if (RequestScoring != null)
        {
            RequestScoring.OnEventRaised -= AddScore;
        }
    }

    private void AddScore(int amount)
    {
        Debug.Log(Score.RuntimeMultiplier);
        Debug.Log(amount * Score.RuntimeMultiplier);
        Score.RuntimeScore += (amount * Score.RuntimeMultiplier);
    }
}