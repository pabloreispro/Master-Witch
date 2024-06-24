using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Netcode;
using JetBrains.Annotations;
using Game.SO;
using Network;
using System;
using UI;


public enum PlayerState
{
    Idle,
    IdleBasket,
    IdleItem,
    Interact,
    Walking,
    WalkingItem,
    WalkingBasket,
    PuttingBasket
}
public class Player : NetworkBehaviour
{
     [Header("Animation Configs")]
    public PlayerState currentState;
    public Animator animator;

    [Header("Info Player")]
    public int id;
    [SerializeField] MeshRenderer hatRenderer;

    
    [Header("Scriptable Object")]
    public Interactable interact;
    public FoodSO getIngredient;

    [Header("Ingredient Hand")]
    public GameObject assetIngredient; 
    public Transform boneBasket, boneItem;
    public NetworkVariable<bool> stateObjectIngrediente = new NetworkVariable<bool>();
    public bool isHand;
    public bool isHandBasket;

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
        animator = GetComponent<Animator>();
        
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
        isHandBasket = false;
    }

    public void AddItemBasket(FoodSO ingredient)
    {
        if(this.GetComponentInChildren<Tool>().ingredients.Count < basketMax)
        {
            this.GetComponentInChildren<Tool>().ingredients.Add(new RecipeData(ingredient));
        }
    }

    public void ChangeState(PlayerState newState)
    {
        currentState = newState;
    }

    public void OnConnected(Material newMaterial, int id)
    {
        hatRenderer.material = newMaterial;
        this.id = id;

    }

    

    
}
