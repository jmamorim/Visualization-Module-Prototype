using UnityEngine;
using UnityEngine.EventSystems;

public class GraphBehaviour : MonoBehaviour, IPointerClickHandler
{
    public GraphGenerator graphGenerator;

    private RectTransform rectTransform;
    private bool isExpanded = false;

    private Vector2 originalPosition;
    private Vector3 originalScale;

    public float expandScale = 2f;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        originalPosition = rectTransform.anchoredPosition;
        originalScale = rectTransform.localScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        isExpanded = !isExpanded;

        if (isExpanded && graphGenerator.canExpandGraph())
        {
            rectTransform.SetAsLastSibling();
            graphGenerator.setCanExpand(false);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localScale = originalScale * expandScale;
        }
        else
        {
            graphGenerator.setCanExpand(true);
            rectTransform.anchoredPosition = originalPosition;
            rectTransform.localScale = originalScale;
        }
    }
}
