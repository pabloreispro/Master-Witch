using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KrakenTentacle : Ingredient
{
    //Prende um jogador aleatorio na area
    [SerializeField] LayerMask playerMask;
    [SerializeField] float range;
    [SerializeField] float duration;
    [SerializeField] GameObject rootVfx;
    PlayerMovement player;
    public override void StartEffect()
    {
        base.StartEffect();
        var playersColliders = Physics.OverlapSphere(transform.position, range, playerMask);
        var i = Random.Range(0, playersColliders.Length);
        player = playersColliders[i].GetComponent<PlayerMovement>();
        player.CanMove = false;
        var vfx = Instantiate(rootVfx, player.transform.position, rootVfx.transform.rotation);
        Destroy(vfx, duration);
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
