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
            SetPlayerItemServerRpc(indexSlots);
            slotSelected = null;
            panelInventory.SetActive(false);
        }
    }

    [ServerRpc(RequireOwnership =false)]
    void SetPlayerItemServerRpc(int itemIndex)
    {
        SetPlayerItemClientRpc(itemIndex);
        bench.RemoveIngredient(storageItems[itemIndex]);
    }
    [ClientRpc]
    void SetPlayerItemClientRpc(int itemIndex)
    {
        Debug.Log("peguei o item");
        Debug.Log("Player in storage: "+player.id);

        if(IsServer){
            var objectSpawn = Instantiate(storageItems[itemIndex].foodPrefab, new Vector3(player.assetIngredient.transform.position.x, 1.0f, player.assetIngredient.transform.position.z), Quaternion.identity);
            objectSpawn.GetComponent<NetworkObject>().Spawn();
            objectSpawn.GetComponent<NetworkObject>().TrySetParent(player.transform);
        }

        
        /*player.StatusAssetServerRpc(true);
        player.ingredient = storageItems[itemIndex];
        player.ChangeMeshHandServerRpc();*/
        
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
