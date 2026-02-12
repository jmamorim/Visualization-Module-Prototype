using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScrollBarOnEnableFix : MonoBehaviour
{
    public Scrollbar scrollbar;

    private void OnEnable()
    {
        StartCoroutine(SetScrollbarNextFrame());
    }

    private IEnumerator SetScrollbarNextFrame()
    {
        yield return null;
        scrollbar.value = 1f;
    }
}
