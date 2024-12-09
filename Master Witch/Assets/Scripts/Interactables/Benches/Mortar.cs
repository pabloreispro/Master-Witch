using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.VFX;
using Game.UI;

public class Mortar : Bench
{
    public VisualEffect smoke;
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
            if(_player.buttonPressed){
                EnableSFXServerRpc();
                ChangeVariableServerRpc(true);
                EnabledParticlesServerRpc();
            }else{
                DisableSFXServerRpc();
                ChangeVariableServerRpc(false);
                _player = null;
                DisableParticlesServerRpc();
            }
        }
        if( (_player!=null) != _wasPlayerInteracting)
        {
            _wasPlayerInteracting = !_wasPlayerInteracting;
            GameInterfaceManager.Instance.eKey.SetActive(_player!=null && _player.isHand.Value);
            //GameInterfaceManager.Instance.spaceKey.SetActive(_player!= null && ingredients.Count > 0 && objectInBench == null);
        }
        
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

    [ServerRpc(RequireOwnership=false)]
    public void EnabledParticlesServerRpc()
    {
        EnabledParticlesClientRpc();
    }
    [ServerRpc(RequireOwnership=false)]
    public void DisableParticlesServerRpc()
    { 
        DisableParticlesClientRpc();
    }

    [ClientRpc]
    public void EnabledParticlesClientRpc()
    {
        smoke.SetBool("isPreparing",true);
    }
    [ClientRpc]
    public void DisableParticlesClientRpc()
    { 
        smoke.SetBool("isPreparing",false);
    }
}
