using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
public class BusenBurner : Bench
{
    private void Start()
    {
        isPreparing.OnValueChanged += (a,b) => visualEffect[0].SetBool("isPreparing", isPreparing.Value);
        isPreparing.OnValueChanged += (a,b) => visualEffect[1].SetBool("isPreparing", isPreparing.Value);
    }
    
    public override void Pick(Player player)
    {
        if (endProgress)
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
        var interact = player.GetComponentInChildren<Ingredient>();
        endProgress = false;
        AddIngredient(interact.food);
        progress();
        interact.DestroySelf();
        player.isHand.Value = false;
    }
}
