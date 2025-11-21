using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class InterfaceManager : MonoBehaviour
{
    public List<GameObject> plotButtons, addRemoveButtons, dimensionsGameObjects, idStandsDropdowns;
    public Parser parser;

    [SerializeField] int activePlotIndex = 0;

    public void SetActivePlotIndex(int index)
    {
        activePlotIndex = index;
        foreach (var btn in plotButtons)
        {
            var button = btn.GetComponent<Button>();
            var text = button.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.fontStyle = TMPro.FontStyles.Normal;
            }
        }
        var activePlotButton = plotButtons[index].GetComponent<Button>();
        var activePlottext = activePlotButton.GetComponentInChildren<TMP_Text>();
        if (activePlottext != null)
        {
            activePlottext.fontStyle = TMPro.FontStyles.Bold;
        }

        DeactivateGameObjects(dimensionsGameObjects);

        int firstIndex = activePlotIndex % 2 == 0 ? activePlotIndex : activePlotIndex + 1;
        int secondIndex = firstIndex + 1;

        if (firstIndex >= 0 && firstIndex < dimensionsGameObjects.Count)
            dimensionsGameObjects[firstIndex].SetActive(true);
        if (secondIndex >= 0 && secondIndex < dimensionsGameObjects.Count)
            dimensionsGameObjects[secondIndex].SetActive(true);
        
        DeactivateGameObjects(idStandsDropdowns);
        idStandsDropdowns[activePlotIndex].SetActive(true);
    }

    //removed a plot
    public void ToggleAddButton()
    {
        DeactivateGameObjects(addRemoveButtons);
        addRemoveButtons[0].SetActive(true);
        plotButtons[plotButtons.Count - 1].SetActive(false);
        idStandsDropdowns[idStandsDropdowns.Count - 1].GetComponent<TMP_Dropdown>().ClearOptions();
        idStandsDropdowns[idStandsDropdowns.Count - 1].SetActive(false);
        idStandsDropdowns.First().SetActive(true);
        parser.removeEntryList();
    }

    //added a plot
    public void ToggleRemoveButton()
    {
        DeactivateGameObjects(addRemoveButtons);
        addRemoveButtons[1].SetActive(true);
        plotButtons[plotButtons.Count - 1].SetActive(true);
        parser.addEntryList();
    }

    public int getActivePlotIndex()
    {
        return activePlotIndex;
    }
    private void DeactivateGameObjects(List<GameObject> gameObjects)
    {
        foreach (GameObject btn in gameObjects)
        {
            btn.SetActive(false);
        }
    }

}
