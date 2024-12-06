using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.SO;
using Unity.Netcode;

public class Ingredient : Interactable
{
    protected MeshCollider meshCollider;
    protected NetworkObject networkObject;
    protected Rigidbody rb;
    protected FollowTransform followTransform;
    public FoodSO food;
    public List<RecipeData> itemsUsed = new List<RecipeData>();
    [SerializeField] float startDelay = -1;
    bool effectStarted;

    private void Awake()
    {
        meshCollider = GetComponent<MeshCollider>();
        networkObject = GetComponent<NetworkObject>();
        rb = GetComponent<Rigidbody>();
        followTransform = GetComponent<FollowTransform>();
        meshCollider.enabled = false;
    }

    public override void Pick(Player player)
    {
        if (effectStarted) return;
        if (IsInvoking(nameof(StartEffect)))
        {
            CancelInvoke(nameof(StartEffect));
            OnEffectCanceled();
        }
        meshCollider.enabled = false;
        networkObject.TrySetParent(player.transform);
        networkObject.transform.position = player.boneItem.transform.position;
        rb.useGravity = false;
        rb.isKinematic = false;
        followTransform.targetTransform = player.boneItem.transform;
        player.ChangeState(PlayerState.PickItem);
        player.SetItemHandClientRpc(gameObject);
        
    }
    public override void Drop(Player player)
    {
        base.Drop(player);
        if(startDelay >= 0)
            Invoke(nameof(StartEffect),startDelay);
    }
    public virtual void StartEffect()
    {
        effectStarted = true;
        BlockPosition();
    }
    public virtual void EndEffect()
    {
        Destroy(gameObject, 1);
    }
    public virtual void OnEffectCanceled()
    {

    }
    protected virtual void BlockPosition()
    {
        meshCollider.enabled = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }
}
