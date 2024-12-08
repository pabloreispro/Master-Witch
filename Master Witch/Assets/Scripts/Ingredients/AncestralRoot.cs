using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class AncestralRoot : Ingredient
{
    [SerializeField] LayerMask playerMask;
    [SerializeField] float range;
    [SerializeField] float duration;
    [SerializeField] GameObject rootVfx;
    [SerializeField] GameObject aoeVfx;
    PlayerMovement[] players;

    public override void Drop(Player player)
    {
        base.Drop(player);
        var vfx = Instantiate(aoeVfx, transform.position, rootVfx.transform.rotation, transform);
        Destroy(vfx, startDelay);
    }

    public override void StartEffect()
    {
        base.StartEffect();
        var playersColliders = Physics.OverlapSphere(transform.position, range, playerMask);
        players = new PlayerMovement[playersColliders.Length];
        for (int i = 0; i < playersColliders.Length; i++)
        {
            players[i] = playersColliders[i].GetComponent<PlayerMovement>();
            players[i].CanMove = false;
            var vfx = Instantiate(rootVfx, players[i].transform.position, rootVfx.transform.rotation);
            Destroy(vfx, duration);
        }
        Invoke(nameof(EndEffect), duration);
    }
    public override void EndEffect()
    {
        base.EndEffect();
        for (int i = 0; i < players.Length; i++)
        {
            players[i].CanMove = true;
        }
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
