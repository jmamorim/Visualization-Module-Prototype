using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using XCharts.Runtime;

public class GraphGenerator : MonoBehaviour
{
    //public BarChart diameterChart, heightChart;
    public List<LineChart> charts;

    int highlightedIndex1, highlightedIndex2;
    bool canExpand = true;
    List<int> sortedYears;

    public void receiveData(List<List<YieldTableEntry>> tableData, int current_year1, int current_year2)
    {

        HashSet<int> allYears = new HashSet<int>();
        foreach (var seriesData in tableData)
        {
            foreach (var entry in seriesData)
                allYears.Add(entry.year);
        }

        sortedYears = new List<int>(allYears);
        sortedYears.Sort();

        PopulateLineChart(charts[0], sortedYears, tableData, e => e.N);
        PopulateLineChart(charts[1], sortedYears, tableData, e => e.Nst);
        PopulateLineChart(charts[2], sortedYears, tableData, e => e.Ndead);
        PopulateLineChart(charts[3], sortedYears, tableData, e => e.hdom);
        PopulateLineChart(charts[4], sortedYears, tableData, e => e.dg);
        PopulateLineChart(charts[5], sortedYears, tableData, e => e.G);

        foreach (LineChart chart in charts)
            highlightPoint(chart, current_year1, current_year2);
    }

    public void PopulateLineChart(
        LineChart chart,
        List<int> years,
        List<List<YieldTableEntry>> tableData,
        Func<YieldTableEntry, float> valueSelector)
    {
        chart.ClearData();
        chart.RemoveAllSerie();
        chart.RemoveData();
        chart.RefreshChart();

        highlightedIndex1 = -1;
        highlightedIndex2 = -1;

        sortedYears = years;

        foreach (var year in years)
            chart.AddXAxisData(year.ToString());

        for (int i = 0; i < tableData.Count; i++)
        {
            chart.AddSerie<Line>($"Plot{i + 1}");
            var serie = chart.GetSerie(i);
            serie.lineType = LineType.Normal;
            serie.symbol.show = true;
            serie.lineStyle.width = 2f;

            Color lineColor = Color.HSVToRGB((i * 0.25f) % 1f, 0.8f, 0.9f);
            serie.lineStyle.color = lineColor;
            serie.itemStyle.color = lineColor;

            List<SerieData> serieDataList = new List<SerieData>();

            int index = 0;
            Dictionary<int, float> yearToValue = new Dictionary<int, float>();
            foreach (var entry in tableData[i])
                yearToValue[entry.year] = valueSelector(entry);

            foreach (var year in years)
            {
                if (yearToValue.TryGetValue(year, out float value))
                    chart.AddData(i, value);
                else
                    chart.AddData(i, 0); 

                var data = serie.GetSerieData(index);
                if (!yearToValue.ContainsKey(year))
                    data.ignore = true; 
                else
                    data.state = SerieState.Normal;

                serieDataList.Add(data);
                index++;
            }
        }

        chart.RefreshChart();
    }

    public void highlightPoint(LineChart chart, int year1, int year2)
    {
        if (chart.series.Count > 0)
        {
            var serie1 = chart.GetSerie(0);

            if (highlightedIndex1 >= 0 && highlightedIndex1 < serie1.dataCount)
                serie1.GetSerieData(highlightedIndex1).state = SerieState.Normal;

            int index1 = sortedYears.IndexOf(year1);
            if (index1 >= 0 && index1 < serie1.dataCount)
            {
                var data1 = serie1.GetSerieData(index1);
                if (!data1.ignore)
                    data1.state = SerieState.Emphasis;
                highlightedIndex1 = index1;
            }
            else
                highlightedIndex1 = -1;
        }

        if (chart.series.Count > 1)
        {
            var serie2 = chart.GetSerie(1);

            if (highlightedIndex2 >= 0 && highlightedIndex2 < serie2.dataCount)
                serie2.GetSerieData(highlightedIndex2).state = SerieState.Normal;

            int index2 = sortedYears.IndexOf(year2);
            if (index2 >= 0 && index2 < serie2.dataCount)
            {
                var data2 = serie2.GetSerieData(index2);
                if (!data2.ignore)
                    data2.state = SerieState.Emphasis;
                highlightedIndex2 = index2;
            }
            else
                highlightedIndex2 = -1;
        }

        chart.RefreshChart();
    }

    private void removeHighlight(LineChart chart)
    {
        var serie1 = chart.GetSerie(0);
        if (highlightedIndex1 >= 0)
        {
            serie1.GetSerieData(highlightedIndex1).state = SerieState.Normal;
        }
        if (chart.series.Count > 1)
        {
            var serie2 = chart.GetSerie(1);
            if (highlightedIndex2 >= 0)
            {
                serie2.GetSerieData(highlightedIndex2).state = SerieState.Normal;
            }
        }
        chart.RefreshChart();
    }

    public void changeHightlightedYearGraphs(int year1, int year2)
    {
        foreach (LineChart chart in charts)
            removeHighlight(chart);
        foreach (LineChart chart in charts)
            highlightPoint(chart, year1, year2);
    }

    public bool canExpandGraph()
    {
        return canExpand;
    }

    public void setCanExpand(bool value)
    {
        canExpand = value;
    }

}