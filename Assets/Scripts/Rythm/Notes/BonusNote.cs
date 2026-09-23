using RomainUTR.SLToolbox;
using UnityEngine;

public class BonusNote : NoteBehavior
{
    [Header("References")]
    [SerializeField] private SpriteRenderer SR;
    [SerializeField] private Sprite[] SpriteList;

    public override void Initialize(Vector3 startPos, Vector3 targetPos, float spawnTime, float targetTime, float direction, RythmConductor conductor, float duration, int requiredHits = 0)
    {
        base.Initialize(startPos, targetPos, spawnTime, targetTime, direction, conductor);

        SR.sprite = SpriteList.GetRandom();
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
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CurrentState = NoteState.Hit;
            }
        }
    }

    public override void TryToScoring()
    {
        RequestScoring.Raise(ScoreData.BonusNote);
    }
}