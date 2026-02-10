using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using CustomUI;
using System;

public class GraphicQualitySettings : MonoBehaviour
{
    [SerializeField] DropdownEx dropdown;

    const string QUALITY_PREF_KEY = "QualityLevel";

    void Start()
    {
        int savedQuality = PlayerPrefs.GetInt(QUALITY_PREF_KEY, 1);
        UnityEngine.QualitySettings.SetQualityLevel(savedQuality, true);

        dropdown.value = (uint)savedQuality;
        dropdown.RefreshShownValue();

        dropdown.onValueChanged.AddListener(OnQualityChanged);
    }

    void OnQualityChanged(uint index)
    {
        UnityEngine.QualitySettings.SetQualityLevel((int)index, true);
        PlayerPrefs.SetInt(QUALITY_PREF_KEY, (int)index);
        PlayerPrefs.Save();
    }
}
