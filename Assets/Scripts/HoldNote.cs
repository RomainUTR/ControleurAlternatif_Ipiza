using UnityEngine;

public class HoldNote : NoteBehavior
{
    [Header("Settings")]
    [SerializeField] private float MaxGraceTime = 0.2f;
    private float _graceTimer = 0f;

    [Header("References")]
    [SerializeField] private SpriteRenderer SR;

    //[Header("Input")]
    //[Header("Output")]

    public override void Initialize(Vector3 startPos, Vector3 targetPos, float spawnTime, float targetTime, float direction, RythmConductor conductor, float duration = 0f)
    {
        base.Initialize(startPos, targetPos, spawnTime, targetTime, direction, conductor, duration);
        Duration = duration;
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
}