using Sirenix.OdinInspector;
using UnityEngine;

public class OSCEncoderTurn : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float OffsetAngle;

    [Header("References")]
    [SerializeField] private Transform PizzaTransform;
    [SerializeField] private RSE_OnTurnCompleted OnTurnCompleted;

    [ReadOnly] public float _currentAngle = 0f;
    public float _previousAngle = 0f;
    public float _accumulatedDegrees = 0f;
    public int TotalTurns = 0;
    private int _previousTotalTurns;
    private bool _isInitialized = false;

    public void HandleOSCEncoderAngle(float value)
    {
        _currentAngle = value;

        if (PizzaTransform != null)
        {
            PizzaTransform.rotation = Quaternion.Euler(0f, 0f, -_currentAngle + OffsetAngle);
        }
    }

    public void HandleOSCEncoderTurns(int amount)
    {
        TotalTurns = amount;

        if (!_isInitialized)
        {
            _previousTotalTurns = TotalTurns;
            _isInitialized = true;
            return;
        }

        if (TotalTurns != _previousTotalTurns)
        {
            int turnDelta = TotalTurns - _previousTotalTurns;
            _previousTotalTurns = TotalTurns;
            TriggerTurn(turnDelta);
        }
    }

    private void TriggerTurn(int turns)
    {
        Debug.Log($"[Encoder] Le joueur a fait {turns} tour(s) avec l'encodeur !");
        OnTurnCompleted?.Raise(turns);
    }
}
