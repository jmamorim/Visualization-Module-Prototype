using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShapeInputController : MonoBehaviour
{
    // Place on the dropdown: 0:placeholder 1:Square 2:Rectangle 3:Circular
    public List<GameObject> formatsInputs;
    private TMP_Dropdown dropdown;

    private void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        OnDropdownValueChanged(dropdown.value);
    }

    private void OnDestroy()
    {
        if (dropdown != null)
        {
            dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
        }
    }

    public void OnDropdownValueChanged(int value)
    {
        DeactivateAllInputFormats();

        if (value > 0 && value <= formatsInputs.Count)
        {
            formatsInputs[value - 1].SetActive(true);
        }
    }

    private void DeactivateAllInputFormats()
    {
        foreach (GameObject formatInput in formatsInputs)
        {
            formatInput.SetActive(false);
        }
    }

    public (int, List<float>) GetSelectedShapeFormat()
    {
        var plotShape = dropdown.value;
        List<float> dimensions = new List<float>();
        switch (plotShape)
        {
            case 1 or 3: // Square or circular
                dimensions.Add(float.Parse(formatsInputs[plotShape-1].GetComponent<TMP_InputField>().text));
                break;
            case 2: // Rectangle
                var rectangleInputs = formatsInputs[plotShape-1].GetComponentsInChildren<TMP_InputField>();
                foreach (var input in rectangleInputs)
                {
                    dimensions.Add(float.Parse(input.text));
                }
                break;
        }
        return (plotShape, dimensions);
    }
}
