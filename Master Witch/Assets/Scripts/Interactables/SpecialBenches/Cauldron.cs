using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Cauldron : Bench
{
    private ToolsSO toolInBench;

    private void FixedUpdate() {
        if(toolInBench!=null && ingredients.Count > 0){
            isPreparing.Value = true;
        }
    }

    public override void Pick(Player player)
    {
        if (endProgress && player.isHand.Value == false)
        {
            var recipeData = new RecipeData(targetRecipe, ingredients);
            var objectSpawn = Instantiate(recipeData.TargetFood.foodPrefab, new Vector3(player.assetIngredient.transform.position.x, 1.0f, player.assetIngredient.transform.position.z), Quaternion.identity);
            objectSpawn.GetComponent<NetworkObject>().Spawn();
            objectSpawn.GetComponent<NetworkObject>().TrySetParent(player.transform);
            player.GetComponentInChildren<Ingredient>().itensUsed.Add(recipeData);
            toolInBench = null;
            Reset();
        }
    }

    public override void Drop(Player player)
    {
        var interact = player.GetComponentInChildren<Interactable>();
        switch(interact){
            case Ingredient:
                endProgress = false;
                AddIngredient((interact as Ingredient).food);
                progress();
            break;
            case Tool:
                toolInBench = (interact as Tool).tool;
            break;
        }
        interact.DestroySelf();
    }
}
