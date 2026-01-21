using UnityEngine;
using UnityEngine.UI;

public class ScrollBarOnEnableFix : MonoBehaviour
{
    public Scrollbar scrollbar;

    private void OnEnable()
    {
        scrollbar.value = 1f;
    }
}
