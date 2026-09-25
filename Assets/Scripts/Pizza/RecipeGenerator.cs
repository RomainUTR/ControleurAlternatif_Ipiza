using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RecipeGenerator : MonoBehaviour
{
    [Header("Generator Pools")]
    [SerializeField] private List<SSO_Ingredient> CheesePool;
    [SerializeField] private List<SSO_Ingredient> ProteinPool;
    [SerializeField] private List<SSO_Ingredient> OtherPool;

    public event UnityAction<List<SSO_Ingredient>> OnRecipeGenerated;

    public void GenerateRecipe()
    {
        List<SSO_Ingredient> newRecipe = new List<SSO_Ingredient>();
        int recipeSize = Random.Range(2, 5);

        AddIngredientToRecipe(newRecipe, GetRandomIngredientFrom(CheesePool));

        if (newRecipe.Count < recipeSize)
        {
            AddIngredientToRecipe(newRecipe, GetRandomIngredientFrom(ProteinPool));
        }

        List<SSO_Ingredient> remainingPool = new List<SSO_Ingredient>();
        remainingPool.AddRange(OtherPool);

        while(newRecipe.Count < recipeSize)
        {
            AddIngredientToRecipe(newRecipe, GetRandomIngredientFrom(remainingPool));
        }

        OnRecipeGenerated?.Invoke(newRecipe);
    }

    private SSO_Ingredient GetRandomIngredientFrom(List<SSO_Ingredient> pool)
    {
        if (pool == null || pool.Count == 0) return null;
        return pool[Random.Range(0, pool.Count)];
    }

    private void AddIngredientToRecipe(List<SSO_Ingredient> recipe, SSO_Ingredient ingredient)
    {
        if (ingredient == null) return;

        int randomIndex = Random.Range(0, recipe.Count + 1);
        recipe.Insert(randomIndex, ingredient);
    }
}