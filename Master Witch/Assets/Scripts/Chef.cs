using Game.SO;
using Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;

public class Chef : Interactable
{
    [SerializeField] ChefSO chefSO;

    // PROVISORIO
    public override void Drop(Player player)
    {
        var tool = player.GetComponentInChildren<Tool>();
        if (tool?.ingredients.Count <= 0) return;
        base.Drop(player);
        Debug.Log("1");
        Review(tool.ingredients[0], player.id);
        Debug.Log("3");
        tool.DestroySelf();
        //player.StatusAssetServerRpc(false);
    }
    public override void Pick(Player player)
    {
        var tool = player.GetComponentInChildren<Tool>();
        if (tool?.ingredients.Count <= 0) return;
        base.Pick(player);
        //Review(player.recipeIngredients, playerRecipe, player.id);
        Review(tool.ingredients[0], player.id);
    }
    public void Review(RecipeData recipe, int playerID)
    {
        Debug.Log("2");
        var score = chefSO.ReviewRecipe(recipe);
        Debug.Log($"Total score of {recipe.TargetFood.name} is {score}");
        NetworkManagerUI.Instance.UpdatePlayerScore(playerID, score);
        EliminationPlayer.Instance.UpdadeScoresPlayers(playerID, score);
    }
}
