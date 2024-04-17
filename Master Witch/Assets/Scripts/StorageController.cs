using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.SO;
using UnityEngine;
using UnityEngine.UI;
using System;

public class StorageController : MonoBehaviour
{
    public Button g;
    public Button[] slots;
    public FoodSO slotSelected;
    public List<FoodSO> storageItems;

    // Start is called before the first frame update
    void Start()
    {
        

        //slots.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));

        var Bench = GetComponent<Bench>();
        storageItems.AddRange(Bench.ingredients);
        for (int i = 0; i < Bench.ingredients.Count; i++)
        {
            slots[i].onClick.AddListener(() => OnSlotSelected(i));
        }
    }

    void OnSlotSelected(int slotIndex)
    {
        if(storageItems.ElementAt(slotIndex) != null)
        slotSelected = storageItems.ElementAt(slotIndex);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
