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
    public Player player;

    public GameObject[] slotsStorage;

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
            //interact.DestroySelf();
            foreach (var item in slotsStorage)
            {
                if(item.transform.childCount == 0)
                {
                    interact.GetComponent<FollowTransform>().targetTransform = null;
                    interact.GetComponent<NetworkObject>().TrySetParent(item.transform);
                    interact.gameObject.transform.rotation = new Quaternion(90,90,90,0);
                    interact.gameObject.transform.position = item.transform.position;
                    interact.gameObject.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
                    interact.gameObject.GetComponent<Rigidbody>().useGravity = false;
                    interact.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                    break;
                }
            }
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
        var objSelected = slotsStorage[itemIndex].transform.GetChild(0).GetComponent<NetworkObject>();
        objSelected.GetComponent<NetworkObject>().TrySetParent(playerScene.transform);
        objSelected.transform.localScale = new Vector3(1,1,1);
        objSelected.transform.rotation = Quaternion.identity;
        objSelected.gameObject.GetComponent<Rigidbody>().useGravity = false;
        objSelected.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        playerScene.SetItemHandClientRpc(objSelected);
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
        }
        //OnSlotSelected(0);
        //Active = true;
    }

    public void DisableStorage(){
        panelInventory.SetActive(false);
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
