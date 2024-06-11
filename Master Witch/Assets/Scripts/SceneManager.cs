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

    // Start is called before the first frame update
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
        StartCoroutine(TimeCounter());
    }

    [ServerRpc (RequireOwnership = false)]
    public void StartMainServerRpc()
    {
        timeCount.Value = 100;
        StartCoroutine(TimeCounter());
    }
           

    [ServerRpc(RequireOwnership = false)]
    public void ChangeSceneServerRpc(bool a, bool b)
    {
        ChangeSceneClientRpc(sceneMarket.Value = a, sceneMain.Value = b);

        if(prefabMain.activeSelf)
            StartMainServerRpc();
        else
            StartMarketServerRpc();
        
    }
    [ClientRpc]
    public void ChangeSceneClientRpc(bool a, bool b)
    {
        prefabMarket.SetActive(a);
        prefabMain.SetActive(b);
        
    }
    
    
    IEnumerator TimeCounter()
    {
        Debug.Log("Timer");
        while(timeCount.Value > 0)
        {
            yield return new WaitForSeconds(1f);
            timeCount.Value -= 1;
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
            //NetworkManagerUI.Instance.continueButton.interactable = false;
        }
    }

    
}