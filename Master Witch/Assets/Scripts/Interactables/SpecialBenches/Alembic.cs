using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class Alembic : Bench
{
    public List<ToolsSO> _toolInBench = new();
    
    public AudioSource sfx;

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

    private void FixedUpdate() {
        _Special();
    }

    private void _Special(){
        if(_toolInBench.Count>0 && ingredients.Count > 0){
            ChangeVariableServerRpc(true);
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
            _toolInBench.Clear();          
            Reset();*/
            
            DisableSFXServerRpc();
            objectInBench.GetComponentInChildren<NetworkObject>().TrySetParent(player.transform);
            player.SetItemHandClientRpc(objectInBench);
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
                AddIngredient(i);
                progress();
            break;
            case Tool t when t.tool.benchType == benchType:
                EnableSFXServerRpc();         
                _toolInBench.Add(t.tool);
            break;
        }
        interact.DestroySelf();
    }
}
