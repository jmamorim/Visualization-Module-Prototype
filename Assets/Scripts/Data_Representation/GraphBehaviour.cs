using System;
using UnityEngine;
using UnityEngine.EventSystems;
using XCharts.Runtime;

public class GraphBehaviour : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public CameraBahaviour cam1, cam2;
    public Manager manager;

    [SerializeField] private bool isMultiLine = false;
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 dragOffset;
    private bool isDragging = false;
    private bool isResizing = false;
    private Vector2 originalSize;
    private Vector2 originalMousePos;
    private BaseChart chart;
    private const float minSize = 100f;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        chart = GetComponent<LineChart>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDragging || isResizing || chart == null)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            chart.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        float minDistance = float.MaxValue;
        int clickedSerieIndex = -1;
        int clickedDataIndex = -1;

        for (int i = 0; i < chart.series.Count; i++)
        {
            var serie = chart.GetSerie(i);
            if (serie == null) continue;

            for (int j = 0; j < serie.dataCount; j++)
            {
                var serieData = serie.GetSerieData(j);
                if (serieData == null || serieData.ignore) continue;

                var dataPosition = serieData.context.position;
                if (dataPosition == Vector3.zero) continue;

                float distance = Vector2.Distance(localPoint, new Vector2(dataPosition.x, dataPosition.y));
                if (distance < minDistance)
                {
                    minDistance = distance;
                    clickedSerieIndex = i;
                    clickedDataIndex = j;
                }
            }
        }

        if (minDistance < 10f && clickedSerieIndex >= 0)
        {
            var xAxis = chart.GetChartComponent<XAxis>();
            if (xAxis != null && clickedDataIndex < xAxis.data.Count)
            {
                string xValue = xAxis.data[clickedDataIndex];
                if (int.TryParse(xValue, out int year))
                    manager.changeSimYearOnGraphClick(clickedSerieIndex, year, isMultiLine);
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            isResizing = true;
            cam1.DisableRotation();
            cam2.DisableRotation();
            originalSize = rectTransform.sizeDelta;
            originalMousePos = eventData.position;
        }
        else
        {
            isDragging = true;
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
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isResizing)
        {
            if (canvas == null) return;

            Vector2 delta = (eventData.position - originalMousePos) / canvas.scaleFactor;
            float scale = Mathf.Max(delta.x, delta.y);
            Vector2 newSize = originalSize + new Vector2(scale, scale);

            newSize.x = Mathf.Max(minSize, newSize.x);
            newSize.y = Mathf.Max(minSize, newSize.y);

            rectTransform.sizeDelta = newSize;

            return;
        }

        if (isDragging)
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
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isResizing)
        {
            isResizing = false;
            cam1.EnableRotation();
            cam2.EnableRotation();
            return;
        }

        if (isDragging)
        {
            isDragging = false;
            if (!manager.isParalelCameraActiveFunc())
            {
                cam1.EnableRotation();
                cam2.EnableRotation();
            }
        }
    }
}
