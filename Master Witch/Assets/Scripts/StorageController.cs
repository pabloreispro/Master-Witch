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



public class StorageController : SingletonNetwork<StorageController>
{
    
    public Button[] slots;
    public FoodSO slotSelected;
    public List<FoodSO> storageItems;
    public Camera mainCamera;
    public GameObject panelInventory;
    public int indexSlots;
    Bench bench;
    public Player player;
    //public bool Active { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        Vector3 lookDir = panelInventory.transform.position - mainCamera.transform.position ;
        
        panelInventory.transform.rotation = Quaternion.LookRotation(lookDir);
        bench = GetComponent<Bench>();
        

    }
    public bool isActive = false;
    public void Initialize(List<FoodSO> ingredients)
    {
        if(!isActive){
            storageItems.Clear();
            foreach (var item in slots)
            {
                item.interactable = false;
            }
            storageItems.AddRange(ingredients);
            UpdateInventory();
        }
        //OnSlotSelected(0);
        //Active = true;
    }

    public void OnSlotSelected(int slotIndex)
    {
        if(storageItems.ElementAt(slotIndex) != null) 
        {
            slotSelected = storageItems.ElementAt(slotIndex);
            indexSlots = slotIndex;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectedIngredient(int indexSlots){
        if(player.IsOwner){
            UpdateInventory();
            Time.timeScale = 1;
            SetPlayerItemServerRpc(indexSlots, PlayerNetworkManager.Instance.GetID[player]);
            slotSelected = null;
            panelInventory.SetActive(false);
            isActive = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SetPlayerItemServerRpc(int itemIndex, ulong playerID)
    {
        var playerScene = PlayerNetworkManager.Instance.GetPlayer[playerID];
        var objectSpawn = Instantiate(storageItems[itemIndex].foodPrefab, new Vector3(playerScene.assetIngredient.transform.position.x, 1.0f, playerScene.assetIngredient.transform.position.z), Quaternion.identity);
        objectSpawn.GetComponent<NetworkObject>().Spawn();
        objectSpawn.GetComponent<NetworkObject>().TrySetParent(playerScene.transform); 
        objectSpawn.GetComponent<Collider>().enabled = false;
        playerScene.ChangeState(PlayerState.PickItem);
        playerScene.SetItemHandClientRpc(objectSpawn);
        bench.RemoveIngredient(storageItems[itemIndex]);
    }
    void UpdateInventory()
    {
        
        int maxIndex = Mathf.Min(slots.Length, storageItems.Count); 

        for (int i = 0; i < maxIndex; i++)
        {
            if (storageItems.ElementAt(i) != null)
            {
                slots[i].interactable = true;
                slots[i].image.sprite = storageItems[i].imageFood;
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
}
