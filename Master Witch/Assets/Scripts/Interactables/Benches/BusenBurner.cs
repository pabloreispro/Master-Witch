using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Rendering;
public class BusenBurner : Bench
{
    public const float TIMER_MULTI = 30;
    public NetworkVariable<float> timeBusen = new();
    public Slider tempSlider;
    public Image backgroundSliderTemp;
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
                _UpTimeBenchServerRpc();
                
            }else if(timeBusen.Value >= 0){
                _DownTimeBenchServerRpc();
            }
            tempSlider.value = timeBusen.Value;
            switch(timeBusen.Value)
            {
                case >= 90 and <= 100:
                    ChangeVariableServerRpc(false);
                    _player.buttonPressed = false;
                    backgroundSliderTemp.color = Color.red;
                    _player = null;
                    break;

                case >= 75 and < 90:
                    ChangeVariableServerRpc(true);
                    backgroundSliderTemp.color = Color.green;
                    break;

                default:
                    ChangeVariableServerRpc(false);
                    backgroundSliderTemp.color = Color.blue;
                    _player = null;
                    break;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void _UpTimeBenchServerRpc(){
        timeBusen.Value = timeBusen.Value + Time.deltaTime * TIMER_MULTI;
    }

    [ServerRpc(RequireOwnership = false)]
    private void _DownTimeBenchServerRpc(){
        timeBusen.Value = timeBusen.Value - Time.deltaTime * TIMER_MULTI;
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
