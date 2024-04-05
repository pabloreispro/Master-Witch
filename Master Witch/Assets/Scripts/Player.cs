using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using JetBrains.Annotations;
using Game.SO;

public class Player : NetworkBehaviour
{
    [Header("Info Player")]
    public int id;
    string name;
    Color color;

    
    [Header("Scriptable Object")]
    public Interactable interact;
    public FoodSO ingredient;
    public FoodSO getIngredient;
    public ToolsSO tool;

    [Header("Ingredient Hand")]
    public GameObject assetIngredient;
    public NetworkVariable<bool> stateObjectIngrediente = new NetworkVariable<bool>();
    public bool isHand;

    public void ONetworkSpawn()
    {
        stateObjectIngrediente.Value = false;
    }
    [ServerRpc]
    public void StatusAssetServerRpc(bool has){
        stateObjectIngrediente.Value = has;
        StatusClientRpc(stateObjectIngrediente.Value);
    }
    [ClientRpc]
    public void StatusClientRpc(bool has){
        assetIngredient.SetActive(has);
       
    }

    [ServerRpc]
    public void ChangeMeshHandServerRpc(){
        ChangeMeshHandClientRpc();
    }
    [ClientRpc]
    public void ChangeMeshHandClientRpc(){
        assetIngredient.GetComponent<MeshFilter>().sharedMesh = ingredient.foodPrefab.GetComponent<MeshFilter>().sharedMesh;
        assetIngredient.GetComponent<MeshRenderer>().sharedMaterial = ingredient.foodPrefab.GetComponent<MeshRenderer>().sharedMaterial;
    }
    [ServerRpc]
    public void ChangeMeshHandToolServerRpc(){
        ChangeMeshHandToolClientRpc();
    }
    [ClientRpc]
    public void ChangeMeshHandToolClientRpc(){
        assetIngredient.GetComponent<MeshFilter>().sharedMesh = tool.prefab.GetComponent<MeshFilter>().sharedMesh;
        assetIngredient.GetComponent<MeshRenderer>().sharedMaterial = tool.prefab.GetComponent<MeshRenderer>().sharedMaterial;
    }
}
