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
    public FoodSO getIngredient;

    [Header("Ingredient Hand")]
    public GameObject assetIngredient;
    public NetworkVariable<bool> stateObjectIngrediente = new NetworkVariable<bool>();
    public bool isHand;

    public List<Bench> bench = new List<Bench>();
    [Header("Basket Config")]
    public int basketMax;

    
    public override void OnNetworkSpawn()
    {
        if(IsLocalPlayer)
        {
            NewCamController.Instance.target = this.transform;
        }
    }
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
        
        GetComponent<PlayerMovement>().controller.enabled = false;
        transform.position = pos;
        transform.rotation = Quaternion.identity;
        transform.rotation = Quaternion.Euler(0f,180f,0f);
        GetComponent<PlayerMovement>().controller.enabled = true;
        //StatusAssetServerRpc(false);
        isHand = false;
    }

    public void AddItemBasket(FoodSO ingredient)
    {
        if(this.GetComponentInChildren<Tool>().ingredients.Count < basketMax)
        {
            this.GetComponentInChildren<Tool>().ingredients.Add(ingredient);
        }
    }

    public void OnConnected(Material newMaterial, int id)
    {
        hatRenderer.material = newMaterial;
        this.id = id;
    }
}
