using UnityEngine;
using UnityEngine.UI;

public class ScrollControler : MonoBehaviour
{
    public ScrollRect scrollRect;
    public void ScrollDown()
    {
        Canvas.ForceUpdateCanvases();   
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
