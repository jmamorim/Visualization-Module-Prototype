using TMPro;
using UnityEngine;

public class ReloadButtonBehaviour : MonoBehaviour
{
    public Initializer initializer;
    public TMP_Dropdown selectedSim;

    public void reloadSim()
    {
        initializer.ReloadSimulation(selectedSim.options[selectedSim.value].text);
    }
}
