using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
public struct IngredientMapping
{
    public InputActionReference InputAction;
    public SSO_Ingredient Ingredient;
}

public class PizzaManager : MonoBehaviour
{
    public enum PizzaState
    {
        DoughFlattening,
        IngredientAssembly,
        Cooking,
        Ready
    }

    [Header("State Machine")]
    public PizzaState CurrentState = PizzaState.DoughFlattening;

    [Header("Dough Settings")]
    [SerializeField] private int RequiredDoughTurns = 3;
    [SerializeField] private int RequiredTurnDirection = 1;

    [Header("Ingredients Settings")]
    [SerializeField] private List<SSO_Ingredient> CurrentRecipe;
    [SerializeField] private int TurnsToPlaceIngredient = 2;

    [Header("Input Mapping")]
    [Tooltip("Associe chaque action d'input (bouton) à son ingrédient correspondant.")]
    [SerializeField] private List<IngredientMapping> InputMapping = new List<IngredientMapping>();
    [SerializeField] private RSE_OnTurnCompleted OnTurnCompleted;

    [Header("Scene References")]
    [SerializeField] private Transform PizzaTransform;
    [SerializeField] private TMP_Text TurnText;

    [Header("UI References")]
    [SerializeField] private Transform OrderContainer;
    [SerializeField] private GameObject OrderIconPrefab;

    private Dictionary<InputAction, SSO_Ingredient> _runtimeActionMap = new Dictionary<InputAction, SSO_Ingredient>();
    private List<Image> _spawnedOrderIcons = new List<Image>();

    private int _turnCount = 0;
    private int _currentRecipeIndex = 0;
    private int _currentIngredientTurns = 0;
    private SSO_Ingredient _selectedIngredient = null;

    private void Awake()
    {
        foreach (var mapping in InputMapping)
        {
            if (mapping.InputAction != null && mapping.Ingredient != null)
            {
                _runtimeActionMap.Add(mapping.InputAction.action, mapping.Ingredient);
            }
        }
    }

    private void OnEnable()
    {
        OnTurnCompleted.OnEventRaised += HandleTurnCompletion;
    }

    private void OnDisable()
    {
        OnTurnCompleted.OnEventRaised -= HandleTurnCompletion;
    }

    private void Start()
    {
        GenerateOrderUI();
    }

    private void HandleTurnCompletion(int amount)
    {
        switch (CurrentState)
        {
            case PizzaState.DoughFlattening:
                ProcessDoughFlattening(amount);
                break;
            case PizzaState.IngredientAssembly:
                ProcessIngredientTurning(amount);
                break;
        }
    }

    void ProcessDoughFlattening(int turnAmount)
    {
        if (Mathf.Sign(turnAmount) == Mathf.Sign(RequiredTurnDirection))
        {
            _turnCount += Mathf.Abs(turnAmount);
            TurnText.text = $"{_turnCount} / {RequiredDoughTurns}";

            if (_turnCount >= RequiredDoughTurns)
            {
                Debug.Log("Pâte étalée !");
                StartIngredientAssembly();
                TurnText.text = "Pâte prête";
            }
        }
        else
        {
            Debug.Log("Mauvais sens de rotation");
        }
    }

    void StartIngredientAssembly()
    {
        Debug.Log("StartIngredientAssembly");
        CurrentState = PizzaState.IngredientAssembly;
        _currentRecipeIndex = 0;

        foreach (var action in _runtimeActionMap.Keys)
        {
            action.Enable();
            action.performed += OnIngredientPressed;
        }
    }

    void StopIngredientAssembly()
    {
        foreach (var action in _runtimeActionMap.Keys)
        {
            action.performed -= OnIngredientPressed;
            action.Disable();
        }
    }

    void OnIngredientPressed(InputAction.CallbackContext ctx)
    {
        Debug.Log("OnIngredientPressed");

        if (CurrentState != PizzaState.IngredientAssembly) return;

        if (_selectedIngredient != null) return;

        if (_runtimeActionMap.TryGetValue(ctx.action, out SSO_Ingredient pressedIngredient))
        {
            ValidateIngredientSelection(pressedIngredient);
        }
    }

    void ValidateIngredientSelection(SSO_Ingredient pressedIngredient)
    {
        Debug.Log("ValidateIngredientSelection");
        if (CurrentRecipe == null || _currentRecipeIndex >= CurrentRecipe.Count) return;

        SSO_Ingredient expectedIngredient = CurrentRecipe[_currentRecipeIndex];

        if (pressedIngredient == expectedIngredient)
        {
            _selectedIngredient = pressedIngredient;
            _currentIngredientTurns = 0;

            Debug.Log($"Sélection correcte : {pressedIngredient.IngredientName}. Tourne le joystick !");
            TurnText.text = $"Étale {pressedIngredient.IngredientName} (0/{TurnsToPlaceIngredient})";
        } else
        {
            Debug.Log($"Erreur de commande ! Attendu : {expectedIngredient.IngredientName} | Reçu : {pressedIngredient.IngredientName}");
        }
    }

    void ProcessIngredientTurning(int turnAmount)
    {
        if (_selectedIngredient == null) return;

        _currentIngredientTurns += Mathf.Abs(turnAmount);

        TurnText.text = $"Étale {_selectedIngredient.IngredientName} ({_currentIngredientTurns}/{TurnsToPlaceIngredient})";

        if (_currentIngredientTurns >= TurnsToPlaceIngredient)
        {
            Debug.Log($"Garniture ajoutée : {_selectedIngredient.IngredientName} !");

            Instantiate(_selectedIngredient.PizzaLayerPrefab, PizzaTransform);

            if (_currentRecipeIndex < _spawnedOrderIcons.Count)
            {
                Image completedIcon = _spawnedOrderIcons[_currentRecipeIndex];
                completedIcon.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            }

            _selectedIngredient = null;
            _currentRecipeIndex++;

            if (_currentRecipeIndex >= CurrentRecipe.Count)
            {
                Debug.Log("Recette complète ! On passe à la cuisson.");
                StopIngredientAssembly();
                CurrentState = PizzaState.Cooking;
                TurnText.text = "Au four !";
            }
            else
            {
                Debug.Log($"En attente du prochain ingrédient...");
                TurnText.text = "Garniture suivante ?";
            }
        }
    }

    private void GenerateOrderUI()
    {
        foreach (Transform child in OrderContainer)
        {
            Destroy(child.gameObject);
        }
        _spawnedOrderIcons.Clear();

        foreach (SSO_Ingredient ingredient in CurrentRecipe)
        {
            GameObject iconObj = Instantiate(OrderIconPrefab, OrderContainer);
            Image iconImage = iconObj.GetComponent<Image>();

            if (iconImage != null && ingredient.Icon != null)
            {
                iconImage.sprite = ingredient.Icon;
                _spawnedOrderIcons.Add(iconImage);
            }
        }
    }
}