using System;
using UnityEngine;

public enum NoteState
{
    Pending,
    Ongoing,
    Hit,
    Miss
}

public abstract class NoteBehavior : MonoBehaviour
{
    //[Header("Settings")]
    //[Header("Input")]
    //[Header("Output")]

    public RSE_RequestScoring RequestScoring;
    public SSO_ScoreData ScoreData;

    public float TargetTime {  get; private set; }
    public float Direction {  get; private set; }
    public NoteState CurrentState { get; protected set; } = NoteState.Pending;

    public NoteType CurrentType;
    public float Duration { get; protected set; }

    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private float _spawnTime;
    private RythmConductor _conductor;

    public virtual void Initialize(Vector3 startPos, Vector3 targetPos, float spawnTime, float targetTime, float direction, RythmConductor conductor, float duration = 0f, int requiredHits = 0)
    {
        _startPosition = startPos;
        _targetPosition = targetPos;
        _spawnTime = spawnTime;
        TargetTime = targetTime;
        _conductor = conductor;

        Direction = direction;
    }

    private void Update()
    {
        if (TargetTime == 0f) return;

        float currentTime = _conductor.CurrentTrackTime;
        float progression = (currentTime - _spawnTime) / (TargetTime - _spawnTime);

        if (CurrentState == NoteState.Ongoing)
        {
            progression = 1f;
        }

        transform.position = Vector3.LerpUnclamped(_startPosition, _targetPosition, progression);
    }

    public abstract void EvaluateInput(KeyboardPlatterController platter, float tolerance, float currentTime);

    public virtual void TriggerMissFeedback(float currentTime)
    {
        Destroy(gameObject);
    }

    public abstract void TryToScoring();
}