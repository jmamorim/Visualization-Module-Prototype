using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class IdStandsDropdown : MonoBehaviour
{
    public bool isMainPlot;
    public GameObject prescInfo, textSimInfo, simInfo, intervalField, parseButton;
    //used to get the info from metadata
    public Initializer initializer;
    //if multi-visualization is enabled, to check if can enable the parsing button
    public TMP_Dropdown dropdown2 = null;

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
        dropdown.ClearOptions();
        
        List<string> standIds = new List<string> { "Escolha um povoamento..." };
        standIds.AddRange(idStands);

        dropdown.AddOptions(standIds);
        dropdown.value = 0;

        dropdown.RefreshShownValue();
        OnDropdownValueChanged(dropdown.value);
    }

    // needs to get info
    public void OnDropdownValueChanged(int value)
    {
        if (dropdown == null || dropdown.options.Count == 0) return;
        if (value != 0)
        {
            //get info about the selected stand
            string selectedText = dropdown.options[value].text;
            prescInfo.SetActive(true);
            textSimInfo.SetActive(true);
            simInfo.SetActive(true);
            if(dropdown2 == null)
            {
                intervalField.SetActive(true);
                parseButton.SetActive(true);
            }
            else
            {
                if (dropdown2.value != 0)
                {
                    intervalField.SetActive(true);
                    parseButton.SetActive(true);
                }
            }
        }
        else
        {
            prescInfo.SetActive(false);
            textSimInfo.SetActive(false);
            simInfo.SetActive(false);
            intervalField.SetActive(false);
            parseButton.SetActive(false);
        }
    }

}
