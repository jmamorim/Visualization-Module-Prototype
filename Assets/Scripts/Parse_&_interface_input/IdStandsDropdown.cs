using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class IdStandsDropdown : MonoBehaviour
{
    public Parser parser;
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

    public void initDropdown(List<string> idStands)
    {
        if (dropdown == null)
        {
            dropdown = GetComponent<TMP_Dropdown>();
        }
        dropdown.ClearOptions();
        foreach (string name in idStands)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData() { text = name });
        }
        dropdown.RefreshShownValue();
        OnDropdownValueChanged(dropdown.value);
    }

    public void OnDropdownValueChanged(int value)
    {
        if (dropdown == null || dropdown.options.Count == 0) return;
        string selectedText = dropdown.options[value].text;
    }

}
