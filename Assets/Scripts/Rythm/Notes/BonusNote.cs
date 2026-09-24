using RomainUTR.SLToolbox;
using UnityEngine;

public class BonusNote : NoteBehavior
{
    [System.Serializable]
    public struct BonusMapping
    {
        public Sprite BonusSprite;
        public SSO_Ingredient Ingredient;
    }

    [Header("References")]
    [SerializeField] private SpriteRenderer SR;
    [SerializeField] private BonusMapping[] Mappings;
    [SerializeField] private SSO_InputReader InputReader;

    private SSO_Ingredient _myAssignedIngredient;
    private bool _isInHitWindow = false;

    private void OnEnable()
    {
        InputReader.OnIngredientPressedEvent += HandleIngredientInput;
    }

    private void OnDisable()
    {
        InputReader.OnIngredientPressedEvent -= HandleIngredientInput;
    }

    public override void Initialize(Vector3 startPos, Vector3 targetPos, float spawnTime, float targetTime, float direction, RythmConductor conductor, float duration = 0f, int requiredHits = 0)
    {
        base.Initialize(startPos, targetPos, spawnTime, targetTime, direction, conductor, duration, requiredHits);

        BonusMapping currentMapping = Mappings.GetRandom();
        SR.sprite = currentMapping.BonusSprite;

        _myAssignedIngredient = currentMapping.Ingredient;
    }

    public override void EvaluateInput(IPlatterInput platter, float tolerance, float currentTime)
    {
        if (CurrentState == NoteState.Hit || CurrentState == NoteState.Miss) return;

        float timeDifference = currentTime - TargetTime;

        if (timeDifference > tolerance)
        {
            CurrentState = NoteState.Miss;
            _isInHitWindow = false;
            return;
        }

        _isInHitWindow = Mathf.Abs(timeDifference) <= tolerance;
    }

    public override void TryToScoring()
    {
        RequestScoring.Raise(ScoreData.BonusNote);
    }

    private void HandleIngredientInput(SSO_Ingredient pressedIngredient)
    {
        if (CurrentState == NoteState.Hit || CurrentState == NoteState.Miss) return;

        if (pressedIngredient == _myAssignedIngredient && _isInHitWindow)
        {
            CurrentState = NoteState.Hit;
            Debug.Log($"Bonus {pressedIngredient.name} validé !");
        }
    }
}