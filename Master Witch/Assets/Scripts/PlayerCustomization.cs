using Network;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCustomization : MonoBehaviour
{
    public Customization[] acessories;
    public Customization[] hats;
    public Material[] skins;
    public SkinnedMeshRenderer playerRenderer;

    public void SetCustomization(PlayerCustomizationData customization)
    {
        if (acessories[customization.acessoryIndex] != null)
            acessories[customization.acessoryIndex].SetActive(true);
        if (hats[customization.hatIndex] != null)
            hats[customization.hatIndex].SetActive(true);
        playerRenderer.material = skins[customization.skinIndex];

    }
}
