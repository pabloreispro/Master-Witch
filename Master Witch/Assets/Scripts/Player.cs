using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Netcode;
using JetBrains.Annotations;
using Game.SO;
using Network;
using System;

public class Player : NetworkBehaviour
{
    [Header("Info Player")]
    public int id;
    string name;
    [SerializeField] MeshRenderer hatRenderer;

    
    [Header("Scriptable Object")]
    public Interactable interact;
    public FoodSO ingredient;
    public FoodSO getIngredient;
    public ToolsSO tool;

    [Header("Ingredient Hand")]
    public GameObject assetIngredient;
    public NetworkVariable<bool> stateObjectIngrediente = new NetworkVariable<bool>();
    public bool isHand;
    public List<FoodSO> recipeIngredients = new List<FoodSO>();

    public List<Bench> bench = new List<Bench>();
    [Header("Basket Config")]
    public int basketMax;
    public List <FoodSO> ingredientsBasket = new List<FoodSO>();

    
    void Start()
    {
        
        foreach (Bench item in FindObjectsOfType<Bench>() )
        {
            if(item.benchType == BenchType.Storage )
            {
                bench.Add(item);
            }
        }

        bench.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
        
    }

    
    [ServerRpc (RequireOwnership = false)]
    public void RepositionServerRpc(Vector3 pos)
    {
        RepositionClientRpc(pos);
    }
    [ClientRpc]
    public void RepositionClientRpc(Vector3 pos)
    {
        
        this.GetComponent<PlayerMovement>().controller.enabled = false;
        this.transform.position = pos;
        this.transform.rotation = Quaternion.identity;
        this.transform.rotation = Quaternion.Euler(0f,180f,0f);
        this.GetComponent<PlayerMovement>().controller.enabled = true;
    }
    

    
    
    
    [ServerRpc(RequireOwnership = false)]
    public void StatusAssetServerRpc(bool has){
        stateObjectIngrediente.Value = has;
        StatusClientRpc(stateObjectIngrediente.Value);
    }
    [ClientRpc]
    public void StatusClientRpc(bool has){
        assetIngredient.SetActive(has);
       
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeMeshHandServerRpc(){
        ChangeMeshHandClientRpc();
    }
    [ClientRpc]
    public void ChangeMeshHandClientRpc(){
        assetIngredient.GetComponent<MeshFilter>().sharedMesh = ingredient.foodPrefab.GetComponent<MeshFilter>().sharedMesh;
        assetIngredient.GetComponent<MeshRenderer>().sharedMaterial = ingredient.foodPrefab.GetComponent<MeshRenderer>().sharedMaterial;
        assetIngredient.transform.localScale = ingredient.foodPrefab.transform.localScale;
        assetIngredient.transform.rotation = ingredient.foodPrefab.transform.rotation;
    }
    [ServerRpc(RequireOwnership = false)]
    public void ChangeMeshHandToolServerRpc(){
        ChangeMeshHandToolClientRpc();
    }
    [ClientRpc]
    public void ChangeMeshHandToolClientRpc(){
        assetIngredient.GetComponent<MeshFilter>().sharedMesh = tool.prefab.GetComponent<MeshFilter>().sharedMesh;
        assetIngredient.GetComponent<MeshRenderer>().sharedMaterial = tool.prefab.GetComponent<MeshRenderer>().sharedMaterial;
        assetIngredient.transform.localScale = tool.prefab.transform.localScale;
        assetIngredient.transform.rotation = tool.prefab.transform.rotation;
    }

    public void AddItemBasket(FoodSO ingredient)
    {
        if(ingredientsBasket.Count < basketMax)
        {
            ingredientsBasket.Add(ingredient);
            Debug.Log("Add");
        }
    }

    public void OnConnected(Material newMaterial)
    {
        hatRenderer.material = newMaterial;
    }
}
