using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Network;
using System.Linq;

public class SceneManager : SingletonNetwork<SceneManager>
{
     
    [SerializeField]
    private GameObject prefabMarket, prefabMain;
    
    public List<Transform> spawnPlayersMarket = new List<Transform>();

    
    public List<Transform> spawnPlayersMain = new List<Transform>();
    public NetworkVariable<bool> test = new NetworkVariable<bool>();
    public NetworkVariable<bool> test2 = new NetworkVariable<bool>();
    public NetworkVariable<int> timeCount = new NetworkVariable<int>();
    public Text texto;

    // Start is called before the first frame update
    void Start()
    {
        timeCount.OnValueChanged += (a,b) => texto.text = b.ToString();
    }

    [ServerRpc (RequireOwnership = false)]
    public void RepositionPlayerServerRpc()
    {
        for (int i = 0; i < PlayerNetworkManager.Instance.playerList.Values.ToList().Count; i++)
        {
            var player = PlayerNetworkManager.Instance.playerList.Values.ToList().ElementAt(i);
            player.RepositionServerRpc(spawnPlayersMain.ElementAt(i).position);
        }
    }
    
    public void StartMarket()
    {
        StartCoroutine("TimeCounter");
    }

    [ServerRpc (RequireOwnership = false)]
    public void ChangeSceneServerRpc(bool a, bool b)
    {
        ChangeSceneClientRpc(test.Value = a, test2.Value = b);
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
            timeCount.Value--;
        }
        ChangeSceneServerRpc(false,true);
        RepositionPlayerServerRpc();
        
    }
}