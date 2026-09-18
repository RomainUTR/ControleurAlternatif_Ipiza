using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    //[Header("Settings")]
    [Header("References")]
    [SerializeField] private TMP_Text ScoreText;
    [SerializeField] private TMP_Text ComboText;

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
        ScoreText.text = FormatNumber(Score.RuntimeScore);
        ComboText.text = $"x {Score.RuntimeCombo}";
    }

    private string FormatNumber(double num)
    {
        if (num >= 1000000000)
            return (num / 1000000000).ToString("F1") + "B";

        if (num >= 1000000)
            return (num / 1000000).ToString("F1") + "M";

        if (num >= 1000)
            return (num / 1000).ToString("F1") + "K";

        return num.ToString("F0");
    }
}