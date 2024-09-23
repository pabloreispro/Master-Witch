using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
public class BusenBurner : Bench
{
    public const float TIMER_MULTI = 10;
    public NetworkVariable<float> timeBusen = new();
    private void Start()
    {
        isPreparing.OnValueChanged += (a,b) => visualEffect[0].SetBool("isPreparing", isPreparing.Value);
        isPreparing.OnValueChanged += (a,b) => visualEffect[1].SetBool("isPreparing", isPreparing.Value);
    }

    private void FixedUpdate()
    {
        if(_player !=null && ingredients.Count > 0){
            if(_player.buttonPressed){
                Debug.Log("Busen");
                timeBusen.Value = timeBusen.Value + Time.deltaTime * TIMER_MULTI;
            }else if(timeBusen.Value >= 0){
                timeBusen.Value = timeBusen.Value - Time.deltaTime * TIMER_MULTI;
            }

            switch(timeBusen.Value)
            {
                case >= 90 and <= 100:
                    isPreparing.Value = false;
                    _player.buttonPressed = false;
                    _player = null;
                    break;

                case >= 75 and < 90:
                    isPreparing.Value = true;
                    break;

                default:
                    isPreparing.Value = false;
                    _player = null;
                    break;
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
            player.GetComponentInChildren<Ingredient>().itensUsed.Add(recipeData);
            player.SetItemHandClientRpc(objectSpawn);
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
    }
}
