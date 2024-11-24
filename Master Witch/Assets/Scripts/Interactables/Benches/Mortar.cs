using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Mortar : Bench
{
    private void Start()
    {
        
    }

    private void FixedUpdate()
    {
        if(_player !=null && ingredients.Count > 0){
            if(_player.buttonPressed){
                Debug.Log("Mortar");
                ChangeVariableServerRpc(true);
            }else{
                ChangeVariableServerRpc(false);
                _player = null;
            }
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
            player.GetComponentInChildren<Ingredient>().itemsUsed.Add(recipeData);
            player.SetItemHandClientRpc(objectSpawn);
            Reset();
        }
    }
    public override void Drop(Player player)
    {
        var interact = player.GetComponentInChildren<Ingredient>();
        endProgress = false;
        AddIngredient(interact);
        progress();
        interact.DestroySelf();
    }
}
