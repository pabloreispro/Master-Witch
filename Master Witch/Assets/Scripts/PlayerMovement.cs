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


public class PlayerMovement : Player
{
   

    [Header("Movement Configs")]
    private PlayerInput _playerInput;
    public float speedRotation;
    public float speedPlayer;
    public CharacterController controller;
    public float distanciaMaxima = 2.0f;
    public int numberOfRays = 10;
    private Storage _benchStorage;

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
        _playerInput.PlayerInteract.Storage.started += _InteractStorage;
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

    private void _VerifyStorage(){
        if ((interact as Storage) != null)
        {
            _benchStorage = (interact as Storage);
            _benchStorage.player = this;
        }else if(_benchStorage!=null){
            _benchStorage.player = null;
        }
    }

    private void _RaycastPlayer(){
        float angleStep = 60.0f / (numberOfRays - 1); 

        for (int i = 0; i < numberOfRays; i++)
        {
            float angle = transform.eulerAngles.y - 30 + angleStep * i; 
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

            Ray ray = new Ray(transform.position + new Vector3(0, 1.0f, 0), direction);
            RaycastHit hit;

            Debug.DrawRay(ray.origin, ray.direction * distanciaMaxima, Color.red);

            if (Physics.Raycast(ray, out hit, distanciaMaxima))
            {
                Interactable tempInteract = hit.collider.gameObject.GetComponent<Interactable>();
                if (tempInteract != null)
                {
                    interact = tempInteract;
                    break;
                }
            }
            else
            {
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
            else if(isHand.Value && !isHandBasket.Value){ChangeState(PlayerState.IdleItem);}
            else ChangeState(PlayerState.Idle);
             isMoving.Value=false;
        }

        controller.Move(move* Time.deltaTime * speedPlayer);
    }

    private void _InteractStorage(InputAction.CallbackContext context){
        if(IsOwner && interact as Storage){
            if(context.started){
                interact.GetComponent<Storage>().Initialize();
            }
        }
    }

    private void _Interact(InputAction.CallbackContext context){
        if(IsOwner){
            if(context.started){
                _PickDropObject();
                _ButtonInteraction();
            }
            if(context.canceled){
                (interact as Bench).isPreparing.Value = false;
            }
        }
    }
    private void _ButtonInteraction(){
        if(interact != null && !isHand.Value){
            (interact as Bench).isPreparing.Value = true; 
            Invoke("_ClickedButton", 0.5f);
        }
    }
    private void _ClickedButton(){
        if(!(interact as Bench).isPerformed){
            (interact as Bench).isPreparing.Value = false; 
            return;
        }
    }
    private void _PickDropObject(){
        if(isHand.Value)
        {
            if(interact == null){
                this.GetComponentInChildren<Interactable>().GetComponent<Collider>().enabled = true;
                var obj = this.GetComponentInChildren<Interactable>().GetComponent<NetworkObject>();
                obj.GetComponent<FollowTransform>().targetTransform = null;
                obj.TryRemoveParent();
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
    

    
}
