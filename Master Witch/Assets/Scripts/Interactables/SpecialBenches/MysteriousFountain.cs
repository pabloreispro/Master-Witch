using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MysteriousFountain : Bench
{
    public override void Pick(Player player)
    {
        if (endProgress && player.isHand.Value == false)
        {
            var recipeData = new RecipeData(targetRecipe, ingredients);
            var objectSpawn = Instantiate(recipeData.TargetFood.foodPrefab, new Vector3(player.assetIngredient.transform.position.x, 1.0f, player.assetIngredient.transform.position.z), Quaternion.identity);
            objectSpawn.GetComponent<NetworkObject>().Spawn();
            objectSpawn.GetComponent<NetworkObject>().TrySetParent(player.transform);
            player.GetComponentInChildren<Ingredient>().itensUsed.Add(recipeData);            
            Reset();
        }
    }
    public override void Drop(Player player)
    {
        var interact = player.GetComponentInChildren<Interactable>();
        switch(interact){
            case Ingredient i:
                endProgress = false;
                AddIngredient(i.food);
                progress();
                isPreparing.Value = true;
            break;
        }
        interact.DestroySelf();
    }
}
