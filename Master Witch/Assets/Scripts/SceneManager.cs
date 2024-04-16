using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class SceneManager : NetworkBehaviour
{
    [SerializeField]
    private GameObject prefabMarket, prefabMain;
    public bool l;
    public NetworkVariable<bool> test = new NetworkVariable<bool>();
    public NetworkVariable<bool> test2 = new NetworkVariable<bool>();
    public NetworkVariable<int> timeCount = new NetworkVariable<int>();
    public Text texto;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartMarket()
    {
        StartCoroutine("a");
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
    
    [ServerRpc]
    public void UpdateTextServerRpc()
    {
        UpdateTextClientRpc();
    }
    [ClientRpc]
    public void UpdateTextClientRpc()
    {
        texto.text = timeCount.Value.ToString();
    }
    IEnumerator a()
    {
        while(timeCount.Value > 0)
        {
            UpdateTextServerRpc();
            yield return new WaitForSeconds(1f);
            timeCount.Value--;
        }
        ChangeSceneServerRpc(false,true);
        
    }
}
