using System.Collections.Generic;
using TMPro;
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
    [SerializeField] private int TurnsToPlaceIngredient = 2;

    [Header("Oven Settings")]
    [SerializeField] private int TurnsToOven = 5;

    [Header("Input Mapping")]
    [Tooltip("Associe chaque action d'input (bouton) à son ingrédient correspondant.")]
    [SerializeField] private List<IngredientMapping> InputMapping = new List<IngredientMapping>();
    [SerializeField] private RSE_OnTurnCompleted OnTurnCompleted;
    [SerializeField] private InputActionReference OvenInputAction;
    [SerializeField] private InputActionReference ServeInputAction;

    [Header("Scene References")]
    [SerializeField] private Transform PizzaTransform;
    [SerializeField] private TMP_Text TurnText;

    [Header("UI References")]
    [SerializeField] private Transform OrderContainer;
    [SerializeField] private GameObject OrderIconPrefab;

    [Header("Events")]
    [SerializeField] private RecipeGenerator RecipeGen;

    private Dictionary<InputAction, SSO_Ingredient> _runtimeActionMap = new Dictionary<InputAction, SSO_Ingredient>();
    private List<Image> _spawnedOrderIcons = new List<Image>();

    private int _turnCount = 0;
    private int _currentRecipeIndex = 0;
    private int _currentIngredientTurns = 0;
    private SSO_Ingredient _selectedIngredient = null;
    private int _currentCookingTurns = 0;
    private bool _isOvenOn = false;
    private List<SSO_Ingredient> CurrentRecipe;

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
        if (RecipeGen != null) RecipeGen.OnRecipeGenerated += HandleNewRecipe;
    }

    private void OnDisable()
    {
        OnTurnCompleted.OnEventRaised -= HandleTurnCompletion;
        if (RecipeGen != null) RecipeGen.OnRecipeGenerated -= HandleNewRecipe;
    }

    private void Start()
    {
        if (RecipeGen != null) RecipeGen.GenerateRecipe();
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
            case PizzaState.Cooking:
                ProcessCookingTurning(amount);
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
                StartCooking();
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

            iconObj.transform.SetAsFirstSibling();

            Image iconImage = iconObj.GetComponent<Image>();

            if (iconImage != null && ingredient.Icon != null)
            {
                iconImage.sprite = ingredient.Icon;
                _spawnedOrderIcons.Add(iconImage);
            }
        }
    }

    private void StartCooking()
    {
        CurrentState = PizzaState.Cooking;
        _currentCookingTurns = 0;
        _isOvenOn = false;
        TurnText.text = "Appuyez sur le bouton du four !";

        if (OvenInputAction != null)
        {
            OvenInputAction.action.Enable();
            OvenInputAction.action.performed += OnOvenPressed;
        }
    }

    private void StopCooking()
    {
        if (OvenInputAction != null)
        {
            OvenInputAction.action.performed -= OnOvenPressed;
            OvenInputAction.action.Disable();
        }
    }

    private void OnOvenPressed(InputAction.CallbackContext ctx)
    {
        if (CurrentState != PizzaState.Cooking) return;

        if (!_isOvenOn)
        {
            _isOvenOn = true;
            Debug.Log("Le four est allumé ! Tournez la pizza !");
            TurnText.text = $"Cuisson : {_currentCookingTurns}/{TurnsToOven}";
        } else if (_currentCookingTurns >= TurnsToOven)
        {
            _isOvenOn = false;
            StopCooking();
            StartServing();
        }
    }

    private void ProcessCookingTurning(int amount)
    {
        if (!_isOvenOn || _currentCookingTurns >= TurnsToOven) return;

        _currentCookingTurns += Mathf.Abs(amount);
        TurnText.text = $"Cuisson : {_currentCookingTurns}/{TurnsToOven}";

        if (_currentCookingTurns >= TurnsToOven)
        {
            Debug.Log("Cuisson parfaite ! Eteignez le four !");
            TurnText.text = "Eteignez le four ! (Appuyez sur le bouton)";
        }
    }

    private void StartServing()
    {
        CurrentState = PizzaState.Ready;
        TurnText.text = "Pizza prête ! Appuyez pour servir.";

        if (ServeInputAction != null)
        {
            ServeInputAction.action.Enable();
            ServeInputAction.action.performed += OnServe;
        }
    }

    private void StopServing()
    {
        if (ServeInputAction != null)
        {
            ServeInputAction.action.performed -= OnServe;
            ServeInputAction.action.Disable();
        }
    }

    private void OnServe(InputAction.CallbackContext ctx)
    {
        if (CurrentState != PizzaState.Ready) return;

        Debug.Log("Pizza servie ! En attente de la prochaine commande...");

        // AddScore

        StopServing();
        ResetForNextOrder();
    }

    private void ResetForNextOrder()
    {
        foreach (Transform child in PizzaTransform)
        {
            if (!child.CompareTag("Indicator"))
            {
                Destroy(child.gameObject);
            }
        }

        _turnCount = 0;
        _currentRecipeIndex = 0;
        _currentIngredientTurns = 0;
        _currentCookingTurns = 0;
        _selectedIngredient = null;
        _isOvenOn = false;

        CurrentState = PizzaState.DoughFlattening;
        TurnText.text = $"0/{RequiredDoughTurns}";

        if (RecipeGen != null) RecipeGen.GenerateRecipe();
    }

    private void HandleNewRecipe(List<SSO_Ingredient> generatedRecipe)
    {
        CurrentRecipe = generatedRecipe;
        GenerateOrderUI();
    }
}