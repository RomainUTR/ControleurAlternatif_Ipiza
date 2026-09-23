using System.Buffers;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpamNote : NoteBehavior
{
    //[Header("Settings")]
    [Header("References")]
    [SerializeField] private SpriteRenderer SR;
    [SerializeField] private Transform TrailTransform;
    [SerializeField] private TMP_Text CounterText;

    [Header("Input")]
    [SerializeField] private InputActionReference SpamInput;
    //[Header("Output")]

    private int _requiredHits;
    private int _currentHits;
    private float _fallSpeed;
    private int _extraNoteCount;

    private SpriteRenderer _trailSR;

    public override void Initialize(Vector3 startPos, Vector3 targetPos, float spawnTime, float targetTime, float direction, RythmConductor conductor, float duration = 0, int requiredHits = 0)
    {
        base.Initialize(startPos, targetPos, spawnTime, targetTime, direction, conductor, duration, requiredHits);
        Duration = duration;
        _requiredHits = requiredHits;

        if (TrailTransform != null)
        {
            float distance = Vector3.Distance(startPos, targetPos);
            float timeToFall = targetTime - spawnTime;

            _fallSpeed = distance / timeToFall;

            float trailLength = _fallSpeed * Duration;

            TrailTransform.localScale = new Vector3(trailLength, 0.2f, 1f);
            _trailSR = TrailTransform.gameObject.GetComponent<SpriteRenderer>();
        }

        CounterText.text = _requiredHits.ToString();
    }

    public override void EvaluateInput(IPlatterInput platter, float tolerance, float currentTime)
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
                if (TrailTransform != null)
                {
                    float remainingTime = (TargetTime + Duration) - currentTime;
                    float currentTrailLength = _fallSpeed * Mathf.Max(0, remainingTime);

                    TrailTransform.localScale = new Vector3(currentTrailLength, 0.2f, 1f);
                }

                if (SpamInput != null && SpamInput.action.WasPressedThisFrame())
                {
                    _currentHits++;
                    CounterText.text = Mathf.Max(0, _requiredHits - _currentHits).ToString();

                    if (_currentHits < _requiredHits)
                    {
                        SR.color = Color.Lerp(Color.white, Color.red, (float)_currentHits / _requiredHits);
                        _trailSR.color = Color.Lerp(Color.white, Color.red, (float)_currentHits / _requiredHits);
                    }
                    else
                    {
                        _extraNoteCount++;
                        SR.color = Color.yellow;
                        _trailSR.color = Color.yellow;
                    }
                }

                if (currentTime > TargetTime + Duration)
                {
                    if (_currentHits >= _requiredHits)
                    {
                        CurrentState = NoteState.Hit;
                        RequestScoring.Raise(_extraNoteCount * ScoreData.SpamNoteBonus);
                        _extraNoteCount = 0;
                    }
                    else
                    {
                        CurrentState = NoteState.Miss;
                        _extraNoteCount = 0;
                    }
                }
                break;
        }
    }

    public override void TryToScoring()
    {
        RequestScoring.Raise(ScoreData.SpamNote);
    }
}