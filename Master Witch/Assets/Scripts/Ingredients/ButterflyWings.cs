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
    [SerializeField] GameObject vfx;
    [SerializeField] SphereCollider aoeCollider;
    List<PlayerMovement> players = new List<PlayerMovement>();
    List<GameObject> vfxs = new List<GameObject>();

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
        Invoke(nameof(EndEffect), duration);
    }
    public override void EndEffect()
    {
        base.EndEffect();
        aoeCollider.enabled = false;
        foreach (var player in players)
        {
        }
        for (int i = 0; i < players.Count; i++)
        {
            players[i].SpeedModifier = 1;
            Destroy(vfxs[i]);
        }
        vfxs.Clear();
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
            var vfx = Instantiate(this.vfx, player.transform.position, player.transform.rotation, player.transform);
            vfx.transform.Rotate(180, 0, 0);
            vfxs.Add(vfx);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerMask) != 0)
        {
            var player = other.GetComponent<PlayerMovement>();
            player.SpeedModifier = 1;
            int i = players.IndexOf(player);
            players.Remove(player);
            Destroy(vfxs[i]);
            vfxs.RemoveAt(i);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, range);
    }

}
