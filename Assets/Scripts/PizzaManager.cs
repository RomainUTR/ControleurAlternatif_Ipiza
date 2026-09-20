using TMPro;
using UnityEngine;

public class PizzaManager : MonoBehaviour
{
    //[Header("Settings")]
    [Header("References")]
    [SerializeField] private TMP_Text TurnText;

    [Header("Input")]
    [SerializeField] private RSE_OnTurnCompleted OnTurnCompleted;

    //[Header("Output")]

    private int _turnCount = 0;

    private void OnEnable()
    {
        OnTurnCompleted.OnEventRaised += HandleTurnCompletion;
    }

    private void OnDisable()
    {
        OnTurnCompleted.OnEventRaised -= HandleTurnCompletion;
    }

    private void HandleTurnCompletion(int amount)
    {
        _turnCount += amount;
        TurnText.text = _turnCount.ToString();
    }
}