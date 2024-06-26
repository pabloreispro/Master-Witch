using Unity.Netcode;
using UnityEngine;

public class SynchronizedObject : NetworkBehaviour
{
    
    public Transform objectToSync; 

    private void Update()
    {
        
            
        UpdateClientPositionAndRotationClientRpc(objectToSync.position, objectToSync.rotation);
        
    }

    [ClientRpc]
    private void UpdateClientPositionAndRotationClientRpc(Vector3 position, Quaternion rotation)
    {
        
        
            objectToSync.position = position;
            objectToSync.rotation = rotation;
        
    }
}