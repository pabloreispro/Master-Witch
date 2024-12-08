using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KrakenTentacle : Ingredient
{
    //Prende um jogador aleatorio na area
    [SerializeField] LayerMask playerMask;
    [SerializeField] float range;
    [SerializeField] float duration;
    [SerializeField] GameObject aoeVfx;
    [SerializeField] GameObject rootVfx;
    PlayerMovement player;

    public override void Drop(Player player)
    {
        base.Drop(player);
        var vfx = Instantiate(aoeVfx, player.transform.position, rootVfx.transform.rotation);
        Destroy(vfx, startDelay);
    }
    public override void StartEffect()
    {
        base.StartEffect();
        var playersColliders = Physics.OverlapSphere(transform.position, range, playerMask);
        if (playersColliders.Length > 0)
        {
            var i = Random.Range(0, playersColliders.Length);
            player = playersColliders[i].GetComponent<PlayerMovement>();
            player.CanMove = false;
            var rootVfx = Instantiate(this.rootVfx, player.transform.position, this.rootVfx.transform.rotation);
            Destroy(rootVfx, duration);
        }
        Invoke(nameof(EndEffect), duration);
    }
    public override void EndEffect()
    {
        base.EndEffect();
        player.CanMove = true;
    }
    public override void OnEffectCanceled()
    {
        base.OnEffectCanceled();
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, range);
    }

}
