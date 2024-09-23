using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.SO;
using Unity.Netcode;

public class Ingredient : Interactable
{
    public FoodSO food;
    public List<RecipeData> itemsUsed = new List<RecipeData>();

    private void Start() {
         this.GetComponent<Collider>().enabled = false;
    }

    public override void Pick(Player player)
    {
        this.GetComponent<Collider>().enabled = false;
        this.GetComponent<NetworkObject>().TrySetParent(player.transform);
        this.GetComponent<NetworkObject>().transform.position = player.boneItem.transform.position;
        this.GetComponent<FollowTransform>().targetTransform = player.boneItem.transform;
        player.SetItemHandClientRpc(gameObject);
        player.ChangeState(PlayerState.Interact);
    }
}
