using System;
using UnityEngine;
using UnityEngine.EventSystems;
using XCharts.Runtime;

public class GraphBehaviour : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public CameraBahaviour cam1, cam2;
    public Manager manager;

    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 dragOffset;
    private bool isDragging = false;
    private BaseChart chart;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        chart = GetComponent<LineChart>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isDragging && chart != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                chart.GetComponent<RectTransform>(),
                eventData.position,
                eventData.pressEventCamera,
                out localPoint
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

                    if (dataPosition != Vector3.zero)
                    {
                        float distance = Vector2.Distance(localPoint, new Vector2(dataPosition.x, dataPosition.y));

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            clickedSerieIndex = i;
                            clickedDataIndex = j;
                        }
                    }
                }
            }

            if (minDistance < 10f)
            {
                var serie = chart.GetSerie(clickedSerieIndex);
                var serieData = serie.GetSerieData(clickedDataIndex);

                var xAxis = chart.GetChartComponent<XAxis>();
                if (xAxis != null && clickedDataIndex < xAxis.data.Count)
                {
                    string xValue = xAxis.data[clickedDataIndex];

                    if (int.TryParse(xValue, out int year))
                    {
                        manager.changeSimYearOnGraphClick(clickedSerieIndex, year);
                    }
                }
            }   
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
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
        isDragging = false;
        if (manager.isParalelCameraActiveFunc())
        {
            return;
        }
        cam1.EnableRotation();
        cam2.EnableRotation();
    }


}