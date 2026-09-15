using UnityEngine;

public class ScratchNote : NoteBehavior
{
    [Header("References")]
    [SerializeField] private SpriteRenderer SR;
    [SerializeField] private Sprite SpriteUp;
    [SerializeField] private Sprite SpriteDown;


    public override void Initialize(Vector3 startPos, Vector3 targetPos, float spawnTime, float targetTime, float direction, RythmConductor conductor)
    {
        base.Initialize(startPos, targetPos, spawnTime, targetTime, direction, conductor);

        SR.sprite = (Direction > 0f) ? SpriteUp : SpriteDown;
    }

    public override bool EvaluateInput(KeyboardPlatterController platter, float tolerance)
    {
        if (Mathf.Abs(platter.CurrentSpeed) > 0.1f && Mathf.Sign(platter.CurrentSpeed) == Mathf.Sign(Direction))
        {
            float speedDifference = Mathf.Abs(1f - Mathf.Abs(platter.CurrentSpeed));
            return speedDifference <= tolerance;
        }
        return false;
    }
}