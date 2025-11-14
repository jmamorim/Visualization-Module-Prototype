using UnityEngine;

public class AddRemoveButtons : MonoBehaviour
{
    public InterfaceManager interfaceManager;

    public void OnAddButtonClick()
    {
        interfaceManager.ToggleRemoveButton();
    }

    public void OnRemoveButtonClick()
    {
        interfaceManager.ToggleAddButton();
    }
}
