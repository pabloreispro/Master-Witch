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
using Unity.Services.Lobbies.Models;
using Game.UI;


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
    float explosionTime;
    Vector3 explosionVelocity;

    public bool CanMove { get; set; } = true;
    public float SpeedModifier { get; set; } = 1;
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

            if (explosionTime > 0)
            {
                explosionTime -= Time.deltaTime;
                controller.Move(explosionVelocity * Time.deltaTime);
                explosionVelocity = Vector3.Lerp(explosionVelocity, Vector3.zero, Time.deltaTime * 5f); // Diminui gradualmente a for�a
                return;
            }

            if (SceneManager.Instance.isMovementAllowed.Value && CanMove)
            {
                _MovementPlayer();
            }
            else 
            {ChangeState(PlayerState.Idle); isMoving.Value=false;}
            
            _VerifyStorage();
            
        }
    }
    public override void Reposition(Vector3 pos)
    {
        base.Reposition(pos);
        SpeedModifier = 1;
        CanMove = true;
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
                    

                    if(interact as CuttingBench && (interact as CuttingBench)._wasPlayerInteracting && (interact as CuttingBench).ingredients.Count > 0 && (interact as CuttingBench).objectInBench == null)
                    {
                        GameInterfaceManager.Instance.spaceKey.SetActive(true);
                        GameInterfaceManager.Instance.spaceAnim.SetBool("Hold",false);
                    }
                    if(interact as BusenBurner && (interact as BusenBurner)._wasPlayerInteracting && (interact as BusenBurner).ingredients.Count > 0 && (interact as BusenBurner).objectInBench == null)
                    {
                        GameInterfaceManager.Instance.spaceKey.SetActive(true);
                        GameInterfaceManager.Instance.spaceAnim.SetBool("Hold",true);
                    }
                    if(interact as Mortar && (interact as Mortar)._wasPlayerInteracting && (interact as Mortar).ingredients.Count > 0 && (interact as Mortar).objectInBench == null)
                    {
                        GameInterfaceManager.Instance.spaceKey.SetActive(true);
                        GameInterfaceManager.Instance.spaceAnim.SetBool("Hold",true);
                    }
                    
                }
            }
            else
            {
                if (tempInteract != null)
                {
                    if(tempInteract.gameObject.GetComponent<HighlightObj>() != null) 
                    {
                        tempInteract.gameObject.GetComponent<HighlightObj>().RemoveHighlight();
                    }
                    if(tempInteract as Bench)
                    {
                        (tempInteract as Bench)._player = null;
                    }
                }
                GameInterfaceManager.Instance.spaceKey.SetActive(false);
                interact = null;
            }
        }
    }

    private void _MovementPlayer()
{
    // Captura o input do jogador
    Vector2 inputVector = _playerInput.PlayerControl.Movement.ReadValue<Vector2>();
    Vector3 horizontalMove = new Vector3(inputVector.x, 0, inputVector.y).normalized;

    float gravity = -9.81f;
    float verticalVelocity = 0;
    float groundCheckDistance = 0.1f;
    bool isGrounded;

    // Checa se o jogador está no chão
    isGrounded = controller.isGrounded;

    if (isGrounded && verticalVelocity < 0)
    {
        verticalVelocity = -2f; // Mantém o jogador próximo ao chão
    }

    // Aplica gravidade quando no ar
    if (!isGrounded)
    {
        verticalVelocity += gravity * Time.deltaTime;
    }

    // Movimento horizontal
    if (inputVector != Vector2.zero)
    {
        Quaternion targetRotation = Quaternion.LookRotation(horizontalMove);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speedRotation);
        
        if (isHand.Value)
        {
            ChangeState(PlayerState.WalkingItem);
        }
        else
        {
            ChangeState(PlayerState.Walking);
        }

        isMoving.Value = true;
    }
    else
    {
        if (isHand.Value)
        {
            ChangeState(PlayerState.IdleItem);
        }
        else
        {
            ChangeState(PlayerState.Idle);
        }

        isMoving.Value = false;
    }

    // Combina movimento horizontal com gravidade no eixo Y
    Vector3 finalMove = horizontalMove * speedPlayer * SpeedModifier;
    finalMove.y = verticalVelocity;

    // Move o jogador
    controller.Move(finalMove * Time.deltaTime);
}

    public void AddExplosiveForce(float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModifier, float explosionDuration)
    {
        Vector3 direction = (transform.position - explosionPosition).normalized;
        float distance = Vector3.Distance(transform.position, explosionPosition);

        // Calcula a for�a com base na dist�ncia da explos�o
        float force = Mathf.Clamp(1 - (distance / explosionRadius), 0, 1) * explosionForce;

        // Adiciona uma for�a para cima
        explosionVelocity = direction * force + Vector3.up * upwardsModifier;

        explosionTime = explosionDuration; // Reseta o tempo de explos�o
    }
    private void _Interact(InputAction.CallbackContext context){
        if(IsOwner){
            if (!CanDoInput()) return;
            if(context.started){
                _PickDropObject();
            }
        }
    }
    private void _Action(InputAction.CallbackContext context){
        if(IsOwner){
            if (!CanDoInput()) return;
            if(context.started){
                buttonPressed = true;
            }
            if(context.canceled){
                buttonPressed = false;
            }
        }
    }
    public bool CanDoInput()
    {
        if (DialogueSystem.Instance.DialogueIsOpen)
        {
            DialogueSystem.Instance.SkipDialogue();
            return false;
        }
        return true;
    }
    private void _PickDropObject(){
        //PickPutServerRpc();
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
                ChangeState(PlayerState.PickItem);
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
        var interactable = GetComponentInChildren<Interactable>();

        interactable.GetComponent<Collider>().enabled = true;
        var obj = interactable.GetComponent<NetworkObject>();
        obj.GetComponent<FollowTransform>().targetTransform = null;
        obj.GetComponent<Rigidbody>().useGravity = true;
        obj.GetComponent<Rigidbody>().isKinematic = false;
        if (interactable is Ingredient)
        {
            (interactable as Ingredient).DropServerRpc(NetworkObjectId);
        }
        obj.TryRemoveParent();
        ChangeState(PlayerState.DropItem);
    }
}
