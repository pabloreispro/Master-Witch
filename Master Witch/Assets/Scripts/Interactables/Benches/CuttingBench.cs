using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class CuttingBench : Bench
{
    private void Start()
    {
        isPreparing.OnValueChanged += (a,b) => visualEffect[0].SetBool("isPreparing", isPreparing.Value);
    }

    private void FixedUpdate()
    {
        _Gameplay();
    }

    private void _Gameplay(){
        if(_player !=null && ingredients.Count > 0){
            if(_player.buttonPressed){
                ChangeVariableServerRpc(true);
                Debug.Log("Cutting");
                Invoke("_ClickedButton", 0.5f);
            }else{
                ChangeVariableServerRpc(false);
                _player = null;
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
}
