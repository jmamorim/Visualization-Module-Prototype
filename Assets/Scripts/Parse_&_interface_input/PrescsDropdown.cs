using CustomUI;
using System.Collections.Generic;
using UnityEngine;

public class PrescsDropdown : MonoBehaviour
{
    public Manager manager;
    public bool isMainPlot;

    DropdownEx dropdown;
    List<string> allPrescNames = new List<string>();
    string currentSelectedPresc;

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

        allPrescNames.Clear();
        for (int i = 0; i < prescNameSO.Count; i++)
        {
            allPrescNames.Add($"{prescNameSO[i]}-{prescNameYT[i]}");
        }

        dropdown.ClearOptions();
        dropdown.value = 0;
        dropdown.AddOptions(allPrescNames);

        if (allPrescNames.Count > 0)
        {
            currentSelectedPresc = allPrescNames[0];
        }

        dropdown.RefreshShownValue();
    }

    public void SetComparisonMode(bool isComparing)
    {
        if (dropdown != null && dropdown.options.Count > 0 && dropdown.value < dropdown.options.Count)
        {
            currentSelectedPresc = dropdown.options[(int)dropdown.value].text;
        }

        RefreshDropdownOptions(isComparing);
    }

    private void RefreshDropdownOptions(bool isComparing)
    {
        string previousSelection = currentSelectedPresc;

        List<string> optionsToShow = new List<string>();

        if (isComparing)
        {
            if (isMainPlot)
            {
                if (!string.IsNullOrEmpty(previousSelection))
                {
                    optionsToShow.Add(previousSelection);
                }
            }
            else
            {
                string mainDropdownSelection =
                    manager.prescDropdown1.GetComponent<PrescsDropdown>().GetCurrentSelectedPresc();

                foreach (string presc in allPrescNames)
                {
                    if (presc != mainDropdownSelection)
                    {
                        optionsToShow.Add(presc);
                    }
                }
            }
        }
        else
        {
            optionsToShow = new List<string>(allPrescNames);
        }

        dropdown.value = 0;
        dropdown.ClearOptions();

        if (optionsToShow.Count == 0)
            return;

        dropdown.AddOptions(optionsToShow);

        int indexToSelect = 0;

        if (isMainPlot && !string.IsNullOrEmpty(previousSelection))
        {
            int foundIndex = optionsToShow.IndexOf(previousSelection);
            if (foundIndex >= 0)
            {
                indexToSelect = foundIndex;
            }
        }

        dropdown.value = (uint)indexToSelect;
        dropdown.RefreshShownValue();

        currentSelectedPresc = optionsToShow[indexToSelect];
        manager.updateSelectedPrescriptions(currentSelectedPresc, isMainPlot);
    }


    private void OnDropdownValueChanged(uint value)
    {
        if (dropdown == null || dropdown.options.Count == 0)
            return;

        int index = (int)value;
        if (index < 0 || index >= dropdown.options.Count)
            return;

        string selectedText = dropdown.options[index].text;
        currentSelectedPresc = selectedText;

        manager.updateSelectedPrescriptions(selectedText, isMainPlot);

        if (isMainPlot && manager.IsComparingPresc())
        {
            PrescsDropdown secondaryDropdown = manager.prescDropdown2.GetComponent<PrescsDropdown>();
            if (secondaryDropdown != null)
            {
                secondaryDropdown.SetComparisonMode(true);
            }
        }
    }

    public string GetCurrentSelectedPresc()
    {
        return currentSelectedPresc;
    }
}