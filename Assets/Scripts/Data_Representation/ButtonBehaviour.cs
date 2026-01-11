using System.Collections.Generic;
using UnityEngine;

public class ButtonBehaviour : MonoBehaviour
{
    public GameObject item;
    public bool disablesSelf;
    public Vector2 origianlPos;
    public Animator animator;
    public bool isButtonDisableEnable = false, open = false;

    RectTransform rectTransform;

    private void Start()
    {
        if (item != null)
            rectTransform = item.GetComponent<RectTransform>();
    }

    public void clickItem()
    {
        if (isButtonDisableEnable)
        {
            if (animator != null)
            {
                animator.SetBool("IsOpen", open);
            }
        }
        if (item != null)
        {
            item.SetActive(!item.activeSelf);
            rectTransform.anchoredPosition = origianlPos;
        }
    }
}
