using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.SO;

namespace Game.SO {

[CreateAssetMenu(fileName = "RecipeSO", menuName = "Game/RecipeSO")]
    public class RecipeSO : FoodSO
    {
        public bool finishRecipe;
        public CategoryModifier categoryModifier;
        public RecipeCondition[] recipeConditions;
        public bool CheckConditions(List<FoodSO> ingredients, BenchType benchType)
        {
            for (int i = 0; i < recipeConditions.Length; i++)
            {
                if (!recipeConditions[i].CheckCondition(ingredients, benchType))
                {
                    Debug.Log($"Condition {i} is not completed");
                    return false;
                }
            }
            Debug.Log($"All conditions completed");
            return true;
        }
        public float GetScore(List<RecipeData> foods)
        {
            float score = 0;
            foreach (var item in foods)
            {
                float modifier = 0;
                for (int i = 0; i < item.TargetFood.category.Length; i++)
                {
                    switch (item.TargetFood.category[i])
                    {
                        case Category.Animal:
                            modifier += categoryModifier.Animal;
                            Debug.Log($"Adding {categoryModifier.Animal} modifier by Animal category preference in {item.TargetFood.name} in the recipe {name}. Total {modifier}");
                            break;
                        case Category.Vegetal:
                            modifier += categoryModifier.Vegetal;
                            Debug.Log($"Adding {categoryModifier.Vegetal} modifier by Vegetal category preference in {item.TargetFood.name} in the recipe {name}. Total {modifier}");
                            break;
                        case Category.Fungi:
                            modifier += categoryModifier.Fungi;
                            Debug.Log($"Adding {categoryModifier.Fungi} modifier by Fungi category preference in {item.TargetFood.name} in the recipe {name}. Total {modifier}");
                            break;
                        case Category.Mystical:
                            modifier += categoryModifier.Mystical;
                            Debug.Log($"Adding {categoryModifier.Mystical} modifier by Mystical category preference in {item.TargetFood.name} in the recipe {name}. Total {modifier}");
                            break;
                        default:
                            break;
                    }
                    var recipe = item.TargetFood as RecipeSO;
                    if (recipe != null)
                        score += recipe.GetScore(item.UtilizedIngredients);
                }
                score += item.TargetFood.score * modifier;
            }
            return score;
        }
    }
}
