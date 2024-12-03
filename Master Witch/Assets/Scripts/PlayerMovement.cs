using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.VisualScripting;
using Game.SO;
using UnityEngine.Rendering;
using Network;
using System.Linq;
using System;
using Unity.Multiplayer.Tools.NetStatsMonitor;
using UI;
using Game.SceneGame;


public class PlayerMovement : Player
{
    [Header("Movement Configs")]
    private PlayerInput _playerInput;
    private Interactable tempInteract;
    public float speedRotation;
    public float speedPlayer;
    public CharacterController controller;
    public float distanciaMaxima = 2.0f;
    public int numberOfRays = 10;
    private Storage _benchStorage;
    public bool buttonPressed;

    

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        controller.enabled = true;
    }
    
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        _playerInput = new PlayerInput();
        _playerInput.PlayerControl.Enable();
        _playerInput.PlayerInteract.Enable();
        _playerInput.PlayerInteract.Interaction.started += _Interact;
        _playerInput.PlayerInteract.Interaction.canceled += _Interact;
        _playerInput.PlayerInteract.Action.started += _Action;
        _playerInput.PlayerInteract.Action.canceled += _Action;
        //_playerInput.PlayerInteract.Interaction.started += _Interact;
        //inputs para buttons 
        //na funcao precisa colocar InputAction.CallbackContext context
        //e o context funciona como um ativador
        //playerInput.PlayerControl.Movement.performed += MovementPlayer;
    }
    

    void FixedUpdate()
    {
        if(IsOwner == true){
            
            _RaycastPlayer();

            if(SceneManager.Instance.isMovementAllowed.Value)
            {
                _MovementPlayer();
            }
            else 
            {ChangeState(PlayerState.Idle); isMoving.Value=false;}
            
            _VerifyStorage();
            
        }
    }

    private void _VerifyStorage()
    {
        if (interact is Storage storage)
        {
            
            if(interact as Storage && isHand.Value == false){
                interact.GetComponent<Storage>().Initialize();
            }
            _benchStorage = storage;
            _benchStorage.ChangeState(Storage.StorageState.Open);
            _benchStorage.player = this;
            
        }
        else
        {
            if (_benchStorage != null)
            {
                _benchStorage.DisableStorage();
                _benchStorage.ChangeState(Storage.StorageState.Close);
                _benchStorage.player = null;
                _benchStorage = null; 
            }
        }
    }

    private void _RaycastPlayer(){
        float angleStep = 60.0f / (numberOfRays - 1); 

        for (int i = 0; i < numberOfRays; i++)
        {
            float angle = transform.eulerAngles.y - 30 + angleStep * i; 
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

            Ray ray = new Ray(transform.position + new Vector3(0, 0.5f, 0), direction);
            RaycastHit hit;
            
            Debug.DrawRay(ray.origin, ray.direction * distanciaMaxima, Color.red);

            if (Physics.Raycast(ray, out hit, distanciaMaxima))
            {
                tempInteract = hit.collider.gameObject.GetComponent<Interactable>();
                if (tempInteract != null)
                {
                    if(tempInteract.gameObject.GetComponent<HighlightObj>() != null) tempInteract.gameObject.GetComponent<HighlightObj>().Highlight();
                    interact = tempInteract;
                    if(interact as Bench)
                        (interact as Bench)._player = this;
                    break;
                }
            }
            else
            {
                if (tempInteract != null)
                {
                    if(tempInteract.gameObject.GetComponent<HighlightObj>() != null) tempInteract.gameObject.GetComponent<HighlightObj>().RemoveHighlight();
                }
                
                interact = null;
            }
        }
    }

    private void _MovementPlayer(){
        
        Vector2 inputVector = _playerInput.PlayerControl.Movement.ReadValue<Vector2>();
        
        Vector3 move = new Vector3(inputVector.x, 0, inputVector.y).normalized;
        
        float gravity = -9.81f; 
        float verticalVelocity = 0; 

        verticalVelocity += gravity * Time.deltaTime;

        move.y += verticalVelocity;
        
        if(inputVector!=new Vector2(0,0)){
            
            Quaternion r = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, r, speedRotation);
            
            if(isHand.Value && isHandBasket.Value){ChangeState(PlayerState.WalkingBasket);}
            else if(isHand.Value && !isHandBasket.Value){ChangeState(PlayerState.WalkingItem);}
            else ChangeState(PlayerState.Walking);
            isMoving.Value=true;
            
        }
        else 
        {
            if(isHand.Value && isHandBasket.Value){ChangeState(PlayerState.IdleBasket);}
            if(isHand.Value && !isHandBasket.Value){ChangeState(PlayerState.IdleItem);}
            else ChangeState(PlayerState.Idle);
            isMoving.Value=false;
            WalkClientRpc();
        }

        controller.Move(move* Time.deltaTime * speedPlayer);
    }

    private void _Interact(InputAction.CallbackContext context){
        if(IsOwner){
            if(context.started){
                _PickDropObject();
            }
        }
    }
    private void _Action(InputAction.CallbackContext context){
        if(IsOwner){
            if(context.started){
                buttonPressed = true;
            }
            if(context.canceled){
                buttonPressed = false;
            }
        }
    }
    private void _PickDropObject(){
        PickPutClientRpc();
        if(isHand.Value)
        {
            if(interact == null){
                DropItemHandServerRpc();
                
            }
            else
            {
                interact.DropServerRpc(NetworkObjectId); 
            }
        }
        else
        {
            if(interact != null){
                interact.PickServerRpc(NetworkObjectId);
                ChangeState(PlayerState.Interact);
            }
        }
    }

    [ServerRpc (RequireOwnership = false)]
    public void DropItemHandServerRpc()
    {
        DropItemHandClientRpc();
    }
    [ClientRpc]
    public void DropItemHandClientRpc()
    {
        this.GetComponentInChildren<Interactable>().GetComponent<Collider>().enabled = true;
        var obj = this.GetComponentInChildren<Interactable>().GetComponent<NetworkObject>();
        obj.GetComponent<FollowTransform>().targetTransform = null;
        obj.GetComponent<Rigidbody>().useGravity = true;
        obj.GetComponent<Rigidbody>().isKinematic = false;
        obj.TryRemoveParent();
    }
    

    
}
