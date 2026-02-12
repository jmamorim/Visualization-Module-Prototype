using UnityEngine;
using UnityEngine.UI;

public class WindZoneController : MonoBehaviour
{
    public GameObject windzone;
    public Button windButton;

    public void clickWindButton()
    {
        windzone.SetActive(!windzone.activeSelf);
        windButton.image.color = windzone.activeSelf ? Color.green : Color.red;
        
    }
}
