using UnityEngine;
using UnityEngine.EventSystems;

public class GraphBehaviour : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CameraBahaviour cam1, cam2;
    
    private RectTransform rectTransform;
    private Canvas canvas;
    //private Vector2 originalPosition;
    private Vector2 dragOffset;


    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        //originalPosition = rectTransform.anchoredPosition;

        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        cam1.DisableRotation();
        cam2.DisableRotation();
        rectTransform.SetAsLastSibling();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        dragOffset = rectTransform.anchoredPosition - localPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        ))
        {
            rectTransform.anchoredPosition = localPoint + dragOffset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        cam1.EnableRotation();
        cam2.EnableRotation();
    }
}