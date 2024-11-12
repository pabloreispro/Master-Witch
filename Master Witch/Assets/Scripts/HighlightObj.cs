using UnityEngine;

public class HighlightObj : MonoBehaviour
{

    private Material[] originalMaterials;
    private Material[] instanceMaterials;

    void Start()
    {
        // Obtém todos os renderers nos filhos e armazena seus materiais
        Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[childRenderers.Length];
        instanceMaterials = new Material[childRenderers.Length];

        for (int i = 0; i < childRenderers.Length; i++)
        {
            originalMaterials[i] = childRenderers[i].material;
            instanceMaterials[i] = new Material(originalMaterials[i]);
            childRenderers[i].material = instanceMaterials[i];
            
            // Desativa a emissão inicialmente
            instanceMaterials[i].DisableKeyword("_EMISSION");
        }
    }

    public void Highlight()
    {
        // Ativa a emissão em todos os materiais dos filhos
        foreach (var material in instanceMaterials)
        {
            material.EnableKeyword("_EMISSION");
            
        }
    }

    public void RemoveHighlight()
    {
        // Desativa a emissão em todos os materiais dos filhos
        foreach (var material in instanceMaterials)
        {
            material.DisableKeyword("_EMISSION");
        }
    }

    
}