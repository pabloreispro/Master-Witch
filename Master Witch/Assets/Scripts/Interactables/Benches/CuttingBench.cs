using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class CuttingBench : Bench
{
    public ParticleSystem cutParticle;
    private void Start()
    {
        
    }

    private void FixedUpdate()
    {
        _Gameplay();
    }

    private void _Gameplay(){
        if(_player !=null && ingredients.Count > 0){
            if(_player.buttonPressed){
                ChangeVariableServerRpc(true);
                GetInfoIngredient();
                Debug.Log("Cutting");
                Invoke("_ClickedButton", 0.5f);
            }else{
                ChangeVariableServerRpc(false);
                _player = null;
                StopCutParticleClientRpc();
            }
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
        PlayCutParticleClientRpc();
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
