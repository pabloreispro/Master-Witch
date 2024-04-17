using Game.SO;
using Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chef : Interactable
{
    [SerializeField] ChefSO chefSO;

    // PROVISORIO
    public override void Drop(Player player)
    {
        var playerRecipe = player.ingredient as RecipeSO;
        if (playerRecipe == null) return;
        base.Drop(player);
        Review(player.recipeIngredients, playerRecipe, player.id);
        player.StatusAssetServerRpc(false);
    }
    public override void Pick(Player player)
    {
        var playerRecipe = player.ingredient as RecipeSO;
        if (playerRecipe == null) return;
        base.Pick(player);
        Review(player.recipeIngredients, playerRecipe, player.id);
    }
    public void Review(List<FoodSO> foods, RecipeSO targetRecipe, int playerID)
    {
        var score = chefSO.ReviewRecipe(foods, targetRecipe);
        Debug.Log($"Total score of {targetRecipe.name} is {score}");
        NetworkManagerUI.Instance.UpdatePlayerScore(playerID, score);
    }
}
