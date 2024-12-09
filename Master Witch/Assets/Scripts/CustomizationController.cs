using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomizationController : MonoBehaviour
{
    public const string PLAYER_ACESSORY_KEY = "PlayerAcessory";
    public const string PLAYER_HAT_KEY = "PlayerHat";
    public const string PLAYER_SKIN_KEY = "PlayerSkin";
    public PlayerCustomization player;
    int currentAcessory;
    int currentHat;
    int currentSkin;
    // Start is called before the first frame update
    void Start()
    {
        SetAcessory(PlayerPrefs.GetInt(PLAYER_ACESSORY_KEY));
        SetHat(PlayerPrefs.GetInt(PLAYER_HAT_KEY));
        SetSkin(PlayerPrefs.GetInt(PLAYER_SKIN_KEY));
    }

    public void NextAcessory()
    {
        if (player.acessories[currentAcessory] != null)
            player.acessories[currentAcessory].SetActive(false);
        currentAcessory = (currentAcessory + 1) % player.acessories.Length;
        SetAcessory(currentAcessory);
    }
    public void PreviousAcessory()
    {
        if (player.acessories[currentAcessory] != null)
            player.acessories[currentAcessory].SetActive(false);
        currentAcessory = currentAcessory <= 0 ? player.acessories.Length - 1 : currentAcessory - 1;
        SetAcessory(currentAcessory);
    }
    void SetAcessory(int index)
    {
        currentAcessory = index;
        PlayerPrefs.SetInt(PLAYER_ACESSORY_KEY, index);
        if(player.acessories[currentAcessory] != null)
            player.acessories[currentAcessory].SetActive(true);
    }


    public void NextHat()
    {
        if (player.hats[currentHat] != null)
            player.hats[currentHat].SetActive(false);
        currentHat = (currentHat + 1) % player.hats.Length;
        SetHat(currentHat);

    }
    public void PreviousHat()
    {
        if (player.hats[currentHat] != null)
            player.hats[currentHat].SetActive(false);
        currentHat = currentHat <= 0 ? player.hats.Length - 1 : currentHat - 1;
        SetHat(currentHat);
    }
    void SetHat(int index)
    {
        currentHat = index;
        PlayerPrefs.SetInt(PLAYER_HAT_KEY, index);
        if (player.hats[currentHat] != null)
            player.hats[currentHat].SetActive(true);

    }


    public void NextSkin()
    {
        currentSkin = (currentSkin + 1) % player.skins.Length;
        SetSkin(currentSkin);

    }
    public void PreviousSkin()
    {
        currentSkin = currentSkin <= 0 ? player.skins.Length - 1 : currentSkin - 1;
        SetSkin(currentSkin);
    }
    void SetSkin(int index)
    {
        currentSkin = index;
        PlayerPrefs.SetInt(PLAYER_SKIN_KEY, index);
        player.playerRenderer.material = player.skins[index];
    }


}
[Serializable]
public class Customization
{
    public GameObject[] gameObjects;
    public void SetActive(bool value)
    {
        foreach (var item in gameObjects)
        {
            item.SetActive(value);
        }
    }
}