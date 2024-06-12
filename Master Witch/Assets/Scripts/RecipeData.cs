using Game.SO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.AI;

[System.Serializable]
public class RecipeData
{
    [SerializeField] FoodSO targetFood;
    [SerializeField] List<RecipeData> utilizedIngredients = new List<RecipeData>();
    public FoodSO TargetFood => targetFood;
    public List<RecipeData> UtilizedIngredients => utilizedIngredients;

    public RecipeData(FoodSO targetFood)
    {
        this.targetFood = targetFood;
    }
    public RecipeData(FoodSO targetFood, List<RecipeData> utilizedIngredients)
    {
        this.targetFood = targetFood;
        foreach (var item in utilizedIngredients)
        {
            this.utilizedIngredients.Add(item);
        }
    }

    public float CalculateScore(Func<FoodSO, float> conditionAction)
    {
        if (conditionAction == null) return 0;
        float score = conditionAction.Invoke(targetFood);
        if (utilizedIngredients.Count > 0)
        {
            for (int i = 0; i < utilizedIngredients.Count; i++)
            {
                score += utilizedIngredients[i].CalculateScore(conditionAction);
            }
        }
        return score;
    }
}
