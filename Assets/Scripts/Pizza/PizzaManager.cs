using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class PizzaManager : MonoBehaviour
{
    public enum PizzaState
    {
        DoughFlattening,
        IngredientAssembly,
        Cooking,
        Ready
    }

    [System.Serializable]
    public struct IngredientTray
    {
        public SSO_Ingredient Ingredient;
        public GameObject FilledVisual;
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

    [Header("Inputs")]
    [SerializeField] private SSO_InputReader InputReader;

    [Header("Scene References")]
    [SerializeField] private Transform PizzaTransform;
    [SerializeField] private TMP_Text TurnText;
    [SerializeField] private RSO_GameMode CurrentGameMode;

    [Header("UI References")]
    [SerializeField] private Transform OrderContainer;
    [SerializeField] private GameObject OrderIconPrefab;

    [Header("Events")]
    [SerializeField] private RecipeGenerator RecipeGen;
    [SerializeField] private RSE_OnTurnCompleted OnTurnCompleted;
    [SerializeField] private RSE_RequestScoring RequestScoring;
    [SerializeField] private SSO_ScoreData ScoreData;
    [SerializeField] private RSE_RefreshUI RefreshUI;
    [SerializeField] private RSO_Score Score;

    [Header("Trays Visuals")]
    [SerializeField] private List<IngredientTray> Trays;

    private List<Image> _spawnedOrderIcons = new List<Image>();

    private int _turnCount = 0;
    private int _currentRecipeIndex = 0;
    private int _currentIngredientTurns = 0;
    private SSO_Ingredient _selectedIngredient = null;
    private int _currentCookingTurns = 0;
    private bool _isOvenOn = false;
    private List<SSO_Ingredient> CurrentRecipe;

    private void OnEnable()
    {
        OnTurnCompleted.OnEventRaised += HandleTurnCompletion;
        InputReader.OnIngredientPressedEvent += HandleIngredient;
        InputReader.OnOvenPressedEvent += HandleOven;
        InputReader.OnServePressedEvent += HandleServe;
        if (RecipeGen != null) RecipeGen.OnRecipeGenerated += HandleNewRecipe;
    }

    private void OnDisable()
    {
        OnTurnCompleted.OnEventRaised -= HandleTurnCompletion;
        InputReader.OnIngredientPressedEvent -= HandleIngredient;
        InputReader.OnOvenPressedEvent -= HandleOven;
        InputReader.OnServePressedEvent -= HandleServe;
        if (RecipeGen != null) RecipeGen.OnRecipeGenerated -= HandleNewRecipe;
    }

    private void Start()
    {
        if (RecipeGen != null) RecipeGen.GenerateRecipe();
    }

    private void HandleTurnCompletion(int amount)
    {
        if (CurrentGameMode.CurrentMode != RSO_GameMode.GameMode.Pizza) return;

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
        CurrentState = PizzaState.IngredientAssembly;
        _currentRecipeIndex = 0;
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

            FillTray(pressedIngredient);

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

            EmptyTray(_selectedIngredient);

            _selectedIngredient = null;
            _currentRecipeIndex++;

            if (_currentRecipeIndex >= CurrentRecipe.Count)
            {
                Debug.Log("Recette complète ! On passe à la cuisson.");
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

        foreach (var tray in Trays)
        {
            if (tray.FilledVisual != null)
            {
                tray.FilledVisual.SetActive(false);
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

    private void HandleIngredient(SSO_Ingredient pressedIngredient)
    {
        if (CurrentGameMode.CurrentMode != RSO_GameMode.GameMode.Pizza) return;
        if (CurrentState != PizzaState.IngredientAssembly) return;
        if (_selectedIngredient != null) return;

        ValidateIngredientSelection(pressedIngredient);
    }

    private void HandleOven()
    {
        if (CurrentGameMode.CurrentMode != RSO_GameMode.GameMode.Pizza) return;
        if (CurrentState != PizzaState.Cooking) return;

        if (!_isOvenOn)
        {
            _isOvenOn = true;
            TurnText.text = $"Cuisson : {_currentCookingTurns}/{TurnsToOven}";
        }
        else if (_currentCookingTurns >= TurnsToOven)
        {
            _isOvenOn = false;
            StartServing();
        }
    }

    private void HandleServe()
    {
        if (CurrentGameMode.CurrentMode != RSO_GameMode.GameMode.Pizza) return;
        if (CurrentState != PizzaState.Ready) return;

        Debug.Log("Pizza servie ! En attente de la prochaine commande...");
        
        int n = CurrentRecipe.Count;
        int earnedPoint = ScoreData.PointsPerPizza * (n * n);

        RequestScoring.Raise(earnedPoint);

        Score.RuntimeMultiplier += ScoreData.MultiplierByComboUnit;
        Score.RuntimeCombo++;

        RefreshUI.Raise();

        ResetForNextOrder();
    }

    private void FillTray(SSO_Ingredient ingredient)
    {
        foreach (var tray in Trays)
        {
            if (tray.Ingredient == ingredient && tray.FilledVisual != null)
            {
                tray.FilledVisual.SetActive(true);
                break;
            }
        }
    }

    private void EmptyTray(SSO_Ingredient ingredient)
    {
        foreach (var tray in Trays)
        {
            if (tray.Ingredient == ingredient && tray.FilledVisual != null)
            {
                tray.FilledVisual.SetActive(false);
                break;
            }
        }
    }
}