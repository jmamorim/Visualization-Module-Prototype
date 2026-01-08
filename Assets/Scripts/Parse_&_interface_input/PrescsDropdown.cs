using CustomUI;
using System.Collections.Generic;
using UnityEngine;

public class PrescsDropdown : MonoBehaviour
{
    public Manager manager;
    public bool isMainPlot;

    private DropdownEx dropdown;

    private void Awake()
    {
        dropdown = GetComponent<DropdownEx>();
    }

    private void Start()
    {
        if (dropdown != null)
        {
            dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }
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
            dropdown = GetComponent<DropdownEx>();

        dropdown.ClearOptions();

        List<string> prescNames = new List<string>();

        for (int i = 0; i < prescNameSO.Count; i++)
        {
            prescNames.Add($"{prescNameSO[i]}-{prescNameYT[i]}");
        }

        dropdown.AddOptions(prescNames);

        dropdown.RefreshShownValue();
    }

    private void OnDropdownValueChanged(uint value)
    {
        if (dropdown == null || dropdown.options.Count == 0)
            return;

        int index = (int)value;

        if (index < 0 || index >= dropdown.options.Count)
            return;

        string selectedText = dropdown.options[index].text;
        manager.updateSelectedPrescriptions(selectedText, isMainPlot);
    }

}
