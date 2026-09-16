using TMPro;
using UnityEngine;

public class SpamNote : NoteBehavior
{
    //[Header("Settings")]
    [Header("References")]
    [SerializeField] private SpriteRenderer SR;
    [SerializeField] private Transform TrailTransform;

    //[Header("Input")]
    //[Header("Output")]

    private int _requiredHits;
    private int _currentHits;

    public override void Initialize(Vector3 startPos, Vector3 targetPos, float spawnTime, float targetTime, float direction, RythmConductor conductor, float duration = 0, int requiredHits = 0)
    {
        base.Initialize(startPos, targetPos, spawnTime, targetTime, direction, conductor, duration, requiredHits);
        Duration = duration;
        _requiredHits = requiredHits;

        if (TrailTransform != null)
        {
            float distance = Vector3.Distance(startPos, targetPos);
            float timeToFall = targetTime - spawnTime;
            float fallSpeed = distance / timeToFall;
            float trailLength = fallSpeed * Duration;

            TrailTransform.localScale = new Vector3(trailLength, 1f, 1f);
        }
    }

    public override void EvaluateInput(KeyboardPlatterController platter, float tolerance, float currentTime)
    {
        if (CurrentState == NoteState.Hit || CurrentState == NoteState.Miss) return;

        switch (CurrentState)
        {
            case NoteState.Pending:
                if (currentTime >= TargetTime)
                {
                    CurrentState = NoteState.Ongoing;
                }
                break;

            case NoteState.Ongoing:
                if (currentTime > TargetTime + Duration)
                {
                    CurrentState = NoteState.Miss;
                    Debug.Log("You lose the spam note");
                    return;
                }

                if (Input.GetKeyDown(KeyCode.Backspace))
                {
                    _currentHits++;
                    Debug.Log(_currentHits);

                    SR.color = Color.Lerp(Color.white, Color.red, (float)_currentHits / _requiredHits);

                    if (_currentHits >= _requiredHits)
                    {
                        CurrentState = NoteState.Hit;
                        Debug.Log("You hit the spam note");
                    }
                }

                break;
        }
    }
}