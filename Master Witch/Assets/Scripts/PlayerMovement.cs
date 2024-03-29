using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.VisualScripting;


public class PlayerMovement : Player
{
    public PlayerInput playerInput;
    public float speed;
    public float speedPlayer;
    CharacterController controller;
    bool groundedPlayer;
    public ServerManager spawnManager;
    
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = new PlayerInput();
        playerInput.PlayerControl.Enable();
        playerInput.PlayerInteract.Enable();
        playerInput.PlayerInteract.Interaction.performed += Interact;
        //inputs para buttons 
        //na funcao precisa colocar InputAction.CallbackContext context
        //e o context funciona como um ativador
        //playerInput.PlayerControl.Movement.performed += MovementPlayer;
    }

    void FixedUpdate()
    {
        if(IsOwner == true)
            MovementPlayer();
    }

    void MovementPlayer(){
        Vector2 inputVector = playerInput.PlayerControl.Movement.ReadValue<Vector2>();

        Vector3 move = new Vector3(inputVector.x, 0, inputVector.y).normalized;
        
        if(inputVector!=new Vector2(0,0)){
            Quaternion r = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, r, speed);
        }
        controller.Move(move* Time.deltaTime * speedPlayer);
    }

    void Interact(InputAction.CallbackContext context){
        if(IsOwner){
            if(context.performed){
                if(!isHand.Value){
                    interact.PickServerRpc(NetworkObjectId);
                    
                    InteractServerRpc(stateObject);
                }else{
                    
                    interact.DropServerRpc(NetworkObjectId);
                }
            }
        }
    }
    
    /*[ServerRpc]
    void DropServerRpc(){
        interact.Drop(this);
        DropClientRpc();
    }
    [ClientRpc]
    void DropClientRpc(){
        interact.Drop(this);
    }*/
    
    [ServerRpc]
    public void InteractServerRpc(bool has){
        stateObjectIngrediente.Value = has;
        isHand.Value = has;
        SetPickObjectClientRpc(stateObjectIngrediente.Value);
    }

    [ClientRpc]
    public void SetPickObjectClientRpc(bool has){
        assetIngredient.SetActive(has);
    }

}
