using Sirenix.OdinInspector;
using UnityEngine;

public class HoldNote : NoteBehavior
{
    [Header("Settings")]
    [SerializeField] private float MaxGraceTime = 0.2f;
    private float _graceTimer = 0f;

    [Header("References")]
    [SerializeField] private SpriteRenderer SR;
    [SerializeField] private Transform TrailTransform;
    [SerializeField] private GameObject VisualUp, VisualDown;

    [Header("Debug En Temps Réel")]
    [ShowInInspector, ReadOnly] private float DebugCurrentSpeed = 0f;
    [ShowInInspector, ReadOnly] private string DebugHoldStatus = "En attente...";
    [ShowInInspector, ReadOnly, ProgressBar(0, "MaxGraceTime", ColorGetter = "GetGraceColor")]
    private float DebugGraceProgressBar => _graceTimer;

    private Color GetGraceColor() => Color.Lerp(Color.green, Color.red, _graceTimer / MaxGraceTime);

    //[Header("Input")]
    //[Header("Output")]

    private float _fallSpeed;
    private SpriteRenderer _trailSR;

    public override void Initialize(Vector3 startPos, Vector3 targetPos, float spawnTime, float targetTime, float direction, RythmConductor conductor, float duration = 0f, int requiredHits = 0)
    {
        base.Initialize(startPos, targetPos, spawnTime, targetTime, direction, conductor, duration);
        Duration = duration;

        bool isDown = Direction > 0f;

        if (VisualDown != null) VisualDown.SetActive(isDown);
        if (VisualUp != null) VisualUp.SetActive(!isDown);

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

    public override void EvaluateInput(IPlatterInput platter, float tolerance, float currentTime)
    {
        if (CurrentState == NoteState.Hit || CurrentState == NoteState.Miss) return;

        float timeDifference = currentTime - TargetTime;

        switch (CurrentState)
        {
            case NoteState.Pending:
                if (timeDifference > tolerance)
                {
                    Debug.LogWarning($"[MISS - DÉBUT] Raté à l'entrée ! Vitesse Platine: {platter.CurrentSpeed:F2} | Dir Joueur: {Mathf.Sign(platter.CurrentSpeed)} | Dir Requise: {Mathf.Sign(Direction)}");

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
                DebugCurrentSpeed = platter.CurrentSpeed;

                if (TrailTransform != null)
                {
                    float remainingTime = (TargetTime + Duration) - currentTime;
                    float currentTrailLength = _fallSpeed * Mathf.Max(0, remainingTime);
                    TrailTransform.localScale = new Vector3(currentTrailLength, 0.2f, 1f);
                }

                if (currentTime >= TargetTime + Duration)
                {
                    DebugHoldStatus = "Validé !";
                    CurrentState = NoteState.Hit;
                    return;
                }

                bool isFastEnough = Mathf.Abs(platter.CurrentSpeed) > 0.1f;
                bool isRightDirection = Mathf.Sign(platter.CurrentSpeed) == Mathf.Sign(Direction);

                if (isFastEnough && isRightDirection)
                {
                    DebugHoldStatus = "Parfait";
                    _graceTimer = 0f;
                }
                else
                {
                    if (!isFastEnough) DebugHoldStatus = "TROP LENT ! (Vitesse < 0.1)";
                    else if (!isRightDirection) DebugHoldStatus = "MAUVAISE DIRECTION !";

                    _graceTimer += Time.deltaTime;

                    if (_graceTimer > MaxGraceTime)
                    {
                        Debug.LogWarning($"[HOLD RATÉ] Vitesse actuelle: {platter.CurrentSpeed:F2} | Trop lent: {!isFastEnough} | Mauvaise Direction: {!isRightDirection}");

                        DebugHoldStatus = "LÂCHÉ";
                        CurrentState = NoteState.Miss;
                    }
                }
                break;
        }
    }

    public override void TriggerMissFeedback(float currentTime)
    {
        _trailSR.color = new Color(1f, 0f, 0f, 0.5f);

        float remainingTime = (TargetTime + Duration) - currentTime;

        this.enabled = false;

        Destroy(gameObject, Mathf.Max(0f, remainingTime));
    }

    public override void TryToScoring()
    {
        RequestScoring.Raise(ScoreData.HoldNote);
    }
}