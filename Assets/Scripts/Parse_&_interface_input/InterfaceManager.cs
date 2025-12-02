using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class InterfaceManager : MonoBehaviour
{
    public List<GameObject> plotButtons, addRemoveButtons, idStandsDropdowns, currentSim;
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

        DeactivateGameObjects(idStandsDropdowns);
        idStandsDropdowns[activePlotIndex].SetActive(true);
        DeactivateGameObjects(currentSim);
        currentSim[activePlotIndex].SetActive(true);
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
        currentSim[currentSim.Count - 1].GetComponentInChildren<TMP_Text>().text = "";
        SetActivePlotIndex(0);
    }

    //added a plot
    public void ToggleRemoveButton()
    {
        DeactivateGameObjects(addRemoveButtons);
        addRemoveButtons[1].SetActive(true);
        plotButtons[plotButtons.Count - 1].SetActive(true);
    }

    public void selectSim(string name, SimulationInfo info)
    {
        //update the dropdowns to contain info related to the simulation
        if (!currentSim[activePlotIndex].activeSelf)
        {
            currentSim[activePlotIndex].SetActive(true);
        }
        currentSim[activePlotIndex].GetComponentInChildren<TMP_Text>().text = name;

        idStandsDropdowns[activePlotIndex].GetComponent<IdStandsDropdown>().initDropdown(info.plotDataByIdPar.Keys.ToList());
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
