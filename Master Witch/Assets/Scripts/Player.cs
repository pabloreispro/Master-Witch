using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using JetBrains.Annotations;
using Game.SO;

public class Player : NetworkBehaviour
{
    public int id;
    string name;
    Color color;
    
    public Interactable interact;
    public GameObject assetIngredient;
    public FoodSO ingredient;
    public NetworkVariable<bool> stateObjectIngrediente = new NetworkVariable<bool>();
    public bool isHand;

    public void ONetworkSpawn()
    {
        stateObjectIngrediente.Value = false;
    }
    public void ResetStatus(bool has){
        interact = null;
        ingredient = null;
        isHand = has;
    }
}
