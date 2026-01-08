using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XCharts.Runtime;

public class GraphBehaviour : MonoBehaviour, IPointerClickHandler
{
    public CameraBehaviour cam1, cam2;
    public Manager manager;

    [SerializeField] bool isMultiLine = false;
    [SerializeField] bool isBar = false;
    [SerializeField] bool isDD = false;
    [SerializeField] BaseChart chart;
    bool isDragging = false;
    bool isResizing = false;
    
    List<float> originalDDValues = new List<float>();
    bool ddPercentageMode = false;

    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private Button fullscreenButton;
    [SerializeField] private Button percentageButton;
    [SerializeField] private Sprite inactiveSprite;
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Color inactiveColor = Color.red;
    [SerializeField] private Color activeColor = Color.green;

    private RectTransform rect;
    private Transform originalParent;
    private int originalSiblingIndex;

    private Vector2 originalAnchorMin;
    private Vector2 originalAnchorMax;
    private Vector2 originalOffsetMin;
    private Vector2 originalOffsetMax;
    private Vector2 originalScale;

    private bool isPercentage = false;
    private bool isFullscreen = false;


    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        UpdateFullscreenButtonVisual();

        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();
    }

    public void ToggleFullscreen()
    {
        if (!isFullscreen)
            EnterFullscreen();
        else
            ExitFullscreen();

        UpdateFullscreenButtonVisual();
    }

    private void UpdateFullscreenButtonVisual()
    {
        if (fullscreenButton == null)
            return;

        Image img = fullscreenButton.GetComponent<Image>();
        if (img == null)
            return;

        img.sprite = isFullscreen ? activeSprite : inactiveSprite;
    }

    private void EnterFullscreen()
    {
        if (rect == null || rootCanvas == null)
            return;

        isFullscreen = true;

        originalParent = rect.parent;
        originalSiblingIndex = rect.GetSiblingIndex();
        originalAnchorMin = rect.anchorMin;
        originalAnchorMax = rect.anchorMax;
        originalOffsetMin = rect.offsetMin;
        originalOffsetMax = rect.offsetMax;
        originalScale = rect.localScale;

        LayoutElement le = GetComponent<LayoutElement>();
        if (le != null)
            le.ignoreLayout = true;

        rect.SetParent(rootCanvas.transform, false);
        rect.SetAsLastSibling();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;

        chart.RefreshChart();
    }


    private void ExitFullscreen()
    {
        if (!isFullscreen)
            return;

        isFullscreen = false;

        rect.SetParent(originalParent, false);
        rect.SetSiblingIndex(originalSiblingIndex);

        rect.anchorMin = originalAnchorMin;
        rect.anchorMax = originalAnchorMax;
        rect.offsetMin = originalOffsetMin;
        rect.offsetMax = originalOffsetMax;
        rect.localScale = originalScale;

        LayoutElement le = GetComponent<LayoutElement>();
        if (le != null)
            le.ignoreLayout = false;

        chart.RefreshChart();
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
        if (!isPercentage)
        {
            isPercentage = true;
            percentageButton.GetComponent<Image>().color = activeColor;
            var series = chart.series;
            foreach (var serie in series)
            {
                serie.barPercentStack = true;
            }
        }
        else
        {
            ToggleAbsoluteView();
        }
    }

    public void ToggleAbsoluteView()
    {
        isPercentage = false;
        percentageButton.GetComponent<Image>().color = inactiveColor;
        var series = chart.series;
        foreach (var serie in series)
        {
            serie.barPercentStack = false;
        }
    }
    public void ToggleDDPercentageView()
    {
        if (!ddPercentageMode)
        {
            Debug.Log("Toggling DD Percentage View");
        
            ddPercentageMode = true;
            percentageButton.GetComponent<Image>().color = activeColor;

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
        else
        {
            ToggleDDAbsoluteView();
        }
    }

    public void ToggleDDAbsoluteView()
    {
        Debug.Log("Toggling DD Absolute View");

        ddPercentageMode = false;
        percentageButton.GetComponent<Image>().color = inactiveColor;
        
        var serie = chart.series.First();
        serie.ClearData();

        foreach (float v in originalDDValues)
            serie.AddData(v);

        var yaxis = chart.GetChartComponent<YAxis>();

        yaxis.min = double.NaN;
        yaxis.max = double.NaN;

        chart.RefreshChart();
    }


}