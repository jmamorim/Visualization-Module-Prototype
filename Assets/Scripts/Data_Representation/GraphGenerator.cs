using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using XCharts.Runtime;
using static UnityEngine.EventSystems.EventTrigger;

public class GraphGenerator : MonoBehaviour
{
    public List<LineChart> lineCharts;
    public List<LineChart> MultiLineCharts;
    public List<BarChart> barCharts;

    int highlightedIndex1, highlightedIndex2;
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

        //Populate line charts
        populateLineChart(lineCharts[0], sortedYears, tableData, e => e.N);
        populateLineChart(lineCharts[1], sortedYears, tableData, e => e.Nst);
        populateLineChart(lineCharts[2], sortedYears, tableData, e => e.Ndead);
        populateLineChart(lineCharts[3], sortedYears, tableData, e => e.hdom);
        populateLineChart(lineCharts[4], sortedYears, tableData, e => e.dg);
        populateLineChart(lineCharts[5], sortedYears, tableData, e => e.G);
        populateLineChart(lineCharts[6], sortedYears, tableData, e => e.V);
        populateLineChart(lineCharts[7], sortedYears, tableData, e => e.Vu_st);

        //populate bar charts
        populateBarChart(
            tableData[0][0],
            tableData.Count > 1 ? tableData[1][0] : null
            );

        populateMultiLineChart(sortedYears, tableData);

