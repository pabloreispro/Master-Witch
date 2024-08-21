using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Furnace : Bench
{
    public List<ToolsSO> _toolInBench = new();

    private float _timerWood;

    private void FixedUpdate() {
        _Special();
    }

    private void _Special(){
        if(_toolInBench.Count>0 && ingredients.Count > 0){
            isPreparing.Value = true;
            _timerWood += Time.deltaTime;
            if(_timerWood >= 5){
                _toolInBench.RemoveAt(_toolInBench.Count-1);
                _timerWood = 0;
            }
        }else{
            isPreparing.Value = false;
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
            _toolInBench.Clear();          
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
            break;
            case Tool t when t.tool.benchType == benchType:         
                _toolInBench.Add(t.tool);
            break;
        }
        interact.DestroySelf();
    }
}
