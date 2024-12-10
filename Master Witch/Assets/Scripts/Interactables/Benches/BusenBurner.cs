using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Rendering;
using Game.UI;
public class BusenBurner : Bench
{
    public const float TIMER_MULTI = 30;
    public float timeBusen;
    public Slider tempSlider;
    public Image backgroundSliderTemp;
    public GameObject fire, smoke;
    public AudioSource sfx;

    public bool _wasPlayerInteracting;

    [ServerRpc(RequireOwnership = false)]
    public void EnableSFXServerRpc(){
        EnableSFXClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DisableSFXServerRpc(){
        DisableSFXClientRpc();
    }

    [ClientRpc]
    public void EnableSFXClientRpc(){
        sfx.Play();
    }

    [ClientRpc]
    public void DisableSFXClientRpc(){
        sfx.Stop();
    }

    private void FixedUpdate()
    {
        if(_player !=null && ingredients.Count > 0){
            //GameInterfaceManager.Instance.spaceKey.SetActive(true);
            if(_player.buttonPressed){
                Debug.Log("Busen");
                _UpTimeBench();
                EnabledParticlesServerRpc();
            }else if(timeBusen > 0){
                EnableSFXServerRpc();
                _DownTimeBench();
                DisableParticlesServerRpc();
            }else if(timeBusen <= 0){
                DisableSFXServerRpc();
            }
            tempSlider.value = timeBusen;
            switch(timeBusen)
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
        else if(_player == null && timeBusen>0){
            
            DisableSFXClientRpc();
            _DownTimeBench();
            DisableParticlesServerRpc();
        }

        if( (_player!=null) != _wasPlayerInteracting)
        {
            _wasPlayerInteracting = !_wasPlayerInteracting;
            GameInterfaceManager.Instance.eKey.SetActive(_player!=null && _player.isHand.Value);
            //GameInterfaceManager.Instance.spaceKey.SetActive(_player!= null && ingredients.Count > 0 && objectInBench == null);
            
        }
    }

    //[ServerRpc(RequireOwnership =false)]
    private void _UpTimeBench(){
        timeBusen = timeBusen + Time.deltaTime * TIMER_MULTI;
    }

    //[ServerRpc(RequireOwnership =false)]
    private void _DownTimeBench(){
        timeBusen = timeBusen - Time.deltaTime * TIMER_MULTI;
    }
    
    public override void Pick(Player player)
    {
        if (endProgress && player.isHand.Value == false)
        {
            /*var recipeData = new RecipeData(targetRecipe, ingredients);
            var objectSpawn = Instantiate(recipeData.TargetFood.foodPrefab, new Vector3(player.assetIngredient.transform.position.x, 1.0f, player.assetIngredient.transform.position.z), Quaternion.identity);
            objectSpawn.GetComponent<NetworkObject>().Spawn();
            objectSpawn.GetComponent<NetworkObject>().TrySetParent(player.transform);
            player.GetComponentInChildren<Ingredient>().itemsUsed.Add(recipeData);
            player.SetItemHandClientRpc(objectSpawn);
            Reset();*/
            objectInBench.GetComponentInChildren<NetworkObject>().TrySetParent(player.transform);
            player.SetItemHandClientRpc(objectInBench);
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

    [ServerRpc(RequireOwnership = false)]
    public void EnabledParticlesServerRpc()
    {
        EnabledParticlesClientRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    public void DisableParticlesServerRpc()
    {
        DisableParticlesClientRpc();
    }

    [ClientRpc]
    public void EnabledParticlesClientRpc()
    {
        fire.SetActive(true);
        smoke.SetActive(true);
    }
    [ClientRpc]
    public void DisableParticlesClientRpc()
    {
        fire.SetActive(false);
        smoke.SetActive(false);
    }
}
