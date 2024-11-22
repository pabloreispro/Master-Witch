using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UI;
using Network;

public class ToggleState : SingletonNetwork<ToggleState>
{
    [SerializeField] Toggle toggle;
    [SerializeField] int id;

    public Toggle Toggle => toggle;
    void Start()
    {
        if (toggle != null)
        {
            if (PlayerNetworkManager.Instance.GetPlayerByIndex(id).NetworkObjectId == NetworkManager.SpawnManager.GetLocalPlayerObject().NetworkObjectId)
            {
                toggle.onValueChanged.AddListener(OnToggleValueChanged);
                toggle.interactable = true;
            }
            else
            {
                toggle.interactable = false;
            }
        }
    }

    void OnToggleValueChanged(bool isOn)
    {        
        GameManager.Instance.ReadyPlayersServerRpc(id, isOn);
    }

}
