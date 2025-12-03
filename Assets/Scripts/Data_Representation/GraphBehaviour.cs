using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using XCharts.Runtime;

public class GraphBehaviour : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public CameraBehaviour cam1, cam2;
    public Manager manager;

    [SerializeField] bool isMultiLine = false;
    [SerializeField] bool isBar = false;
    [SerializeField] bool isDD = false;
    [SerializeField] BaseChart chart;
    RectTransform rectTransform;
    Canvas canvas;
    Vector2 dragOffset;
    bool isDragging = false;
    bool isResizing = false;
    Vector2 originalSize;
    Vector2 originalMousePos;
    const float minSize = 100f;

    List<float> originalDDValues = new List<float>();
    bool ddPercentageMode = false;


    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void SaveDDOriginalValues(List<float> values)
    {
        if (!isDD || chart == null) return;

        originalDDValues.Clear();

        originalDDValues = values;

        if (ddPercentageMode)
        {
            ToggleDDPercentageView();
        }
        else
        {
            ToggleDDAbsoluteView();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDragging || isResizing || chart == null || isDD)
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
    public void TogglePercentageView()
    {
        var series = chart.series;
        foreach (var serie in series)
        {
            serie.barPercentStack = true;
        }
    }

    public void ToggleAbsoluteView()
    {
        var series = chart.series;
        foreach (var serie in series)
        {
            serie.barPercentStack = false;
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

    public void ToggleDDPercentageView()
    {
        ddPercentageMode = true;
        Debug.Log("Toggling DD Percentage View");

        var serie = chart.series.First();

        float total = 0f;
        foreach (float v in originalDDValues)
            total += v;

        serie.ClearData();

        foreach (float v in originalDDValues)
        {
            float percentage = (v / total) * 100f;
            serie.AddData(percentage);
        }

        var yaxis = chart.GetChartComponent<YAxis>();

        yaxis.min = 0;
        yaxis.max = 100;

        chart.RefreshChart();
    }

    public void ToggleDDAbsoluteView()
    {
        Debug.Log("Toggling DD Absolute View");

        ddPercentageMode = false;

        var serie = chart.series.First();
        serie.ClearData();

        foreach (float v in originalDDValues)
            serie.AddData(v);

        var yaxis = chart.GetChartComponent<YAxis>();

        yaxis.min = double.NaN;
        yaxis.max = double.NaN;

        chart.RefreshChart();
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
