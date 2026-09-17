using UnityEngine;

public class HoldNote : NoteBehavior
{
    [Header("Settings")]
    [SerializeField] private float MaxGraceTime = 0.2f;
    private float _graceTimer = 0f;

    [Header("References")]
    [SerializeField] private SpriteRenderer SR;
    [SerializeField] private Transform TrailTransform;
    [SerializeField] private Sprite SpriteUp, SpriteDown;

    //[Header("Input")]
    //[Header("Output")]

    private float _fallSpeed;
    private SpriteRenderer _trailSR;

    public override void Initialize(Vector3 startPos, Vector3 targetPos, float spawnTime, float targetTime, float direction, RythmConductor conductor, float duration = 0f, int requiredHits = 0)
    {
        base.Initialize(startPos, targetPos, spawnTime, targetTime, direction, conductor, duration);
        Duration = duration;

        SR.sprite = (Direction > 0f) ? SpriteUp : SpriteDown;

        if (TrailTransform != null)
        {
            float distance = Vector3.Distance(startPos, targetPos);
            float timeToFall = targetTime - spawnTime;
            
            _fallSpeed = distance / timeToFall;

            float trailLength = _fallSpeed * Duration;

            TrailTransform.localScale = new Vector3(trailLength, 0.2f, 1f);

            _trailSR = TrailTransform.gameObject.GetComponent<SpriteRenderer>();
        }
    }

    public override void EvaluateInput(KeyboardPlatterController platter, float tolerance, float currentTime)
    {
        if (CurrentState == NoteState.Hit || CurrentState == NoteState.Miss) return;

        float timeDifference = currentTime - TargetTime;

        switch (CurrentState)
        {
            case NoteState.Pending:
                if (timeDifference > tolerance)
                {
                    CurrentState = NoteState.Miss;
                    return;
                }

                if (Mathf.Abs(timeDifference) <= tolerance)
                {
                    if (Mathf.Abs(platter.CurrentSpeed) > 0.1f && Mathf.Sign(platter.CurrentSpeed) == Mathf.Sign(Direction))
                    {
                        CurrentState = NoteState.Ongoing;
                    }
                }
                break;

            case NoteState.Ongoing:
                SR.color = Color.yellow;

                if (TrailTransform != null)
                {
                    float remainingTime = (TargetTime + Duration) - currentTime;
                    float currentTrailLength = _fallSpeed * Mathf.Max(0, remainingTime);

                    TrailTransform.localScale = new Vector3(currentTrailLength, 0.2f, 1f);
                }

                if (currentTime >= TargetTime + Duration)
                {
                    CurrentState = NoteState.Hit;
                    return;
                }

                if (Mathf.Abs(platter.CurrentSpeed) > 0.1f && Mathf.Sign(platter.CurrentSpeed) == Mathf.Sign(Direction))
                {
                    _graceTimer = 0f;
                }
                else
                {
                    _graceTimer += Time.deltaTime;

                    if (_graceTimer > MaxGraceTime)
                    {
                        SR.color = Color.red;
                        CurrentState = NoteState.Miss;
                    }
                }
                break;
        }
    }

    public override void TriggerMissFeedback(float currentTime)
    {
        SR.color = new Color(1f, 0f, 0f, 0.5f);
        _trailSR.color = new Color(1f, 0f, 0f, 0.5f);

        float remainingTime = (TargetTime + Duration) - currentTime;

        Destroy(gameObject, Mathf.Max(0f, remainingTime));
    }
}