using RomainUTR.SLToolbox;
using UnityEngine;

public class BonusNote : NoteBehavior
{
    [Header("References")]
    [SerializeField] private SpriteRenderer SR;
    [SerializeField] private Sprite[] SpriteList;

    public override void Initialize(Vector3 startPos, Vector3 targetPos, float spawnTime, float targetTime, float direction, RythmConductor conductor)
    {
        base.Initialize(startPos, targetPos, spawnTime, targetTime, direction, conductor);

        SR.sprite = SpriteList.GetRandom();
    }

    public override bool EvaluateInput(KeyboardPlatterController platter, float tolerance)
    {
        return Input.GetKeyDown(KeyCode.Space);
    }
}