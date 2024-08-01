using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
public class BusenBurner : Bench
{
    public override void Pick(Player player)
    {
        if (endProgress)
        {
            slider.gameObject.SetActive(false);
            var recipeData = new RecipeData(targetRecipe, ingredients);
            var objectSpawn = Instantiate(recipeData.TargetFood.foodPrefab, new Vector3(player.assetIngredient.transform.position.x, 1.0f, player.assetIngredient.transform.position.z), Quaternion.identity);
            objectSpawn.GetComponent<NetworkObject>().Spawn();
            objectSpawn.GetComponent<NetworkObject>().TrySetParent(player.transform);
            player.GetComponentInChildren<Ingredient>().itensUsed.Add(recipeData);
            /*toolInBench.ingredients.Clear();
            toolInBench.ingredients.Add(recipeData);
            toolInBench.transform.position = player.boneItem.transform.position;
            toolInBench.gameObject.GetComponent<FollowTransform>().targetTransform = player.boneItem.transform;
            toolInBench.GetComponent<NetworkObject>().TrySetParent(player.transform);
            player.SetItemHandClientRpc(toolInBench.gameObject);*/
            Reset();
        }
    }
    public override void Drop(Player player)
    {
        var interact = player.GetComponentInChildren<Ingredient>();
        endProgress = false;
        AddIngredient(interact.food);
        interact.DestroySelf();
        player.isHand.Value = false;
    }
}
