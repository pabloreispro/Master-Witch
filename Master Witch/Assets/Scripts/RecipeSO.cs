using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.SO;

namespace Game.SO {

[CreateAssetMenu(fileName = "RecipeSO", menuName = "Game/RecipeSO")]
    public class RecipeSO : FoodSO
    {
       
        public CategoryModifier categoryModifier;
        public RecipeCondition[] recipeConditions;
        public bool CheckConditions(List<FoodSO> ingredients)
        {
            for (int i = 0; i < recipeConditions.Length; i++)
            {
                if (!recipeConditions[i].CheckCondition(ingredients))
                {
                    Debug.Log($"Condition {i} is not completed");
                    return false;
                }
            }
            Debug.Log($"All conditions completed");
            return true;
        }
    }
}
