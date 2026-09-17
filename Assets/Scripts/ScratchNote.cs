using UnityEngine;

public class ScratchNote : NoteBehavior
{
    [Header("References")]
    [SerializeField] private SpriteRenderer SR;
    [SerializeField] private Sprite SpriteUp;
    [SerializeField] private Sprite SpriteDown;

    public override void Initialize(Vector3 startPos, Vector3 targetPos, float spawnTime, float targetTime, float direction, RythmConductor conductor, float duration, int requiredHits = 0)
    {
        base.Initialize(startPos, targetPos, spawnTime, targetTime, direction, conductor);

        SR.sprite = (Direction > 0f) ? SpriteUp : SpriteDown;
    }

    public override void EvaluateInput(KeyboardPlatterController platter, float tolerance, float currentTime)
    {
        if (CurrentState == NoteState.Hit || CurrentState == NoteState.Miss) return;

        float timeDifference = currentTime - TargetTime;

        if (timeDifference > tolerance)
        {
            CurrentState = NoteState.Miss;
            return;
        }

        if (Mathf.Abs(timeDifference) <= tolerance)
        {
            if (!platter.IsConsumed && Mathf.Abs(platter.CurrentSpeed) > 0.1f && Mathf.Sign(platter.CurrentSpeed) == Mathf.Sign(Direction))
            {
                CurrentState = NoteState.Hit;
            }
        }
    }

    public override void TryToScoring()
    {
        RequestScoring.Raise(ScoreData.ScratchNote);
        RefreshUI.Raise();
    }
}