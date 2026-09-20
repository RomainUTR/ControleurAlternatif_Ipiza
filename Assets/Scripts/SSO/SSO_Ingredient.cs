using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "SSO_Ingredient_new", menuName = "Data/SSO/SSO_Ingredient")]
public class SSO_Ingredient : ScriptableObject
{
    [Header("Visuals")]
    public string IngredientName;
    public Sprite Icon;

    public GameObject PizzaLayerPrefab;

    [Header("Input Mapping")]
    public InputActionReference InputAction;
}