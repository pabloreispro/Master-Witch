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
     
    [SerializeField]
    private GameObject prefabMarket, prefabMain;
    
    public List<Transform> spawnPlayersMarket = new List<Transform>();

    public List<Transform> spawnBasket = new List<Transform>();
    public List<Transform> spawnPlayersMain = new List<Transform>();
    public NetworkVariable<bool> sceneMarket = new NetworkVariable<bool>();
    public NetworkVariable<bool> sceneMain = new NetworkVariable<bool>();
    public NetworkVariable<int> timeCount = new NetworkVariable<int>();
    public NetworkVariable<int> timeMain = new NetworkVariable<int>();
    public NetworkVariable<int> timeMarket = new NetworkVariable<int>();
    public TextMeshProUGUI texto;

    public Transform clockHand; 
    public float maxTime = 0;  
    private float currentTime;

    
    public void Start()
    {
        timeCount.OnValueChanged += (a,b) => texto.text = b.ToString();
    }

    [ServerRpc (RequireOwnership = false)]
    public void RepositionPlayerServerRpc()
    {
        RefillBenchesClientRpc();
        for (int i = 0; i < PlayerNetworkManager.Instance.GetPlayer.Values.ToList().Count; i++)
        {
            var player = PlayerNetworkManager.Instance.GetPlayer.Values.ToList().ElementAt(i);
            //player.bench.ElementAt(i).ingredients.AddRange(player.ingredientsBasket);
            if(player.GetComponentInChildren<Tool>() != null){
                player.GetComponentInChildren<Tool>().gameObject.transform.position = spawnBasket.ElementAt(i).transform.position;
                player.GetComponentInChildren<Tool>().GetComponentInChildren<NetworkObject>().TrySetParent(spawnBasket.ElementAt(i).transform);
                player.RepositionServerRpc(spawnPlayersMain.ElementAt(i).position); 
            }
            
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

    [ServerRpc (RequireOwnership = false)]
    public void StartMarketServerRpc()
    {
        timeCount.Value = 30;
        maxTime = timeCount.Value;
        StartCoroutine(TimeCounter());
    }

    [ServerRpc (RequireOwnership = false)]
    public void StartMainServerRpc()
    {
        timeCount.Value = 100;
        maxTime = timeCount.Value;
        StartCoroutine(TimeCounter());
    }
           

    [ServerRpc(RequireOwnership = false)]
    public void ChangeSceneServerRpc(bool a, bool b)
    {
        ChangeSceneClientRpc(sceneMarket.Value = a, sceneMain.Value = b);

        if(prefabMain.activeSelf){
            StartMainServerRpc();
        }
        else{
            StartMarketServerRpc();
            
        }
        
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
            ChangeSceneServerRpc(false,true);
            RepositionPlayerServerRpc();
        }
        else if(prefabMain.activeSelf){
            NetworkManagerUI.Instance.ActiveFinalPanelClientRpc();
        }
    }
    [ServerRpc(RequireOwnership =false)]
    public void ClockTimeraServerRpc()
    {
        currentTime = timeCount.Value;

        if (currentTime <= 0)
        {
            currentTime = 0;
        }

        float angle = (currentTime / maxTime) * 360; 
        clockHand.eulerAngles = new Vector3(0, 0, angle);

        UpdateClockHandClientRpc(angle);
    }

    [ClientRpc]
    void UpdateClockHandClientRpc(float angle)
    {
        
        clockHand.eulerAngles = new Vector3(0, 0, angle);
    }

    
}