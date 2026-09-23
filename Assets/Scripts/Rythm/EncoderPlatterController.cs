using Sirenix.OdinInspector;
using UnityEngine;

public class EncoderPlatterController : MonoBehaviour, IPlatterInput
{
    [Header("Settings")]
    [SerializeField] private float Acceleration = 3f;
    [SerializeField] private float Friction = 2f;
    [SerializeField] private float MaxSpeed = 1f;

    [Header("Hardware Settings")]
    [InfoBox("Vitesse pour déclencher")]
    [SerializeField] private float StartSpeedThreshold = 100f;
    [InfoBox("Vitesse pour maintenir")]
    [SerializeField] private float MaintainSpeedThreshold = 15f;
    [InfoBox("Filtre spatial. Ignore les micro-tremblements du doigt sur l'encodeur")]
    [SerializeField] private float DistanceFilter = 0.5f;
    [InfoBox("Debounce (sec) : Temps nécessaire pour valider un changement de direction.")]
    [SerializeField] private float DebounceTime = 0.05f;

    private float _directionDebounceTimer = 0f;
    private float _pendingDirection = 0f;

    [Header("Network Smoothing")]
    [Tooltip("Tolérance (sec) avant de couper l'input si l'Arduino ne renvoie plus de mouvement")]
    [SerializeField] private float InputBufferTime = 0.1f;

    [Header("Debug")]
    [ReadOnly, ShowInInspector] public float CurrentSpeed { get; private set; } = 0f;
    [ReadOnly] public float TrueContinuousAngle = 0f;
    [SerializeField] private Transform PlatineTransform;

    public bool IsConsumed { get; private set; }
    public float CurrentInput { get; private set; }

    private float _lastInput = 0f;
    private float _lastPacketTime = 0f;
    private float _timeSinceLastMovement = 0f;
    private float _currentAngle = 0f;
    private float _visualContinuousAngle = 0f;

    public void HandleEncoderVector(Vector3 hardwareData)
    {
        float rawAngle = hardwareData.x;
        int rawTurns = Mathf.RoundToInt(hardwareData.y);
        float hardwareDirection = hardwareData.z;

        float newTotalDistance = (rawTurns * 360f) + rawAngle;
        float distanceMoved = Mathf.Abs(newTotalDistance - TrueContinuousAngle);

        if (distanceMoved < DistanceFilter) return;

        float currentTime = Time.realtimeSinceStartup;
        float timeSinceLastPacket = currentTime - _lastPacketTime;

        if (timeSinceLastPacket > 0.001f)
        {
            float physicalVelocity = (distanceMoved * hardwareDirection) / timeSinceLastPacket;

            float activeThreshold = (CurrentInput != 0f) ? MaintainSpeedThreshold : StartSpeedThreshold;

            if (Mathf.Abs(physicalVelocity) > activeThreshold)
            {
                float newRawDirection = Mathf.Sign(physicalVelocity);

                if (newRawDirection != CurrentInput)
                {
                    if (newRawDirection == _pendingDirection)
                    {
                        _directionDebounceTimer += timeSinceLastPacket;

                        if (_directionDebounceTimer >= DebounceTime)
                        {
                            CurrentInput = newRawDirection;
                            _timeSinceLastMovement = 0f;
                        }
                    }
                    else
                    {
                        _pendingDirection = newRawDirection;
                        _directionDebounceTimer = timeSinceLastPacket;
                    }
                }
                else
                {
                    _directionDebounceTimer = 0f;
                    _timeSinceLastMovement = 0f;
                }
            }
        }

        _visualContinuousAngle += distanceMoved * hardwareDirection;

        if (PlatineTransform != null)
        {
            PlatineTransform.rotation = Quaternion.Euler(0f, 0f, -_visualContinuousAngle);
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
