using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ParticuleCollisionObject : NetworkBehaviour
{
    public ParticleSystem dustParticule;
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) // Apenas o servidor detecta a colisão
        {
            Vector3 position = collision.contacts[0].point;
            Vector3 normal = collision.contacts[0].normal;

            // Chama a RPC para os clientes
            SpawnParticleClientRpc(position, normal);
        }
    }

    [ClientRpc]
    private void SpawnParticleClientRpc(Vector3 position, Vector3 normal)
    {
        // Instancia a partícula no cliente
        ParticleSystem instance = Instantiate(dustParticule);

        // Define a posição da instância no ponto de contato
        instance.transform.position = position;

        // Ajusta a rotação com base na normal da superfície (opcional)
        instance.transform.rotation = Quaternion.LookRotation(normal);

        // Certifique-se de que está usando World Simulation Space
        var mainModule = instance.main;
        mainModule.simulationSpace = ParticleSystemSimulationSpace.World;

        // Toca as partículas
        instance.Play();

        // Destroi a instância após a duração das partículas
        Destroy(instance.gameObject, instance.main.duration + instance.main.startLifetime.constantMax);
    }
}
