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
using UnityEngine.Rendering;

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
    public Player player;

    public GameObject[] slotsStorage;
    public AudioSource sfx, sfx2;

    [ClientRpc]
    public void EnableSFXClientRpc(){
        sfx.Play();
    }

    [ClientRpc]
    public void DisableSFXClientRpc(){
        sfx2.Play();
    }

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
        if(ingredients.Count <= 4){
            AddIngredient(interact);
            UpdateInventory();
            //interact.DestroySelf();
            foreach (var item in slotsStorage)
            {
                if(item.transform.childCount == 0)
                {
                    AddItemToStorageTransform(item, interact.gameObject);
                    break;
                }
            }
        }
    }

    public void SelectedIngredient(int indexSlots){
        if(player.IsOwner){
            Time.timeScale = 1;
            ingredients.RemoveAt(indexSlots);
            SetPlayerItemServerRpc(indexSlots, PlayerNetworkManager.Instance.GetID[player]);
            slotSelected = null;
            isActive = false;
            panelInventory.SetActive(false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SetPlayerItemServerRpc(int itemIndex, ulong playerID)
    {
        var playerScene = PlayerNetworkManager.Instance.GetPlayer[playerID];
        var objSelected = slotsStorage[itemIndex].transform.GetChild(0).GetComponent<NetworkObject>();
        objSelected.GetComponent<NetworkObject>().TrySetParent(playerScene.transform);
        objSelected.transform.localScale = new Vector3(1,1,1);
        objSelected.transform.rotation = Quaternion.identity;
        objSelected.gameObject.GetComponent<Rigidbody>().useGravity = false;
        objSelected.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        playerScene.SetItemHandClientRpc(objSelected);
        playerScene.ChangeState(PlayerState.Interact);
        //RemoveIngredient(itemIndex);
        UpdateInventory();
        //RemoveIngredient(ingredients[itemIndex].TargetFood);
    }

    void AddItemToStorageTransform(int index, GameObject interact) => AddItemToStorageTransform(slotsStorage[index], interact);
    void AddItemToStorageTransform(GameObject slot, GameObject item)
    {
        
        item.GetComponent<FollowTransform>().targetTransform = null;
        item.GetComponent<NetworkObject>().TrySetParent(slot.transform);
        item.transform.rotation = new Quaternion(90, 90, 90, 0);
        item.transform.position = slot.transform.position;
        item.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        var rb = item.GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }
    void OrganizeItemTransforms()
    {
        for (int i = slotsStorage.Length - 1; i >= 0; i--)
        {
            if (slotsStorage[i].transform.childCount > 0 && i > 0)
            {
                if (slotsStorage[i - 1].transform.childCount > 0)
                    continue;
                AddItemToStorageTransform(i - 1, slotsStorage[i].transform.GetChild(0).gameObject);
            }
        }
    }
    public void Initialize()
    {
        if(panelInventory.activeSelf == false){
            EnableSFXClientRpc();
            //ingredients.AddRange(ingredients);
            foreach (var item in slots)
            {
                item.interactable = false;
            }
            UpdateInventory();
            panelInventory.SetActive(true);
            PanelPosition();
        }
        //OnSlotSelected(0);
        //Active = true;
    }

    public void DisableStorage(){
        panelInventory.SetActive(false);
        DisableSFXClientRpc();
    }

    void RemoveIngredient(int index){
        
    }

    void UpdateInventory()
    {
        //int maxIndex = Mathf.Min(slots.Length, ingredients.Count); 

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < ingredients.Count && ingredients.ElementAt(i) != null)
            {
                slots[i].interactable = true;
                slots[i].image.sprite = ingredients[i].TargetFood.imageFood;
            }
            else
            {
                slots[i].interactable = false;
                slots[i].image.sprite = null;
            }
        }
        if (ingredients.Count > 0)
        {
            SelectButton(0);
        }
        OrganizeItemTransforms();
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
        ChangeStateServerRpc(newState);
        AnimatorStorage();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeStateServerRpc(StorageState newState)
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
