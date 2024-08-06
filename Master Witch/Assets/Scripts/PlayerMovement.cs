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
    public PlayerInput playerInput;
    public float speed;
    public float speedPlayer;
    public CharacterController controller;
    bool groundedPlayer;
    public float distanciaMaxima = 2.0f;
    public int numberOfRays = 10;
    Storage benchStorage;
    


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        controller.enabled = true;
    }
    
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = new PlayerInput();
        playerInput.PlayerControl.Enable();
        playerInput.PlayerInteract.Enable();
        playerInput.PlayerInteract.Interaction.started += Interact;
        playerInput.PlayerInteract.Interaction.performed += Interact;
        playerInput.PlayerInteract.Interaction.canceled += Interact;
        playerInput.PlayerInteract.Storage.started += InteractStorage;
        //inputs para buttons 
        //na funcao precisa colocar InputAction.CallbackContext context
        //e o context funciona como um ativador
        //playerInput.PlayerControl.Movement.performed += MovementPlayer;
    }
    

    void FixedUpdate()
    {
        if(IsOwner == true){
            
            RaycastPlayer();

            if(SceneManager.Instance.isMovementAllowed.Value)
            {
                MovementPlayer();
            }
            else 
            {ChangeState(PlayerState.Idle); isMoving.Value=false;}
            
            VerifyStorage();
            
        }
    }

    public void VerifyStorage(){
        if ((interact as Storage) != null)
        {
            benchStorage = (interact as Storage);
            benchStorage.player = this;
        }else if(benchStorage!=null){
            benchStorage.player = null;
        }
    }

    public void RaycastPlayer(){
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

    void MovementPlayer(){
        Vector2 inputVector = playerInput.PlayerControl.Movement.ReadValue<Vector2>();
        
        Vector3 move = new Vector3(inputVector.x, 0, inputVector.y).normalized;
        
        float gravity = -9.81f; 
        float verticalVelocity = 0; 

        verticalVelocity += gravity * Time.deltaTime;

        move.y += verticalVelocity;
        
        if(inputVector!=new Vector2(0,0)){
            
            Quaternion r = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, r, speed);
            
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

    void InteractStorage(InputAction.CallbackContext context){
        if(IsOwner && interact as Storage){
            if(context.started){

                interact.GetComponent<Storage>().Initialize();
                
            }
        }
    }

    void Interact(InputAction.CallbackContext context){
        if(IsOwner){
            if(context.started){
                PickDropObject();
            }
            if(context.performed){
                isPressingInterect = true;
                Debug.Log("Botao pressionado");
            }else{
                isPressingInterect = false;
            }
        }
    }
    public void PickDropObject(){
        if(isHand.Value)
        {
            if(interact == null){
                this.GetComponentInChildren<Interactable>().GetComponent<Collider>().enabled = true;
                var obj = this.GetComponentInChildren<Interactable>().GetComponent<NetworkObject>();
                obj.GetComponent<FollowTransform>().targetTransform = null;
                obj.TryRemoveParent();
                isHand.Value = false;
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
