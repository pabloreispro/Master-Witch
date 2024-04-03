using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.VisualScripting;
using Game.SO;
using UnityEngine.Rendering;


public class PlayerMovement : Player
{
    public PlayerInput playerInput;
    public float speed;
    public float speedPlayer;
    CharacterController controller;
    bool groundedPlayer;
    public float distanciaMaxima = 2.0f;
    
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
        if(IsOwner == true){
            RaycastPlayer();
            MovementPlayer();
        }
    }

    public void RaycastPlayer(){
        Ray ray = new Ray(transform.position + new Vector3(0,1.0f, 0), transform.forward);
            RaycastHit hit;
            
            Debug.DrawRay(ray.origin, ray.direction * distanciaMaxima, Color.red);
            
            if (Physics.Raycast(ray, out hit, distanciaMaxima))
            {
                if (hit.collider.gameObject.GetComponent<Interactable>() != null)
                {
                    interact = hit.collider.gameObject.GetComponent<Interactable>();
                
                }else{
                    interact = null;
                }
            }
            else
            {
                interact = null;
            }   
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
                PickDropObject();
            }
        }
    }
    public void PickDropObject(){
        if(isHand){
            if(interact == null){
                DropInteractServerRpc();
                StatusAssetServerRpc(false);
            }
            else{
                interact.DropServerRpc(NetworkObjectId);
                StatusAssetServerRpc(false);
            }
        }else{
            interact.PickServerRpc(NetworkObjectId);
            StatusAssetServerRpc(true);
        }
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

    [ServerRpc(RequireOwnership = false)]
    public void DropInteractServerRpc(){
        var objectSpawn = Instantiate(ingredient.foodPrefab, this.transform.position, Quaternion.identity);
        objectSpawn.GetComponent<NetworkObject>().Spawn(true);
        DropInteractClientRpc();
    }

    [ClientRpc]
    public void DropInteractClientRpc(){
        this.isHand = false;
        this.ingredient = null;
    }

}
