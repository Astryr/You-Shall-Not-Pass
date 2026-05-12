using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UI_Settings : MonoBehaviour
{
    private CameraController camController;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private float mixerMultiplier = 25;

    [Header("SFX Settings")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private string sfxParamter;
    [SerializeField] private TextMeshProUGUI sfxSliderText;

    [Header("BGM Settings")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private string bgmParamter;
    [SerializeField] private TextMeshProUGUI bgmSliderText;

    [Header("Keyboard Sensetivity")]
    [SerializeField] private Slider keyboardSenseSlider;
    [SerializeField] private TextMeshProUGUI keyboardSensText;
    [SerializeField] private string keyboardSenseParametr = "keyboardSens";

    [SerializeField] private float minKeyboardSens = 60;
    [SerializeField] private float maxKeyboardSens = 240;

    [Header("Mouse Sensetivity")]
    [SerializeField] private Slider mouseSenseSlider;
    [SerializeField] private TextMeshProUGUI mouseSensText;
    [SerializeField] private string mouseSenseParamter = "mouseSens";

    [SerializeField] private float minMouseSense = 1;
    [SerializeField] private float maxMouseSense = 10;

    private void Awake()
    {
        camController = FindFirstObjectByType<CameraController>();
    }

    public void SFXSliderValue(float value)
    {
        float newValue = MathF.Log10(value) * mixerMultiplier;
        audioMixer.SetFloat(sfxParamter, newValue);

        sfxSliderText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    public void BGMSliderValue(float value)
    {
        float newValue = MathF.Log10(value) * mixerMultiplier;
        audioMixer.SetFloat(bgmParamter, newValue);

        bgmSliderText.text = Mathf.Round(value * 100) + "%";
    }

    public void KeyboardSensitivity(float value)
    {
        float newSensetivity = Mathf.Lerp(minKeyboardSens,maxKeyboardSens, value);
        camController.AdjustKeyboardSenseitivty(newSensetivity);

        keyboardSensText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    public void MouseSensitivity(float value)
    {
        float newSenseitivty = Mathf.Lerp(minMouseSense,maxMouseSense, value);
        camController.AdjustMouseSensetivity(newSenseitivty);

        mouseSensText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat(keyboardSenseParametr, keyboardSenseSlider.value);
        PlayerPrefs.SetFloat(mouseSenseParamter, mouseSenseSlider.value);
        PlayerPrefs.SetFloat(sfxParamter, sfxSlider.value);
        PlayerPrefs.SetFloat(bgmParamter, bgmSlider.value);
    }

    private void OnEnable()
    {
        keyboardSenseSlider.value = PlayerPrefs.GetFloat(keyboardSenseParametr, .6f);
        mouseSenseSlider.value = PlayerPrefs.GetFloat(mouseSenseParamter, .6f);
        sfxSlider.value = PlayerPrefs.GetFloat(sfxParamter, .6f);
        bgmSlider.value = PlayerPrefs.GetFloat(bgmParamter, .6f);
    }
}