        foreach (LineChart chart in lineCharts)
            highlightPoint(chart, current_year1, current_year2);
        foreach (LineChart chart in MultiLineCharts)
            highlightPointMultiLine(chart, current_year1, current_year2);
    }

    private void populateLineChart(
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

    public void populateBarChart(YieldTableEntry data1, YieldTableEntry data2)
    {
        for (int i = 0; i < barCharts.Count; i++)
        {
            BarChart chart = barCharts[i];
            chart.RemoveAllSerie();
            chart.RefreshChart();

            var xAxis = chart.GetChartComponent<XAxis>();
            if (xAxis == null || xAxis.data.Count == 0)
            {
                Debug.LogWarning("BarChart has no X-axis labels configured.");
                continue;
            }

            int xCount = xAxis.data.Count;

            //Vu_as 
            if (i == 0)
            {
                chart.AddSerie<Bar>("Plot1");
                float[] volumeValues1 = {
                data1.Vu_as1, data1.Vu_as2, data1.Vu_as3,
                data1.Vu_as4, data1.Vu_as5
            };
                for (int j = 0; j < xCount && j < volumeValues1.Length; j++)
                    chart.AddData(0, j, volumeValues1[j]);

                if (data2 != null)
                {
                    chart.AddSerie<Bar>("Plot2");
                    float[] volumeValues2 = {
                    data2.Vu_as1, data2.Vu_as2, data2.Vu_as3,
                    data2.Vu_as4, data2.Vu_as5
                };
                    for (int j = 0; j < xCount && j < volumeValues2.Length; j++)
                        chart.AddData(1, j, volumeValues2[j]);
                }
            }
            //Biomassa
            else if (i == 1)
            {
                chart.AddSerie<Bar>("Plot1");
                float[] biomassValues1 = {
                data1.Ww, data1.Wb, data1.Wbr,
                data1.Wl, data1.Wa, data1.Wr
            };
                for (int j = 0; j < xCount && j < biomassValues1.Length; j++)
                    chart.AddData(0, j, biomassValues1[j]);

                if (data2 != null)
                {
                    chart.AddSerie<Bar>("Plot2");
                    float[] biomassValues2 = {
                    data2.Ww, data2.Wb, data2.Wbr,
                    data2.Wl, data2.Wa, data2.Wr
                };
                    for (int j = 0; j < xCount && j < biomassValues2.Length; j++)
                        chart.AddData(1, j, biomassValues2[j]);
                }
            }

            chart.RefreshChart();
        }
    }

    private void populateMultiLineChart(List<int> years, List<List<YieldTableEntry>> tableData)
    {
        for (int chartIndex = 0; chartIndex < MultiLineCharts.Count; chartIndex++)
        {
            var chart = MultiLineCharts[chartIndex];

            chart.ClearData();
            chart.RemoveAllSerie();
            chart.RemoveData();
            chart.RefreshChart();

            highlightedIndex1 = -1;
            highlightedIndex2 = -1;

            sortedYears = years;

            foreach (var year in years)
                chart.AddXAxisData(year.ToString());

            for (int plotIndex = 0; plotIndex < tableData.Count; plotIndex++)
            {
                var plotData = tableData[plotIndex];

                if (plotData == null || plotData.Count == 0)
                    continue;

                Color baseColor = Color.HSVToRGB((plotIndex * 0.25f) % 1f, 0.8f, 0.9f);
                Color secondaryColor = new Color(baseColor.r * 0.8f, baseColor.g * 0.8f, baseColor.b * 0.8f);

                //iV and maiV
                if (chartIndex == 0)
                {
                    string serieName1 = $"iV Plot{plotIndex + 1}";
                    string serieName2 = $"maiV Plot{plotIndex + 1}";

                    chart.AddSerie<Line>(serieName1);
                    chart.AddSerie<Line>(serieName2);

                    var serie1 = chart.GetSerie(chart.series.Count - 2);
                    var serie2 = chart.GetSerie(chart.series.Count - 1);

                    setupSerieStyle(serie1, baseColor);
                    setupSerieStyle(serie2, secondaryColor);

                    Dictionary<int, float> yearToIV = new Dictionary<int, float>();
                    Dictionary<int, float> yearToMAIV = new Dictionary<int, float>();

                    foreach (var entry in plotData)
                    {
                        yearToIV[entry.year] = entry.iV;
                        yearToMAIV[entry.year] = entry.maiV;
                    }

                    for (int yearIndex = 0; yearIndex < years.Count; yearIndex++)
                    {
                        int year = years[yearIndex];
                        chart.AddData(chart.series.Count - 2, yearIndex, yearToIV.TryGetValue(year, out float iv) ? iv : 0);
                        chart.AddData(chart.series.Count - 1, yearIndex, yearToMAIV.TryGetValue(year, out float maiv) ? maiv : 0);
                    }
                }

                //sumNPV and EEA
                else if (chartIndex == 1)
                {
                    string serieName1 = $"sumNPV Plot{plotIndex + 1}";
                    string serieName2 = $"EEA Plot{plotIndex + 1}";

                    chart.AddSerie<Line>(serieName1);
                    chart.AddSerie<Line>(serieName2);

                    var serie1 = chart.GetSerie(chart.series.Count - 2);
                    var serie2 = chart.GetSerie(chart.series.Count - 1);

                    setupSerieStyle(serie1, baseColor);
                    setupSerieStyle(serie2, secondaryColor);

                    Dictionary<int, float> yearToNPV = new Dictionary<int, float>();
                    Dictionary<int, float> yearToEEA = new Dictionary<int, float>();

                    foreach (var entry in plotData)
                    {
                        yearToNPV[entry.year] = entry.NPVsum;
                        yearToEEA[entry.year] = entry.EEA;
                    }

                    for (int yearIndex = 0; yearIndex < years.Count; yearIndex++)
                    {
                        int year = years[yearIndex];
                        chart.AddData(chart.series.Count - 2, yearIndex, yearToNPV.TryGetValue(year, out float npv) ? npv : 0);
                        chart.AddData(chart.series.Count - 1, yearIndex, yearToEEA.TryGetValue(year, out float eea) ? eea : 0);
                    }
                }
            }

            chart.RefreshChart();
        }
    }

    private void setupSerieStyle(Serie serie, Color color)
    {
        serie.lineType = LineType.Normal;
        serie.symbol.show = true;
        serie.lineStyle.width = 2f;
        serie.lineStyle.color = color;
        serie.itemStyle.color = color;
    }

    private void highlightPoint(LineChart chart, int year1, int year2)
    {
        foreach (Serie serie in chart.series)
        {
            for (int i = 0; i < serie.dataCount; i++)
            {
                serie.GetSerieData(i).state = SerieState.Normal;
            }
        }

        if (chart.series.Count > 0)
        {
            var serie1 = chart.GetSerie(0);
            int index1 = sortedYears.IndexOf(year1);

            if (index1 >= 0 && index1 < serie1.dataCount)
            {
                var data1 = serie1.GetSerieData(index1);
                if (!data1.ignore)
                {
                    data1.state = SerieState.Emphasis;
                    highlightedIndex1 = index1;
                }
            }
            else
            {
                highlightedIndex1 = -1;
            }
        }

        if (chart.series.Count > 1)
        {
            var serie2 = chart.GetSerie(1);
            int index2 = sortedYears.IndexOf(year2);
            if (index2 >= 0 && index2 < serie2.dataCount)
            {
                var data2 = serie2.GetSerieData(index2);
                if (!data2.ignore)
                {
                    data2.state = SerieState.Emphasis;
                    highlightedIndex2 = index2;
                }
            }
            else
            {
                highlightedIndex2 = -1;
            }
        }

        chart.RefreshChart();
    }

    private void highlightPointMultiLine(LineChart chart, int year1, int year2)
    {
        if (chart.series.Count < 2)
            return;
        {
            foreach (Serie serie in chart.series)
            {
                for (int j = 0; j < serie.dataCount; j++)
                {
                    serie.GetSerieData(j).state = SerieState.Normal;
                }
            }
            chart.RefreshChart();

        for (int i = 0; i < chart.series.Count; i += 2)

            if (i + 1 < chart.series.Count)
            {
                int tempHighlightIndex = (i == 0) ? highlightedIndex1 : highlightedIndex2;
                HighlightPair(chart.GetSerie(i), chart.GetSerie(i + 1), year1, ref tempHighlightIndex);

                if (i == 0)
                    highlightedIndex1 = tempHighlightIndex;
                else if (i == 2)
                    highlightedIndex2 = tempHighlightIndex;
            }
        }

        chart.RefreshChart();
    }

    void HighlightPair(Serie serieA, Serie serieB, int year, ref int highlightedIndex)
    {
        if (highlightedIndex >= 0 &&
            highlightedIndex < serieA.dataCount &&
            highlightedIndex < serieB.dataCount)
        {
            serieA.GetSerieData(highlightedIndex).state = SerieState.Normal;
            serieB.GetSerieData(highlightedIndex).state = SerieState.Normal;
        }

        int index = sortedYears.IndexOf(year);
        if (index >= 0 &&
            index < serieA.dataCount &&
            index < serieB.dataCount)
        {
            var dataA = serieA.GetSerieData(index);
            var dataB = serieB.GetSerieData(index);

            if (!dataA.ignore && !dataB.ignore)
            {
                dataA.state = SerieState.Emphasis;
                dataB.state = SerieState.Emphasis;
            }

            highlightedIndex = index;
        }
        else
        {
            highlightedIndex = -1;
        }
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

    private void removeHighlightMultiLine(LineChart chart)
    {
        // Remove highlights from all pairs
        for (int i = 0; i < chart.series.Count; i += 2)
        {
            if (i + 1 < chart.series.Count)
            {
                var serie1 = chart.GetSerie(i);
                var serie2 = chart.GetSerie(i + 1);

                int highlightIndex = (i == 0) ? highlightedIndex1 : highlightedIndex2;

                if (highlightIndex >= 0 &&
                    highlightIndex < serie1.dataCount &&
                    highlightIndex < serie2.dataCount)
                {
                    serie1.GetSerieData(highlightIndex).state = SerieState.Normal;
                    serie2.GetSerieData(highlightIndex).state = SerieState.Normal;
                }
            }
        }
    }


    public void changeHighlightedYearGraphs(int year1, int year2)
    {
        foreach (var chart in lineCharts)
            removeHighlight(chart);
        foreach (var chart in MultiLineCharts)
            removeHighlightMultiLine(chart);
        foreach (var chart in lineCharts)
            highlightPoint(chart, year1, year2);
        foreach (var chart in MultiLineCharts)
            highlightPointMultiLine(chart, year1, year2);
    }


}