using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CameraController : NetworkBehaviour
{
    public Transform player1;
    public Transform player2;
    public Camera mainCamera;
    public float fieldOfViewAngle = 60f;
    public float adjustmentSpeed = 1f; 

    void LateUpdate()
    {
        if (!AreBothPlayersVisible())
        {
            AdjustCameraPosition();
        }
    }

    bool AreBothPlayersVisible()
    {
        return IsInCameraView(player1) && IsInCameraView(player2);
    }

    bool IsInCameraView(Transform player)
    {
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(player.position);
        return screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1 && screenPoint.z > 0;
    }

    void AdjustCameraPosition()
    {
        Vector3 centerPoint = (player1.position + player2.position) / 2f;
        centerPoint.y = mainCamera.transform.position.y; 

        
        Vector3 newCameraPosition = Vector3.Lerp(mainCamera.transform.position, centerPoint, Time.deltaTime * adjustmentSpeed);
        
        
        newCameraPosition.y = mainCamera.transform.position.y;

        
        mainCamera.transform.position = newCameraPosition;
    }
}

