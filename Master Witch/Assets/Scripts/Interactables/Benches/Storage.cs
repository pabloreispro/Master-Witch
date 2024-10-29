using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.SO;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.Netcode;
using Network;
using Unity.VisualScripting;

public class Storage : Bench
{
    public StorageState currentState;
    public Animator animator;
    public Button[] slots;
    public FoodSO slotSelected;
    public Camera mainCamera;
    public GameObject panelInventory;
    public int indexSlots;
    public bool isActive = false;
    //public GameObject inventory;
    public Player player;

    void Start()
    {
        PanelPosition();
    }


    void PanelPosition(){
        mainCamera = Camera.main;
        Vector3 lookDir = panelInventory.transform.position - mainCamera.transform.position ;
        
        panelInventory.transform.rotation = Quaternion.LookRotation(lookDir);
    }

    public override void Drop(Player player)
    {
        var interact = player.GetComponentInChildren<Ingredient>();
        if(ingredients.Count < 4){
            AddIngredient(interact);
            interact.DestroySelf();
        }
    }

    public void SelectedIngredient(int indexSlots){
        if(player.IsOwner){
            UpdateInventory();
            Time.timeScale = 1;
            SetPlayerItemServerRpc(indexSlots, PlayerNetworkManager.Instance.GetID[player]);
            ingredients.RemoveAt(indexSlots);
            slotSelected = null;
            isActive = false;
            panelInventory.SetActive(false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SetPlayerItemServerRpc(int itemIndex, ulong playerID)
    {
        Debug.Log("Player id: "+ playerID);
        Debug.Log("index id: "+ itemIndex);
        var playerScene = PlayerNetworkManager.Instance.GetPlayer[playerID];
        Debug.Log("PlayerSCene: "+playerScene.id+" name: "+playerScene.name);
        var objectSpawn = Instantiate(ingredients[itemIndex].TargetFood.foodPrefab, new Vector3(playerScene.assetIngredient.transform.position.x, 1.0f, playerScene.assetIngredient.transform.position.z), Quaternion.identity);
        objectSpawn.GetComponent<NetworkObject>().Spawn();
        objectSpawn.GetComponent<NetworkObject>().TrySetParent(playerScene.transform); 
        objectSpawn.GetComponent<Collider>().enabled = false;
        playerScene.SetItemHandClientRpc(objectSpawn);
        playerScene.ChangeState(PlayerState.Interact);
        //RemoveIngredient(ingredients[itemIndex].TargetFood);
    }

    public void Initialize()
    {
        if(panelInventory.activeSelf == false){
            //ingredients.Clear();
            foreach (var item in slots)
            {
                item.interactable = false;
            }
            //ingredients.AddRange(ingredients);
            UpdateInventory();
            panelInventory.SetActive(true);
            PanelPosition();
        }else{
            panelInventory.SetActive(false);
        }
        //OnSlotSelected(0);
        //Active = true;
    }

    void UpdateInventory()
    {
        
        int maxIndex = Mathf.Min(slots.Length, ingredients.Count); 

        for (int i = 0; i < maxIndex; i++)
        {
            if (ingredients.ElementAt(i) != null)
            {
                slots[i].interactable = true;
                slots[i].image.sprite = ingredients[i].TargetFood.imageFood;
            }
        }
        if(maxIndex!=0){
            SelectButton(0);

        }
        isActive = true;
    }

    void SelectButton(int index){
        slots[index].Select();

    }

    [ServerRpc (RequireOwnership = false)]
    public void RepositionServerRpc(Vector3 pos, Quaternion rot)
    {
        RepositionClientRpc(pos, rot);
    }
    [ClientRpc]
    public void RepositionClientRpc(Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;
        //transform.rotation = Quaternion.Euler(0f,180f,0f);
    }


    public enum StorageState
    {
        Open,
        Close
    }
    public void ChangeState(StorageState newState)
    {
        currentState = newState;
        ChangeStateClientRpc(newState);
        AnimatorStorage();
    }

    [ClientRpc]
    private void ChangeStateClientRpc(StorageState newState)
    {
        currentState = newState;
        AnimatorStorage();
    }
    public void AnimatorStorage()
    {
        switch (currentState)
        {
            case StorageState.Open:
                animator.SetBool("Open", true);
                animator.SetBool("Close", false);
            break;
            case StorageState.Close:
                animator.SetBool("Open", false);
                animator.SetBool("Close", true);
            break;
        }
    }
}
