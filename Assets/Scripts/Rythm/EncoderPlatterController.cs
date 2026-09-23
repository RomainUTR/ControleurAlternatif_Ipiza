using Sirenix.OdinInspector;
using UnityEngine;

public class EncoderPlatterController : MonoBehaviour, IPlatterInput
{
    [Header("Settings")]
    [SerializeField] private float Acceleration = 3f;
    [SerializeField] private float Friction = 2f;
    [SerializeField] private float MaxSpeed = 1f;

    [Header("Hardware Settings")]
    [InfoBox("Vitesse minimale (degrés/sec) pour activer l'input.")]
    [SerializeField] private float SpeedThreshold = 20f;

    [Header("Network Smoothing")]
    [Tooltip("Tolérance (sec) avant de couper l'input si l'Arduino ne renvoie plus de mouvement")]
    [SerializeField] private float InputBufferTime = 0.1f;

    [Header("Debug")]
    [ReadOnly, ShowInInspector] public float CurrentSpeed { get; private set; } = 0f;
    [ReadOnly] public float TrueContinuousAngle = 0f;

    public bool IsConsumed { get; private set; }
    public float CurrentInput { get; private set; }

    private float _lastInput = 0f;
    private float _lastPacketTime = 0f;
    private float _timeSinceLastMovement = 0f;

    public void HandleEncoderVector(Vector3 hardwareData)
    {
        float rawAngle = hardwareData.x;
        int rawTurns = Mathf.RoundToInt(hardwareData.y);
        float hardwareDirection = hardwareData.z;

        float newTotalDistance = (rawTurns * 360f) + rawAngle;

        float currentTime = Time.realtimeSinceStartup;
        float timeSinceLastPacket = currentTime - _lastPacketTime;

        if (timeSinceLastPacket > 0.001f)
        {
            float distanceMoved = Mathf.Abs(newTotalDistance - TrueContinuousAngle);

            if (distanceMoved > 1f)
            {
                float signedDeltaAngle = distanceMoved * hardwareDirection;
                float physicalVelocity = signedDeltaAngle / timeSinceLastPacket;

                if (Mathf.Abs(physicalVelocity) > SpeedThreshold)
                {
                    CurrentInput = Mathf.Sign(physicalVelocity);
                    _timeSinceLastMovement = 0f;
                }
            }
        }

        TrueContinuousAngle = newTotalDistance;
        _lastPacketTime = currentTime;
    }

    private void Start()
    {
        _lastPacketTime = Time.realtimeSinceStartup;
    }

    private void Update()
    {
        _timeSinceLastMovement += Time.deltaTime;

        if (_timeSinceLastMovement >= InputBufferTime)
        {
            CurrentInput = 0f;
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
        }
        else
        {
            CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, 0f, Friction * Time.deltaTime);
        }

        CurrentSpeed = Mathf.Clamp(CurrentSpeed, -MaxSpeed, MaxSpeed);
    }

    public void ConsumeInput()
    {
        IsConsumed = true;
    }

    [ShowInInspector, ReadOnly, DisplayAsString]
    public string IntendedDirection
    {
        get
        {
            if (CurrentInput > 0f) return "Horaire (Down)";
            if (CurrentInput < 0f) return "Anti-horaire (Up)";
            return "Repos";
        }
    }
}
