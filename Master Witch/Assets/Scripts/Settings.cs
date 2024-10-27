using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    #region Constants
    const string FIRST_PLAY = "FirstPlay";
    const string MASTER_VOLUME = "MasterVolume";
    const string MUSIC_VOLUME = "MusicVolume";
    const string EFFECTS_VOLUME = "EffectsVolume";
    const float DEFAULT_MASTER_VOLUME = 1f;
    const float DEFAULT_MUSIC_VOLUME = 0.7f;
    const float DEFAULT_EFFECTS_VOLUME = 0.7f;
    #endregion
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] TMP_Dropdown resolutionsDropdown;
    [SerializeField] Slider masterVolumeSlider;
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Slider effectVolumeSlider;
    [SerializeField] Toggle fullscreenToggle;
    List<Resolution> resolutions = new List<Resolution>();
    int currentResolutionIndex;

    // Start is called before the first frame update
    void Start()
    {
        resolutions.AddRange(Screen.resolutions);
        resolutionsDropdown.ClearOptions();
        currentResolutionIndex = 0;
        fullscreenToggle.isOn = Screen.fullScreen;
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            var res = Screen.resolutions[i];
            //Filter for the resolution with the highest refresh hate
            if (i < Screen.resolutions.Length - 1)
            {
                //Filtra com o maior hz
                if (res.width == Screen.resolutions[i + 1].width && res.height == Screen.resolutions[i + 1].height)
                {
                    resolutions.Remove(res);
                    continue;
                }
                //Filtra pra 16:9
                if (!Mathf.Approximately((float)res.width / res.height, 16f / 9f))
                {
                    resolutions.Remove(res);
                    continue;
                }
            }
            if (res.width == Screen.width && res.height == Screen.height)
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
            foreach (var res in resolutions)
            {
                if (res.width == Screen.currentResolution.width && res.height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = resolutions.IndexOf(res);
                    SetResolution(currentResolutionIndex);
                }
            }
            masterVolumeSlider.value = DEFAULT_MASTER_VOLUME;
            musicVolumeSlider.value = DEFAULT_MUSIC_VOLUME;
            effectVolumeSlider.value = DEFAULT_EFFECTS_VOLUME;
            SetMasterVolume(masterVolumeSlider.value);
            SetMusicVolume(musicVolumeSlider.value);
            SetEffectsVolume(effectVolumeSlider.value);
            PlayerPrefs.SetInt(FIRST_PLAY, -1);
        }
        else
        {
            masterVolumeSlider.value = PlayerPrefs.GetFloat(MASTER_VOLUME);
            musicVolumeSlider.value = PlayerPrefs.GetFloat(MUSIC_VOLUME);
            effectVolumeSlider.value = PlayerPrefs.GetFloat(EFFECTS_VOLUME);
            SetMasterVolume(masterVolumeSlider.value);
            SetMusicVolume(musicVolumeSlider.value);
            SetEffectsVolume(effectVolumeSlider.value);
        }
    }
    public void SetMasterVolume(float volume) => SetVolume(MASTER_VOLUME, volume);
    public void SetMusicVolume(float volume) => SetVolume(MUSIC_VOLUME, volume);
    public void SetEffectsVolume(float volume) => SetVolume(EFFECTS_VOLUME, volume);
    void SetVolume(string tag, float value)
    {
        var volume = Mathf.Log10(value) * 20;
        audioMixer.SetFloat(tag, volume);
        PlayerPrefs.SetFloat(tag, value);
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
