using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Game.SO;

public class MysteriousFountain : Bench
{
    public List<ToolsSO> _toolInBench = new();
    public FoodSO food;
    private void FixedUpdate() {
        _Special();
    }

    private void _Special(){
        if(_toolInBench.Count>0){
            ChangeVariableServerRpc(true);
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
            _toolInBench.Clear();         
            Reset();
        }
    }
    public override void Drop(Player player)
    {
        if(_toolInBench.Count < 2){
            var interact = player.GetComponentInChildren<Interactable>();
            switch(interact){
                case Tool t when t.tool.benchType == benchType:
                    _toolInBench.Add(t.tool);
                break;
            }
            endProgress = false;
            AddIngredient(food);
            progress();
            interact.DestroySelf();
        }
    }
}
