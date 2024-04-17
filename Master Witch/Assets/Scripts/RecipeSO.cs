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
        public float GetScore(List<FoodSO> foods)
        {
            float score = 0;
            foreach (var item in foods)
            {
                float modifier = 0;
                for (int i = 0; i < item.category.Length; i++)
                {
                    switch (item.category[i])
                    {
                        case Category.Animal:
                            modifier += categoryModifier.Animal;
                            break;
                        case Category.Vegetal:
                            modifier += categoryModifier.Vegetal;
                            break;
                        case Category.Fungi:
                            modifier += categoryModifier.Fungi;
                            break;
                        case Category.Mystical:
                            modifier += categoryModifier.Mystical;
                            break;
                        default:
                            break;
                    }
                }
                score += item.score * modifier;
            }
            return score;
        }
    }
}
