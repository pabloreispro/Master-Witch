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
using Unity.VisualScripting;



public enum PlayerState
{
    Idle,
    IdleItem,
    Interact,
    Walking,
    WalkingItem,
    PickItem,
    DropItem
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
    [SerializeField] SpriteRenderer circleRenderer;

    
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
    
    public AudioSource walkleft, walkright, WalkComp, pick;
    
    public override void OnNetworkSpawn()
    {
        
    }
    void Start()
    {

        if (IsLocalPlayer)
        {
            GetComponent<AudioListener>().enabled = true;
            NewCamController.Instance.target = this.transform;
        }

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
        _VerifyHand();
    }

    [ServerRpc (RequireOwnership = false)]
    private void _ChangeisHandServerRpc(bool state)
    {
        isHand.Value = state;
    }

    private void _VerifyHand(){
        if(this.GetComponentInChildren<Interactable>()){
            _ChangeisHandServerRpc(true);
        }else{
            _ChangeisHandServerRpc(false);
        }
    }
    

    
    [ServerRpc (RequireOwnership = false)]
    public void RepositionServerRpc(Vector3 pos)
    {
        RepositionClientRpc(pos);
    }
    [ClientRpc]
    public void RepositionClientRpc(Vector3 pos) => Reposition(pos);

    public virtual void Reposition(Vector3 pos)
    {
        GetComponent<PlayerMovement>().controller.enabled = false;
        transform.position = pos;
        transform.rotation = Quaternion.identity;
        transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        GetComponent<PlayerMovement>().controller.enabled = true;
        isHandBasket.Value = false;
    }
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
                animator.SetBool("IsWalkingItem", false);
                animator.SetBool("IsInteracting",false);
                animator.SetBool("PickItem",false);
                animator.SetBool("DropItem",false);
                
            break;
            case PlayerState.IdleItem:
                animator.SetBool("IsWalkingItem", false);
                animator.SetBool("IdleItem", true);
                
                break;
            case PlayerState.Interact:
                animator.SetBool("IsInteracting",true);
                
            break;
            case PlayerState.Walking:
                animator.SetBool("IsWalking", true);
                
            break;
            case PlayerState.WalkingItem:
                animator.SetBool("IsWalkingItem", true);
                
            break;
            case PlayerState.PickItem:
                animator.SetBool("PickItem",true);
            break;
            case PlayerState.DropItem:
                animator.SetTrigger("DropItem");
                animator.SetBool("IdleItem",false);
                animator.SetBool("IsWalkingItem",false);
            break;
            
        }
    }

    

    public void OnConnected(Material newMaterial, int id)
    {
        //hatRenderer.material = newMaterial;
        circleRenderer.color = newMaterial.color;
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
        if(item.TryGet(out NetworkObject obj))
        {
            FollowTransform followTransform = obj.GetComponent<FollowTransform>();
            followTransform.targetTransform = boneItem;
        }
    }

    [ServerRpc(RequireOwnership =false)]
    public void WalkLeftServerRpc(){
        Debug.Log("Animation walk left");
        WalkLeftClientRpc();
    }

    [ClientRpc]
    public void WalkLeftClientRpc(){
        walkleft.Play();
    }

    [ServerRpc(RequireOwnership =false)]
    public void WalkRightServerRpc(){
        WalkRightClientRpc();
    }

    [ClientRpc]
    public void WalkRightClientRpc(){
        walkright.Play();
    }

    [ServerRpc(RequireOwnership =false)]
    public void WalkCompServerRpc(){
        WalkCompClientRpc();
    }

    [ClientRpc]
    public void WalkCompClientRpc(){
        WalkComp.Play();
    }

    [ServerRpc(RequireOwnership =false)]
    public void PickPutServerRpc(){
        PickPutClientRpc();
    }
        
    [ClientRpc]
    public void PickPutClientRpc(){
        pick.Play();
    }
    
}
