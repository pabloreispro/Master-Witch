using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class SyncAtivNetworkVariable : NetworkBehaviour
{
    private NetworkVariable<bool> estadoAtivo = new NetworkVariable<bool>();

    private void OnEnable() {
        estadoAtivo.OnValueChanged += MudancaEstado;
    }

    private void OnDisable() {
        estadoAtivo.OnValueChanged -= MudancaEstado;    
    }

    public void SetEstado(bool estado){
        if(IsServer){
            Debug.Log("Ativou");
            estadoAtivo.Value = estado;
        }
    }

    void MudancaEstado(bool valorAntigo, bool valorNovo){
        gameObject.SetActive(valorNovo);
    }
}
