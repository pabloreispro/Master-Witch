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
using UnityEngine.VFX;


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
    public TestAnimator a;
    public VisualEffect footstepsVFX;
    public NetworkVariable<bool> isMoving = new NetworkVariable<bool>(false,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);
    
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
    public NetworkVariable<bool> isHand = new NetworkVariable<bool>();
    public NetworkVariable<bool> isHandBasket = new NetworkVariable<bool>();

    public List<Bench> bench = new List<Bench>();
    [Header("Basket Config")]
    public int basketMax;

    public bool isPressingInterect;
    
    public override void OnNetworkSpawn()
    {
        if(IsLocalPlayer)
        {
            NewCamController.Instance.target = this.transform;
        }
    }
    void Start()
    {
        isMoving.OnValueChanged += (a,b)=>footstepsVFX.SetBool("isMoving",isMoving.Value);
        animator = GetComponent<Animator>();
        
        /*foreach (Bench item in FindObjectsOfType<Bench>() )
        {
            if(item.benchType == BenchType.Storage )
            {
                bench.Add(item);
            }
        }*/

        bench.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
        
    }

    private void Update() {
        _VerifyHandServerRpc();
    }

    [ServerRpc (RequireOwnership = false)]
    private void _VerifyHandServerRpc(){
        if(this.GetComponentInChildren<Interactable>()){
            isHand.Value = true;
        }else{
            isHand.Value = false;
        }
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
        isHandBasket.Value = false;
    }

    /*public void AddItemBasket(FoodSO ingredient)
    {
        if(this.GetComponentInChildren<Tool>().ingredients.Count < basketMax)
        {
            this.GetComponentInChildren<Tool>().ingredients.Add(new RecipeData(ingredient));
        }
    }*/

    public void ChangeState(PlayerState newState)
    {
        currentState = newState;
        ChangeStateClientRpc(newState);
        AnimationController();
    }

    [ClientRpc]
    private void ChangeStateClientRpc(PlayerState newState)
    {
       
        currentState = newState;
        AnimationController();
    }

    public void AnimationController()
    {
        switch(currentState)
        {
            case PlayerState.Idle:
                animator.SetBool("IsWalking", false);
                animator.SetBool("IdleItem", false);
                animator.SetBool("IdleBasket", false);
                animator.SetBool("IsWalkingBasket", false);
                animator.SetBool("IsWalkingItem", false);
                animator.SetBool("PutInBasket",false);
            break;
            case PlayerState.IdleBasket:
                animator.SetBool("IsWalkingBasket", false);
                animator.SetBool("PutInBasket",false);
                animator.SetBool("IdleBasket", true);
                
            break;
            case PlayerState.IdleItem:
                animator.SetBool("IsWalkingItem", false);
                animator.SetBool("IdleItem", true);
                break;
            case PlayerState.Interact:
                a.SetTrigger("Interact");
            break;
            case PlayerState.Walking:
                animator.SetBool("IsWalking", true);
            break;
            case PlayerState.WalkingItem:
                animator.SetBool("IsWalkingItem", true);
            break;
            case PlayerState.WalkingBasket:
                animator.SetBool("IsWalkingBasket", true);
            break;
            case PlayerState.PuttingBasket:
                animator.SetBool("PutInBasket",true);
            break;
            
        }
    }

    public void OnConnected(Material newMaterial, int id)
    {
        hatRenderer.material = newMaterial;
        this.id = id;

    }

    [ClientRpc]
    public void SetBasketHandClientRpc(NetworkObjectReference tool)
    {
        if(tool.TryGet(out NetworkObject basket) )
        {
            FollowTransform followTransform = basket.GetComponent<FollowTransform>();
            followTransform.targetTransform = boneBasket;
        }

    }
    [ClientRpc]
    public void SetItemHandClientRpc(NetworkObjectReference item)
    {
        if(item.TryGet(out NetworkObject obj) )
        {
            FollowTransform followTransform = obj.GetComponent<FollowTransform>();
            followTransform.targetTransform = boneItem;
        }

    }

   
        
    
    
        
    

    

    
}
