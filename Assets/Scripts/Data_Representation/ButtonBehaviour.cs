using System;
using Unity.VisualScripting;
using UnityEngine;

public class ButtonBehaviour : MonoBehaviour
{
    public GameObject item;
    public GameObject self;
    public bool disablesSelf;
    public Vector2 origianlPos;

    RectTransform rectTransform;

    private void Start()
    {
        rectTransform = item.GetComponent<RectTransform>();
    }

    public void clickItem()
    {
        if (item != null)
        {
            item.SetActive(!item.activeSelf);
            rectTransform.anchoredPosition = origianlPos;
            if (disablesSelf)
            {
                if (self == null)
                    gameObject.SetActive(false);
                else
                    self.SetActive(false);
            }
        }
    }
}
