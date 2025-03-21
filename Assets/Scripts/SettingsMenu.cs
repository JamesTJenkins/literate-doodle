using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour {
	[Header("Game")]
	[SerializeField] private Slider sensitivitySlider;
	[SerializeField] private TMP_InputField sensitivityInput;
	[SerializeField] private Toggle invertLookToggle;
	[SerializeField] private Toggle crosshairToggle;
	[Header("Video")]
	[SerializeField] private TMP_Dropdown resolutionDropdown;
	[SerializeField] private TMP_Dropdown windowModeDropdown;
	[SerializeField] private Toggle vsync;
	[Header("Audio")]
	[SerializeField] private AudioMixer mixer;
	[SerializeField] private Slider masterSlider;
	[SerializeField] private Slider musicSlider;
	[SerializeField] private Slider sfxSlider;

	private Resolution[] resolutions;
	private bool unsavedChanges = false;
	private float prevValue;

	private void Start()
	{
		// Set resolutions within settings dropdown
		resolutions = Screen.resolutions;
		Array.Reverse(resolutions);
		resolutionDropdown.ClearOptions();
		List<string> options = new List<string>();

		double refreshRate = resolutions[0].refreshRateRatio.value;
		for (int i = 1; i < resolutions.Length; ++i) {
			if (resolutions[i].refreshRateRatio.value > refreshRate) {
				refreshRate = resolutions[i].refreshRateRatio.value;
			}
		}

		int currentResolutionIndex = 0;
		for (int i = 0; i < resolutions.Length; i++)
		{
			if (resolutions[i].refreshRateRatio.value < refreshRate)
				continue;

			string option = resolutions[i].width + "x" + resolutions[i].height;
			
			options.Add(option);
			if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height && resolutions[i].refreshRateRatio.value == Screen.currentResolution.refreshRateRatio.value)
			{
				currentResolutionIndex = i;
			}
		}

		// Set options and put current resolution as selected option
		resolutionDropdown.AddOptions(options);
		resolutionDropdown.value = currentResolutionIndex;

		// 0: Fullscreen, 1: windowed, 2: borderless
		switch (Screen.fullScreenMode)
		{
			case FullScreenMode.ExclusiveFullScreen:
				windowModeDropdown.value = 0;
				break;
			case FullScreenMode.Windowed:
				windowModeDropdown.value = 1;
				break;
			case FullScreenMode.FullScreenWindow:
				windowModeDropdown.value = 2;
				break;
		}

		// Get player data
		GameData pd = Save.GetData();
		vsync.isOn = pd.vsync;
		SetVsync();
		InitalizeAudio(pd.masterVol, pd.musicVol, pd.sfxVol);
		sensitivitySlider.value = pd.sensitivity;
		sensitivityInput.text = pd.sensitivity.ToString();
		prevValue = pd.sensitivity;
		invertLookToggle.isOn = pd.invertLook;
		crosshairToggle.isOn = pd.crosshair;

		// Events
		PlayerEvents.saveSettings += SaveGameSettings;
	}

	private void OnDestroy() {
		PlayerEvents.saveSettings -= SaveGameSettings;
	}

	// Save stuff
	public async void SaveGameSettings() {
		// Check if anything has been changed
		if (!unsavedChanges)
			return;

		unsavedChanges = false;

		GameData pd = Save.GetData();
		// Game
		pd.UpdateSensitivity(sensitivitySlider.value);
		pd.UpdateInvertLook(invertLookToggle.isOn);
		pd.UpdateCrosshair(crosshairToggle.isOn);
		// Video
		Resolution res = resolutions[resolutionDropdown.value];
		pd.UpdateVideoSettings(res.width, res.height, windowModeDropdown.value, vsync.isOn);
		// Audio
		pd.UpdateAudioSettings(masterSlider.value, musicSlider.value, sfxSlider.value);

		await Save.SaveAsync(pd);
	}

	// Game settings
	public void OnSensitivityEndEdit(string changed) {
		if (!UpdateSensitivityField(changed)) {
			sensitivityInput.text = prevValue.ToString();
			sensitivitySlider.value = prevValue;
		} 
	}

	public void OnSensitivityFieldChanged(string changed) {
		UpdateSensitivityField(changed);
	}
	
	public void OnSensitivitySliderChanged() {
		// Stops making the sensitivity box get overflown
		string value = sensitivitySlider.value.ToString();
		if (value.Length > sensitivityInput.characterLimit) {
			value = value.Substring(0, sensitivityInput.characterLimit);
			sensitivitySlider.value = float.Parse(value);
		}

		sensitivityInput.text = sensitivitySlider.value.ToString();
		prevValue = sensitivitySlider.value;
		unsavedChanges = true;
	}

	private bool UpdateSensitivityField(string changed) {
		if (float.TryParse(changed, out float result)) {
			sensitivitySlider.value = result;
			prevValue = result;
			unsavedChanges = true;

			return true;
		}

		return false;
	}

	public void ToggleInvertLook(bool value) {
		unsavedChanges = true;
	}

	public void ToggleCrosshair(bool value) {
		unsavedChanges = true;
	}

	// Video settings 

	// 0: Fullscreen, 1: windowed, 2: borderless
	public void SetWindowMode()
	{
		Resolution res = resolutions[resolutionDropdown.value];
		switch (windowModeDropdown.value)
		{
			case 0:
				Screen.SetResolution(res.width, res.height, FullScreenMode.ExclusiveFullScreen);
				break;
			case 1:
				Screen.SetResolution(res.width, res.height, FullScreenMode.Windowed);
				break;
			case 2:
				Screen.SetResolution(res.width, res.height, FullScreenMode.FullScreenWindow);
				break;
			default:
				break;
		}

		unsavedChanges = true;
	}

	public void SetVsync() {
		QualitySettings.vSyncCount = vsync.isOn ? 1 : 0;
		Debug.Log(vsync.isOn);
		unsavedChanges = true;
	}

	// Audio settings
	// Used to set sliders, and initial audio settings
	public void InitalizeAudio(float _masterVol, float _musicVol, float _sfxVol)
	{
		// Update sliders
		masterSlider.value = _masterVol;
		musicSlider.value = _musicVol;
		sfxSlider.value = _sfxVol;
		// Set audio settings
		SetVolume(0, _masterVol);
		SetVolume(1, _musicVol);
		SetVolume(2, _sfxVol);
	}

	public void OnVolumeSliderChange(int index)
	{
		switch (index)
		{
			case 0:
				SetVolume(0, masterSlider.value);
				break;
			case 1:
				SetVolume(1, musicSlider.value);
				break;
			case 2:
				SetVolume(2, sfxSlider.value);
				break;
			default:
				break;
		}
		unsavedChanges = true;
	}

	// 0: master, 1: music, 3: sfx
	private void SetVolume(int index, float value)
	{
		switch (index)
		{
			case 0:
				mixer.SetFloat("MasterVol", Mathf.Log(value) * 20);
				break;
			case 1:
				mixer.SetFloat("MusicVol", Mathf.Log(value) * 20);
				break;
			case 2:
				mixer.SetFloat("SFXVol", Mathf.Log(value) * 20);
				break;
			default:
				break;
		}
	}
}
