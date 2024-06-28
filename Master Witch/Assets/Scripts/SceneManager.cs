using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Network;
using System.Linq;
using Unity.VisualScripting;
using UI;
using TMPro;

public class SceneManager : SingletonNetwork<SceneManager>
{
    const int TIMER_MARKET = 10;
    const int TIMER_MAIN = 100;

    [SerializeField]
    private GameObject prefabMarket, prefabMain;
    
    public List<Transform> spawnPlayersMarket = new List<Transform>();
    public List<Transform> spawnPlayersMain = new List<Transform>();
    public List<Transform> spawnBasket = new List<Transform>();
    
    public NetworkVariable<bool> sceneMarket = new NetworkVariable<bool>();
    public NetworkVariable<bool> sceneMain = new NetworkVariable<bool>();
    public NetworkVariable<int> timeCount = new NetworkVariable<int>();
    public NetworkVariable<bool> isMovementAllowed = new NetworkVariable<bool>(false);
    
    

    public Transform clockHand; 
    public float maxTime = 0;  
    private float currentTime;

    
    

    [ServerRpc (RequireOwnership = false)]
    public void RepositionPlayersMainSceneServerRpc()
    {
        RefillBenchesClientRpc();
        for (int i = 0; i < PlayerNetworkManager.Instance.GetPlayer.Values.ToList().Count; i++)
        {
            var player = PlayerNetworkManager.Instance.GetPlayer.Values.ToList().ElementAt(i);
            //player.bench.ElementAt(i).ingredients.AddRange(player.ingredientsBasket);
            if(player.GetComponentInChildren<Tool>() != null){
                var basket =  player.GetComponentInChildren<Tool>().gameObject;
                basket.GetComponent<FollowTransform>().targetTransform = null;
                basket.transform.position = new Vector3(spawnBasket.ElementAt(i).transform.position.x,spawnBasket.ElementAt(i).transform.position.y+20,spawnBasket.ElementAt(i).transform.position.z);
                basket.transform.rotation = Quaternion.identity;
                
                player.GetComponentInChildren<Tool>().GetComponentInChildren<NetworkObject>().TrySetParent(spawnBasket.ElementAt(i).transform);
            }

            player.RepositionServerRpc(spawnPlayersMain.ElementAt(i).position); 
            
        }
    }

    [ServerRpc (RequireOwnership = false)]
    public void RepositionPlayersMarketSceneServerRpc()
    {
        for (int i = 0; i < PlayerNetworkManager.Instance.GetPlayer.Values.ToList().Count; i++)
        {
            var player = PlayerNetworkManager.Instance.GetPlayer.Values.ToList().ElementAt(i);
            player.RepositionServerRpc(spawnPlayersMarket.ElementAt(i).position);
        }
    }


    [ClientRpc]
    void RefillBenchesClientRpc()
    {
        if (IsServer) return;
        for (int i = 0; i < PlayerNetworkManager.Instance.GetPlayer.Values.ToList().Count; i++)
        {
            var player = PlayerNetworkManager.Instance.GetPlayer.Values.ToList().ElementAt(i);
            //Debug.Log($"{player.NetworkObjectId}, {player.ingredientsBasket.Count}");
            //player.bench.ElementAt(i).ingredients.AddRange(player.ingredientsBasket);
            if(player.GetComponentInChildren<Tool>() != null){
                player.GetComponentInChildren<Tool>().GetComponentInChildren<NetworkObject>().TrySetParent(spawnBasket.ElementAt(i).transform);
            }

        }
    }

    
    public void StartMarket()
    {
        timeCount.Value = TIMER_MARKET;
        maxTime = timeCount.Value;
        StartCoroutine(TimeCounter());
    }

    
    public void StartMain()
    {
        timeCount.Value = TIMER_MAIN;
        maxTime = timeCount.Value;
        StartCoroutine(TimeCounter());
    }
           

    [ServerRpc(RequireOwnership = false)]
    public void ChangeSceneServerRpc(bool a, bool b)
    {
        ChangeSceneClientRpc(sceneMarket.Value = a, sceneMain.Value = b);
        
    }
    [ClientRpc]
    public void ChangeSceneClientRpc(bool a, bool b)
    {
        prefabMarket.SetActive(a);
        prefabMain.SetActive(b);
        
    }
    
    
    IEnumerator TimeCounter()
    {
        
        while(timeCount.Value > 0)
        {
            yield return new WaitForSeconds(1f);
            timeCount.Value -= 1;
            ClockTimeraServerRpc();
        }
        ControllerScenes();
    }
    
    public void ControllerScenes()
    {
        if(prefabMarket.activeSelf){
            StartCoroutine(TransitionController.Instance.TransitionMainScene());
        }
        else if(prefabMain.activeSelf){
            NetworkManagerUI.Instance.UpdateFinalRoundScreenClientRpc();
        }
    }
    [ServerRpc(RequireOwnership =false)]
    public void ClockTimeraServerRpc()
    {
    
        float angle = (Mathf.Max(0, timeCount.Value) / maxTime) * 360; 
        clockHand.eulerAngles = new Vector3(0, 0, angle);

        UpdateClockHandClientRpc(angle);
    }

    [ClientRpc]
    void UpdateClockHandClientRpc(float angle)
    {
        clockHand.eulerAngles = new Vector3(0, 0, angle);
    }

    
}