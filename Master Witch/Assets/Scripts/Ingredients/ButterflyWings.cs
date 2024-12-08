using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButterflyWings : Ingredient
{
    //Players ficam rapidos
    [SerializeField] LayerMask playerMask;
    [SerializeField] float range;
    [SerializeField] float duration;
    [SerializeField] float speedModifier;
    [SerializeField] GameObject fogVfx;
    [SerializeField] SphereCollider aoeCollider;
    List<PlayerMovement> players = new List<PlayerMovement>();

    protected override void Awake()
    {
        base.Awake();
        aoeCollider.enabled = false;
    }
    public override void StartEffect()
    {
        base.StartEffect();
        aoeCollider.enabled = true;
        aoeCollider.radius = range;
        var vfx = Instantiate(fogVfx, transform.position, fogVfx.transform.rotation, transform);
        Destroy(vfx, duration);
        Invoke(nameof(EndEffect), duration);
    }
    public override void EndEffect()
    {
        base.EndEffect();
        aoeCollider.enabled = false;
        foreach (var player in players)
        {
            player.SpeedModifier = 1;
        }
    }
    public override void OnEffectCanceled()
    {
        base.OnEffectCanceled();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerMask) != 0)
        {
            var player = other.GetComponent<PlayerMovement>();
            player.SpeedModifier = speedModifier;
            players.Add(player);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerMask) != 0)
        {
            var player = other.GetComponent<PlayerMovement>();
            player.SpeedModifier = 1;
            players.Remove(player);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, range);
    }

}
