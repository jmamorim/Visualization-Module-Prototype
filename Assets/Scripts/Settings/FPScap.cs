using CustomUI;
using System;
using UnityEngine;

public class FPScap : MonoBehaviour
{
    [SerializeField] DropdownEx dropdown;
    int[] fpsCaps = { 30, 60, 120, -1 };

    const string FPS_PREF_KEY = "FPSCap";

    void Start()
    {
        int savedFPSCap = PlayerPrefs.GetInt(FPS_PREF_KEY, 1);
        Application.targetFrameRate = fpsCaps[savedFPSCap];

        dropdown.value = (uint)savedFPSCap;
        dropdown.RefreshShownValue();

        dropdown.onValueChanged.AddListener(OnFPSCapChanged);
    }

    void OnFPSCapChanged(uint index)
    {
        Application.targetFrameRate = fpsCaps[index];

        PlayerPrefs.SetInt(FPS_PREF_KEY, (int)index);
        PlayerPrefs.Save();
    }
}
