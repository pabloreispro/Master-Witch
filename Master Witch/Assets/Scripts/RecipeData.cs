using Game.SO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.AI;

[System.Serializable]
public class RecipeData
{
    [SerializeField] public FoodSO targetFood;
    [SerializeField] List<RecipeData> utilizedIngredients = new List<RecipeData>();
    FoodModifiers foodModifiers;
    public FoodSO TargetFood => targetFood;
    public List<RecipeData> UtilizedIngredients => utilizedIngredients;
    public FoodModifiers FoodModifiers => foodModifiers;

    public RecipeData(FoodSO targetFood)
    {
        this.targetFood = targetFood;
        foodModifiers = targetFood.Modifiers;
    }
    public RecipeData(FoodSO targetFood, List<RecipeData> utilizedIngredients)
    {
        this.targetFood = targetFood;
        foreach (var item in utilizedIngredients)
        {
            this.utilizedIngredients.Add(item);
        }
        //Igneous
        float igneousValue = 0;
        for (int i = 0; i < utilizedIngredients.Count; i++)
            igneousValue += utilizedIngredients[i].foodModifiers.IgneousValue;
        igneousValue /= utilizedIngredients.Count;
        //Poisonous
        float poisonousValue = 0;
        for (int i = 0; i < utilizedIngredients.Count; i++)
            poisonousValue += utilizedIngredients[i].foodModifiers.PoisonousValue;
        poisonousValue /= utilizedIngredients.Count;
        //Curative
        float curativeValue = 0;
        for (int i = 0; i < utilizedIngredients.Count; i++)
            curativeValue += utilizedIngredients[i].foodModifiers.CurativeValue;
        curativeValue /= utilizedIngredients.Count;

        foodModifiers = new FoodModifiers(igneousValue, poisonousValue, curativeValue);
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
