using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.SO;

namespace Game.SO {

[CreateAssetMenu(fileName = "RecipeSO", menuName = "Game/RecipeSO")]
    public class RecipeSO : FoodSO
    {
        const float FOOD_MATCH_SCORE = 5;
        const float CATEGORY_MATCH_SCORE = 2.5f;
        public bool finishRecipe;
        public RecipeCondition[] recipeConditions;
        public ReviewCondition[] reviewConditions;
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
        //public float GetScore(List<RecipeData> foods)
        //{
        //    float score = 0;
        //    foreach (var food in foods)
        //    {
        //        for (int i = 0; i < reviewConditions.Length; i++)
        //        {
        //            switch (reviewConditions[i].type)
        //            {
        //                case ReviewType.FoodPreference:
        //                    if (reviewConditions[i].foods.Contains(food.TargetFood))
        //                    {
        //                        if (reviewConditions[i].isAllowed)
        //                        {
        //                            score += FOOD_MATCH_SCORE;
        //                            Debug.Log($"Adding {FOOD_MATCH_SCORE} score by food preference in {food.TargetFood.name}. Total {score}");
        //                        }
        //                        else
        //                        {
        //                            score -= FOOD_MATCH_SCORE;
        //                            Debug.Log($"Removing {FOOD_MATCH_SCORE} score by food preference in {food.TargetFood.name}. Total {score}");
        //                        }
        //                    }
        //                    break;
        //                case ReviewType.CategoryPreference:
        //                    for (int j = 0; j < food.TargetFood.category.Length; j++)
        //                    {
        //                        if (food.TargetFood.category[j] == reviewConditions[i].category)
        //                        {
        //                            if (reviewConditions[i].isAllowed)
        //                            {
        //                                score += CATEGORY_MATCH_SCORE;
        //                                Debug.Log($"Adding {CATEGORY_MATCH_SCORE} score by category preference in {food.TargetFood.name}. Total {score}");
        //                            }
        //                            else
        //                            {
        //                                score -= CATEGORY_MATCH_SCORE;
        //                                Debug.Log($"Removing {CATEGORY_MATCH_SCORE} score by category preference in {food.TargetFood.name}. Total {score}");
        //                            }
        //                        }
        //                    }
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }
        //    }
        //    return score;
        //}
    }
}
