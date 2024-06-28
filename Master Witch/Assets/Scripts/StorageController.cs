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
    public void Initialize(List<FoodSO> ingredients)
    {
        storageItems.Clear();
        foreach (var item in slots)
        {
            item.interactable = false;
        }
        storageItems.AddRange(ingredients);
        UpdateInventory();
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
        if(IsOwner){
            if(Input.GetKeyDown(KeyCode.G) && slotSelected != null)
            {            
                //Active = false;
            }
            if(Input.GetKeyDown(KeyCode.Q))
            {
                Time.timeScale = 1;
                panelInventory.SetActive(false);
                //Active = false;
            }
        }
    }

    public void SelectedIngredient(int indexSlots){
        if(player.IsOwner){
            UpdateInventory();
            Time.timeScale = 1;
            SetPlayerItemServerRpc(indexSlots, PlayerNetworkManager.Instance.GetID[player]);
            slotSelected = null;
            //panelInventory.SetActive(false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SetPlayerItemServerRpc(int itemIndex, ulong playerID)
    {
        Debug.Log("Player id: "+ playerID);
        Debug.Log("index id: "+ itemIndex);
        var playerScene = PlayerNetworkManager.Instance.GetPlayer[playerID];
        Debug.Log("PlayerSCene: "+playerScene.id+" name: "+playerScene.name);
        var objectSpawn = Instantiate(storageItems[itemIndex].foodPrefab, new Vector3(playerScene.assetIngredient.transform.position.x, 1.0f, playerScene.assetIngredient.transform.position.z), Quaternion.identity);
        objectSpawn.GetComponent<NetworkObject>().Spawn();
        objectSpawn.GetComponent<NetworkObject>().TrySetParent(playerScene.transform); 
        playerScene.SetItemHandClientRpc(objectSpawn);
        playerScene.ChangeState(PlayerState.Interact);
        playerScene.isHand.Value = true; 
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
    }
}
