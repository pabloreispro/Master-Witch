using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class Settings : MonoBehaviour
{
    const string FIRST_PLAY = "FirstPlay";
    const string MASTER_VOLUME = "MasterVolume";
    const string MUSIC_VOLUME = "MusicVolume";
    const string EFFECTS_VOLUME = "EffectsVolume";
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] TMP_Dropdown resolutionsDropdown;
    List<Resolution> resolutions = new List<Resolution>();
    int currentResolutionIndex;

    // Start is called before the first frame update
    void Start()
    {
        resolutions.AddRange(Screen.resolutions);
        resolutionsDropdown.ClearOptions();
        currentResolutionIndex = 0;
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            var res = Screen.resolutions[i];
            //Filter for the resolution with the highest refresh hate
            if (i < Screen.resolutions.Length - 1)
                if (res.width == Screen.resolutions[i + 1].width && res.height == Screen.resolutions[i + 1].height)
                {
                    resolutions.Remove(res);
                    continue;
                }
            if (res.width == Screen.currentResolution.width && res.height == Screen.currentResolution.height)
            {
                currentResolutionIndex = resolutions.IndexOf(res);
            }
        }
        List<string> resolutionsOptions = new List<string>();
        for (int i = 0; i < resolutions.Count; i++)
        {
            resolutionsOptions.Add($"{resolutions[i].width}x{resolutions[i].height}");
        }
        resolutionsDropdown.AddOptions(resolutionsOptions);
        resolutionsDropdown.value = currentResolutionIndex;
        if (PlayerPrefs.GetInt(FIRST_PLAY) == 0)
        {
            if (currentResolutionIndex < resolutions.Count)
            {
                currentResolutionIndex = resolutions.Count - 1;
                SetResolution(currentResolutionIndex);
                PlayerPrefs.SetInt(FIRST_PLAY, -1);
            }
        }

    }
    public void SetMasterVolume(float volume)
    {
        var masterVolume = Mathf.Max(volume, 0.00001f);
        audioMixer.SetFloat(MASTER_VOLUME, masterVolume);
    }
    public void SetMusicVolume(float volume)
    {
        var musicVolume = Mathf.Max(volume, 0.00001f);
        audioMixer.SetFloat(MUSIC_VOLUME, musicVolume);
    }
    public void SetEffectsVolume(float volume)
    {
        var sfxVolume = Mathf.Max(volume, 0.00001f);
        audioMixer.SetFloat(EFFECTS_VOLUME, sfxVolume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution res = resolutions[resolutionIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }
}
