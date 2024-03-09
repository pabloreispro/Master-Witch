using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Interactable : NetworkBehaviour
{
    public bool isBench;
    public bool isIngredient;
    public SyncAtivNetworkVariable sync;
    private void OnControllerColliderHit(ControllerColliderHit hit) {
        if(hit.gameObject.CompareTag("Bench")){
            Bench.instance.foodAsset = gameObject.transform.GetChild(0).gameObject;
        }
        if(hit.gameObject.CompareTag("Ingredient")){
            sync.SetEstado(true);
            
        }
    }
}
