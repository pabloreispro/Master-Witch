using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Game.UI;
public class CuttingBench : Bench
{
    public ParticleSystem cutParticle;
    public AudioSource sfx;

    bool _wasPlayerInteracting;
    
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
        _Gameplay();
    }

    private void _Gameplay(){
        
        if(_player !=null)
        {
            if(ingredients.Count > 0)
            {
                //GameInterfaceManager.Instance.spaceKey.SetActive(_player != null);
                if(_player.buttonPressed){
                    EnableSFXServerRpc();
                    ChangeVariableServerRpc(true);
                    GetInfoIngredient();
                    Debug.Log("Cutting");
                    Invoke("_ClickedButton", 0.2f);
                }else{
                    ChangeVariableServerRpc(false);
                    
                    _player = null;
                    StopCutParticleServerRpc();
                }
            }
            
        }
        if( (_player!=null) != _wasPlayerInteracting)
        {
            _wasPlayerInteracting = !_wasPlayerInteracting;
            GameInterfaceManager.Instance.eKey.SetActive(_player!=null && _player.isHand.Value);
            GameInterfaceManager.Instance.spaceKey.SetActive(_player!= null && ingredients.Count > 0 && objectInBench == null);
        }
        
        
       
        
        
        
    }

    private void _ClickedButton(){
        
        ChangeVariableServerRpc(false);
        if(_player!=null){
           _player.buttonPressed = false;
        }
        return;
    }
    
    public override void Pick(Player player)
    {
        if (endProgress && player.isHand.Value == false)
        {
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
        PositionBench(interact);
    }
    public void GetInfoIngredient()
    {
        var ingredient = ingredients[ingredients.Count - 1].targetFood.foodPrefab.gameObject;
        cutParticle.GetComponent<ParticleSystemRenderer>().mesh = ingredient.GetComponent<MeshFilter>().sharedMesh;
        cutParticle.GetComponent<ParticleSystemRenderer>().material = ingredient.GetComponent<Renderer>().sharedMaterial;
        PlayCutParticleServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayCutParticleServerRpc()
    {
        PlayCutParticleClientRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    public void StopCutParticleServerRpc()
    {
        StopCutParticleClientRpc();
    }

    [ClientRpc]
    public void PlayCutParticleClientRpc()
    {
        cutParticle.Play();
    }
    [ClientRpc]
    public void StopCutParticleClientRpc()
    {
        cutParticle.Stop();
    }
}
