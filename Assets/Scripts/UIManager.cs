using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    //[Header("Settings")]
    [Header("References")]
    [SerializeField] private TMP_Text ScoreText;

    [Header("Input")]
    [SerializeField] private RSE_RefreshUI RefreshUIEvent;
    [SerializeField] private RSO_Score Score;

    //[Header("Output")]

    private void OnEnable()
    {
        if (RefreshUIEvent != null) RefreshUIEvent.OnEventRaised += RefreshUI;
    }

    private void OnDisable()
    {
        if (RefreshUIEvent != null) RefreshUIEvent.OnEventRaised -= RefreshUI;
    }

    private void RefreshUI()
    {
        ScoreText.text = Score.RuntimeScore.ToString();
    }
}