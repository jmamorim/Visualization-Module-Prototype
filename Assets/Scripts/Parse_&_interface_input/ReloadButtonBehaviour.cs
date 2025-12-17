using TMPro;
using UnityEngine;

public class ReloadButtonBehaviour : MonoBehaviour
{
    public Initializer initializer;
    public TMP_Text selectedSim;

    public void reloadSim()
    {
        initializer.ReloadSimulation(selectedSim.text);
    }
}
