using Game.SO;
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
        Review(player.ingredientsBasket, playerRecipe);
    }
    public override void Pick(Player player)
    {
        var playerRecipe = player.ingredient as RecipeSO;
        if (playerRecipe == null) return;
        base.Pick(player);
        Review(player.ingredientsBasket, playerRecipe);
    }
    public void Review(List<FoodSO> foods, RecipeSO targetRecipe)
    {
        Debug.Log($"Total score of {targetRecipe.name} is {chefSO.ReviewRecipe(foods, targetRecipe)}");
    }
}
