using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.SO;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.Netcode;

public class StorageController : NetworkBehaviour
{
    
    public Button[] slots;
    public FoodSO slotSelected;
    public List<FoodSO> storageItems;
    public Camera mainCamera;
    public GameObject panelInventory;
    private int indexSlots;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        Vector3 lookDir = panelInventory.transform.position - mainCamera.transform.position ;
        
        panelInventory.transform.rotation = Quaternion.LookRotation(lookDir);
        

        foreach (var item in slots)
        {
            item.interactable = false;
        }

        
        
        UpdateInventory();
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
        if(Input.GetKeyDown(KeyCode.G) && slotSelected != null)
        {
            storageItems.Remove(slotSelected);
            slots[indexSlots].interactable = false;
            UpdateInventory();
            Time.timeScale = 1;
            var player = GetComponent<Bench>().auxPlayer;
            player.isHand = true;
            player.StatusAssetServerRpc(true);
            player.ingredient = slotSelected;
            player.ChangeMeshHandServerRpc();
            panelInventory.SetActive(false);   
        }
        if(Input.GetKeyDown(KeyCode.Q))
        {
            Time.timeScale = 1;
            panelInventory.SetActive(false);
        }
    }
    void UpdateInventory()
    {
        var bench = GetComponent<Bench>();
        storageItems.AddRange(bench.ingredients);
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
