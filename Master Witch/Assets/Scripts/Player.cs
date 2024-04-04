using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using JetBrains.Annotations;
using Game.SO;

public class Player : NetworkBehaviour
{
    [Header("Info Player")]
    public int id;
    string name;
    Color color;
    
    [Header("Scriptable Object")]
    public Interactable interact;
    public FoodSO ingredient;
    public FoodSO getIngredient;

    [Header("Ingredient Hand")]
    public GameObject assetIngredient;
    public bool stateIngredient;
    public NetworkVariable<bool> stateObjectIngrediente = new NetworkVariable<bool>();
    public bool isHand;

    public void ONetworkSpawn()
    {
        stateObjectIngrediente.Value = false;
    }
    
}
