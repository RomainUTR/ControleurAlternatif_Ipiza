using TMPro;
using UnityEngine;

public class PizzaManager : MonoBehaviour
{
    public enum PizzaState
    {
        DoughFlattening,
        IngredientAssembly,
        Cooking,
        Ready
    }

    [Header("Settings")]
    [SerializeField] private int RequiredDoughTurns = 3;
    [SerializeField] private int RequiredTurnDirection = 1;
    public PizzaState CurrentState = PizzaState.DoughFlattening;

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
        switch (CurrentState)
        {
            case PizzaState.DoughFlattening:
                ProcessDoughFlattening(amount);
                break;
        }
    }

    void ProcessDoughFlattening(int turnAmount)
    {
        if (Mathf.Sign(turnAmount) == Mathf.Sign(RequiredTurnDirection))
        {
            _turnCount += Mathf.Abs(turnAmount);
            TurnText.text = $"{_turnCount} / {RequiredDoughTurns}";

            if (_turnCount >= RequiredDoughTurns)
            {
                Debug.Log("Pâte étalée !");
                CurrentState = PizzaState.IngredientAssembly;
                TurnText.text = "Pâte prête";
            }
        }
        else
        {
            Debug.Log("Mauvais sens de rotation");
        }
    }
}