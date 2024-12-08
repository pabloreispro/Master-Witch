using Game.SO;
using Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;

public class TrialBench : Interactable
{
    // PROVISORIO
    public FoodSO Delivery { get; private set; }
    public override void Drop(Player player)
    {
        var recipe = player.GetComponentInChildren<Ingredient>();
        if (recipe == null) return;
        base.Drop(player);
        Delivery = recipe.food;
        Review(new RecipeData(recipe.food, recipe.itemsUsed), player.id);
        recipe.DestroySelf();
    }
    public override void Pick(Player player)
    {
        //var tool = player.GetComponentInChildren<Tool>();
        //if (tool?.ingredients.Count <= 0) return;
        //base.Pick(player);
        //Review(player.recipeIngredients, playerRecipe, player.id);
        //Review(tool.ingredients[0], player.id);

    }
    public void Review(RecipeData recipe, int playerID)
    {
        ScoreManager.Instance.SetPlayerScore(playerID, recipe);
    }
}
