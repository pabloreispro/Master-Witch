using UnityEngine;
using Game.SO;

[System.Serializable]
public class RecipeCondition 
{
    public Ingredient ingredient;
    public RecipeSO recipe;
    public Category category;
    public float categoryPoints;
    public bool isAllowed;

    public bool CheckCondition()
    {
        return false;
    }

}
