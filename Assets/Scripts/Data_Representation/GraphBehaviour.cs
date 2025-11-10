using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using XCharts.Runtime;

public class GraphBehaviour : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public CameraBehaviour cam1, cam2;
    public Manager manager;

    [SerializeField] bool isMultiLine = false;
    [SerializeField] bool isBar = false;
    [SerializeField] BaseChart chart;
    RectTransform rectTransform;
    Canvas canvas;
    Vector2 dragOffset;
    bool isDragging = false;
    bool isResizing = false;
    Vector2 originalSize;
    Vector2 originalMousePos;
    bool showingPercentage = false;
    List<List<double>> originalData = new List<List<double>>();
    const float minSize = 100f;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
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

            for (int j = 0; j < serie.dataCount; j++)
            {
                var serieData = serie.GetSerieData(j);

                var dataPosition = serieData.context.position;

                float distance = Vector2.Distance(localPoint, new Vector2(dataPosition.x, dataPosition.y));
                if (distance < minDistance)
                {
                    minDistance = distance;
                    clickedSerieIndex = i;
                    clickedDataIndex = j;
                }
            }
        }

        Debug.Log($"Clicked on serie {clickedSerieIndex}, data index {clickedDataIndex}");
        manager.changeSimYearOnGraphClick(clickedSerieIndex, clickedDataIndex, isMultiLine, isBar);
    }

    public void SaveOriginalData()
    {
        originalData.Clear();

        foreach (var serie in chart.series)
        {
            var serieValues = new List<double>();
            for (int i = 0; i < serie.dataCount; i++)
            {
                serieValues.Add(serie.GetYData(i));
            }
            originalData.Add(serieValues);
        }
    }

    public void TogglePercentageView()
    {
        showingPercentage = !showingPercentage;

        int dataCount = chart.series[0].dataCount;

        for (int i = 0; i < dataCount; i++)
        {
            double total = 0;
            Debug.Log($"series count:{chart.series.Count} data count:{dataCount}");
            for (int s = 0; s < chart.series.Count; s++)
            {
                total += originalData[s][i];
            }

            for (int s = 0; s < chart.series.Count; s++)
            {
                double newValue = showingPercentage
                    ? (total > 0 ? (originalData[s][i] / total) * 100.0 : 0.0)
                    : originalData[s][i];

                chart.series[s].GetSerieData(i).data[1] = newValue;
            }
        }
        chart.RefreshChart();
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
