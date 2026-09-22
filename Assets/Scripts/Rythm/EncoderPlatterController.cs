using Sirenix.OdinInspector;
using UnityEngine;

public class EncoderPlatterController : MonoBehaviour, IPlatterInput
{
    [Header("Settings")]
    [SerializeField] private float Acceleration = 3f;
    [SerializeField] private float Friction = 2f;
    [SerializeField] private float MaxSpeed = 1f;

    [Header("Hardware Settings")]
    [InfoBox("Vitesse (degrés/sec). Si la tige est très sensible, monte cette valeur (ex: 30)")]
    [SerializeField] private float SpeedThreshold = 20f;

    [Header("Network Smoothing")]
    [Tooltip("Tolérance en secondes pour lisser les micro-coupures réseau de l'Arduino")]
    [SerializeField] private float InputBufferTime = 0.1f;

    [Header("Debug")]
    [ReadOnly, ShowInInspector] public float CurrentSpeed { get; private set; } = 0f;
    [ReadOnly] public float TrueContinuousAngle = 0f;

    public bool IsConsumed { get; private set; }
    public float CurrentInput { get; private set; }

    private float _lastInput = 0f;
    private float _previousFrameAngle = 0f;
    private float _timeSinceLastMovement = 0f;

    private float _rawAngle = 0f;
    private int _rawTurns = 0;

    public void HandleEncoderAngle(float angle)
    {
        _rawAngle = angle;
    }

    public void HandleEncoderTurns(int turns)
    {
        _rawTurns = turns;
    }

    private void Start()
    {
        TrueContinuousAngle = (_rawTurns * 360f) + _rawAngle;
        _previousFrameAngle = TrueContinuousAngle;
    }

    private void Update()
    {
        TrueContinuousAngle = (_rawTurns * 360f) + _rawAngle;

        float deltaAngle = TrueContinuousAngle - _previousFrameAngle;
        _previousFrameAngle = TrueContinuousAngle;

        float physicalVelocity = deltaAngle / Time.deltaTime;

        if (Mathf.Abs(physicalVelocity) > SpeedThreshold)
        {
            CurrentInput = Mathf.Sign(physicalVelocity);
            _timeSinceLastMovement = 0f;
        } else
        {
            _timeSinceLastMovement += Time.deltaTime;
            if (_timeSinceLastMovement >= InputBufferTime)
            {
                CurrentInput = 0f;
            }
        }

        if (CurrentInput != _lastInput)
        {
            IsConsumed = false;

            if (CurrentInput != 0f && Mathf.Sign(CurrentInput) != Mathf.Sign(CurrentSpeed))
            {
                CurrentSpeed = 0f;
            } 
        }

        _lastInput = CurrentInput;

        if (CurrentInput != 0f)
        {
            CurrentSpeed += CurrentInput * Acceleration * Time.deltaTime;
        } else
        {
            CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, 0f, Friction * Time.deltaTime);
        }

        CurrentSpeed = Mathf.Clamp(CurrentSpeed, -MaxSpeed, MaxSpeed);
    }

    public void ConsumeInput()
    {
        IsConsumed = true;
    }
}
