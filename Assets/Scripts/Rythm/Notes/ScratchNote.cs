using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ScratchNote : NoteBehavior
{
    [Header("References")]
    [SerializeField] private SpriteRenderer SR;
    [SerializeField] private GameObject VisualUp;
    [SerializeField] private GameObject VisualDown;

    public override void Initialize(Vector3 startPos, Vector3 targetPos, float spawnTime, float targetTime, float direction, RythmConductor conductor, float duration, int requiredHits = 0)
    {
        base.Initialize(startPos, targetPos, spawnTime, targetTime, direction, conductor);

        bool isDown = Direction > 0f;

        if (VisualDown != null) VisualDown.SetActive(isDown);
        if (VisualUp != null) VisualUp.SetActive(!isDown);
    }

    public override void EvaluateInput(IPlatterInput platter, float tolerance, float currentTime)
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
            if (!platter.IsConsumed && Mathf.Abs(platter.CurrentSpeed) > 0.1f && platter.CurrentInput != 0f && Mathf.Sign(platter.CurrentInput) == Mathf.Sign(Direction))
            {
                CurrentState = NoteState.Hit;
            }
        }
    }

    public override void TryToScoring()
    {
        RequestScoring.Raise(ScoreData.ScratchNote);
    }
}