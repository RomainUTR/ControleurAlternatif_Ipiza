using UnityEngine;

public class NoteBehavior : MonoBehaviour
{
    //[Header("Settings")]

    [Header("References")]
    [SerializeField] private SpriteRenderer SR;
    [SerializeField] private Sprite SpriteUp;
    [SerializeField] private Sprite SpriteDown;

    //[Header("Input")]
    //[Header("Output")]

    public float TargetTime {  get; private set; }
    public float Direction {  get; private set; }

    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private float _spawnTime;
    private RythmConductor _conductor;

    public void Initialize(Vector3 startPos, Vector3 targetPos, float spawnTime, float targetTime, float direction, RythmConductor conductor)
    {
        _startPosition = startPos;
        _targetPosition = targetPos;
        _spawnTime = spawnTime;
        TargetTime = targetTime;
        _conductor = conductor;

        Direction = direction;
        SR.sprite = (Direction > 0f) ? SpriteUp : SpriteDown;
    }

    private void Update()
    {
        if (TargetTime == 0f) return;

        float currentTime = _conductor.CurrentTrackTime;
        float progression = (currentTime - _spawnTime) / (TargetTime - _spawnTime);

        transform.position = Vector3.LerpUnclamped(_startPosition, _targetPosition, progression);

        if (progression > 1.5f)
        {
            Destroy(gameObject);
        }
    }
}