using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PrescsDropdown : MonoBehaviour
{
    public Manager manager;
    public bool isMainPlot;
    private TMP_Dropdown dropdown;

    private void Awake() 
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    private void Start()
    {
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    private void OnDestroy()
    {
        if (dropdown != null)
        {
            dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
        }
    }

    public void initDropdown(List<string> prescNameSO, List<string> prescNameYT)
    {
        if (dropdown == null) 
        {
            dropdown = GetComponent<TMP_Dropdown>();
        }

        dropdown.ClearOptions();
        List<string> prescNames = new List<string>();

        for (int i = 0; i < prescNameSO.Count; i++)
        {
            prescNames.Add($"{prescNameSO[i]}-{prescNameYT[i]}");
        }

        foreach (string name in prescNames)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData() { text = name });
        }

        dropdown.RefreshShownValue();

    }

    public void OnDropdownValueChanged(int value)
    {
        if (dropdown == null || dropdown.options.Count == 0) return;

        string selectedText = dropdown.options[value].text;
        manager.updateSelectedPrescriptions(selectedText, isMainPlot);
    }
}