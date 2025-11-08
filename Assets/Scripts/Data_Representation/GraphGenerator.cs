using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
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

        List<int> allYears = new List<int>();
        foreach (var seriesData in tableData)
        {
            foreach (var entry in seriesData)
                allYears.Add(entry.year);
        }

        sortedYears =  allYears;
        sortedYears.Sort();

        foreach (LineChart chart in lineCharts)
            prepareChart(chart);

        populateLineChart(lineCharts[0], tableData, e => e.N);
        populateLineChart(lineCharts[1], tableData, e => e.Nst);
        populateLineChart(lineCharts[2], tableData, e => e.Ndead);
        populateLineChart(lineCharts[3], tableData, e => e.hdom);
        populateLineChart(lineCharts[4], tableData, e => e.dg);
        populateLineChart(lineCharts[5], tableData, e => e.G);
        populateLineChart(lineCharts[6], tableData, e => e.V);
        populateLineChart(lineCharts[7], tableData, e => e.Vu_st);

        populateBarCharts(sortedYears, tableData);

        populateMultiLineChart(tableData);

        foreach (LineChart chart in lineCharts)
            highlightPoint(chart, current_year1, current_year2);
        foreach (LineChart chart in MultiLineCharts)
            highlightPointMultiLine(chart, current_year1, current_year2);
    }

    private void populateLineChart(
        LineChart chart,
        List<List<YieldTableEntry>> tableData,
        Func<YieldTableEntry, float> valueSelector)
    {

        highlightedIndex1 = -1;
        highlightedIndex2 = -1;

        for (int i = 0; i < tableData.Count; i++)
        {
            var entries = tableData[i];
            if (entries == null || entries.Count == 0) continue;

            string standId = entries.First().id_stand;
            chart.AddSerie<Line>(standId);
            var serie = chart.GetSerie(i);

            Color lineColor = Color.HSVToRGB((i * 0.15f) % 1f, 1f, 1f);
            setupSerieStyle(serie, lineColor);

            foreach (var entry in entries)
            {
                chart.AddData(i, entry.year, valueSelector(entry));
            }
        }

        chart.RefreshChart();
    }

    private void prepareChart(BaseChart chart)
    {
        chart.ClearData();
        chart.RemoveAllSerie();
        chart.RemoveData();
        chart.RefreshChart();

        var xAxis = chart.GetChartComponent<XAxis>();
        xAxis.type = Axis.AxisType.Value;
        xAxis.minMaxType = Axis.AxisMinMaxType.Custom;

        if (sortedYears.Count > 0)
        {
            int minYear = sortedYears.Min();
            int maxYear = sortedYears.Max();

            xAxis.min = minYear;
            xAxis.max = maxYear;
        }
    }

    private void prepareBarChart(BaseChart chart, List<int> years)
    {
        chart.ClearData();
        chart.RemoveAllSerie();
        chart.RemoveData();
        chart.RefreshChart();

        chart.GetChartComponent<XAxis>().data.Clear();
        var xAxis = chart.GetChartComponent<XAxis>();
        xAxis.type = Axis.AxisType.Category;
        foreach (var y in years)
        {
            chart.AddXAxisData(y.ToString());
        }
    }


    private void populateBarCharts(List<int> years, List<List<YieldTableEntry>> tableData)
    {
        var chart1 = barCharts[0];
        var gb1 = chart1.GetComponent<GraphBehaviour>();

        prepareBarChart(chart1, years);
        string[] volumeComponents = { "Vu_as1", "Vu_as2", "Vu_as3", "Vu_as4", "Vu_as5" };

        for (int standIndex = 0; standIndex < tableData.Count; standIndex++)
        {
            var plotData = tableData[standIndex];
            if (plotData == null || plotData.Count == 0) continue;

            string standId = plotData[0].id_stand;

            for (int c = 0; c < volumeComponents.Length; c++)
            {
                var serie = chart1.AddSerie<Bar>($"{standId} {volumeComponents[c]}");
                serie.stack = $"volume_{standId}";
                Color color = Color.HSVToRGB((float)(c * 0.12f + standIndex * 0.2f) % 1f, 0.8f, 1f);
                serie.itemStyle.color = color;

                for (int j = 0; j < years.Count; j++)
                {
                    chart1.AddData(serie.index, 0f);
                    serie.GetSerieData(j).ignore = true;
                }

                foreach (var entry in plotData)
                {
                    int yearIndex = years.IndexOf(entry.year);
                    int correctedIndex = chart1.GetData(serie.index, yearIndex) != 0f ? yearIndex + 1 : yearIndex;

                    float v = 0f;
                    switch (volumeComponents[c])
                    {
                        case "Vu_as1": v = entry.Vu_as1; break;
                        case "Vu_as2": v = entry.Vu_as2; break;
                        case "Vu_as3": v = entry.Vu_as3; break;
                        case "Vu_as4": v = entry.Vu_as4; break;
                        case "Vu_as5": v = entry.Vu_as5; break;
                    }

                    chart1.UpdateData(serie.index, correctedIndex, v);
                    serie.GetSerieData(correctedIndex).ignore = false;
                }
            }
        }

        chart1.RefreshChart();
        gb1.SaveOriginalData();

        var chart2 = barCharts[1];
        var gb2 = chart2.GetComponent<GraphBehaviour>();

        prepareBarChart(chart2, years);

        string[] biomassComponents = { "Ww", "Wb", "Wbr", "Wl", "Wa", "Wr" };

        for (int standIndex = 0; standIndex < tableData.Count; standIndex++)
        {
            var plotData = tableData[standIndex];
            if (plotData == null || plotData.Count == 0) continue;

            string standId = plotData[0].id_stand;

            for (int c = 0; c < biomassComponents.Length; c++)
            {
                var serie = chart2.AddSerie<Bar>($"{standId} {biomassComponents[c]}");
                serie.stack = $"biomass_{standId}";
                Color color = Color.HSVToRGB((float)(c * 0.12f + standIndex * 0.2f) % 1f, 0.8f, 1f);
                serie.itemStyle.color = color;

                for (int j = 0; j < years.Count; j++)
                {
                    chart2.AddData(serie.index, 0f);
                    serie.GetSerieData(j).ignore = true;
                }

                foreach (var entry in plotData)
                {
                    int yearIndex = years.IndexOf(entry.year);
                    int correctedIndex = chart2.GetData(serie.index, yearIndex) != 0f ? yearIndex + 1 : yearIndex;

                    float v = 0f;
                    switch (biomassComponents[c])
                    {
                        case "Ww": v = entry.Ww; break;
                        case "Wb": v = entry.Wb; break;
                        case "Wbr": v = entry.Wbr; break;
                        case "Wl": v = entry.Wl; break;
                        case "Wa": v = entry.Wa; break;
                        case "Wr": v = entry.Wr; break;
                    }

                    chart2.UpdateData(serie.index, correctedIndex, v);
                    serie.GetSerieData(correctedIndex).ignore = false;
                }
            }
        }
        chart2.RefreshChart();
        gb2.SaveOriginalData();
    }

    private void populateMultiLineChart(List<List<YieldTableEntry>> tableData)
    {
        for (int chartIndex = 0; chartIndex < MultiLineCharts.Count; chartIndex++)
        {
            var chart = MultiLineCharts[chartIndex];
            if (chart == null) continue;

            prepareChart(chart);

            highlightedIndex1 = -1;
            highlightedIndex2 = -1;

            for (int plotIndex = 0; plotIndex < tableData.Count; plotIndex++)
            {
                var plotData = tableData[plotIndex];
                if (plotData == null || plotData.Count == 0)
                    continue;

                string id_stand = plotData.First().id_stand;
                Color baseColor = Color.HSVToRGB((plotIndex * 0.2f) % 1f, 0.8f, 1f);
                Color secondaryColor = Color.HSVToRGB(((plotIndex * 0.2f) + 0.4f) % 1f, 0.8f, 1f);

                string serieName1, serieName2;
                Func<YieldTableEntry, float> valueSelector1, valueSelector2;

                if (chartIndex == 0)
                {
                    serieName1 = $"iV {id_stand}";
                    serieName2 = $"maiV {id_stand}";
                    valueSelector1 = e => e.iV;
                    valueSelector2 = e => e.maiV;
                }
                else
                {
                    serieName1 = $"sumNPV {id_stand}";
                    serieName2 = $"EEA {id_stand}";
                    valueSelector1 = e => e.NPVsum;
                    valueSelector2 = e => e.EEA;
                }

                chart.AddSerie<Line>(serieName1);
                chart.AddSerie<Line>(serieName2);

                var serie1 = chart.GetSerie(chart.series.Count - 2);
                var serie2 = chart.GetSerie(chart.series.Count - 1);

                setupSerieStyle(serie1, baseColor);
                setupSerieStyle(serie2, secondaryColor);

                foreach (var entry in plotData)
                {
                    chart.AddData(serie1.index, entry.year, valueSelector1(entry));
                    chart.AddData(serie2.index, entry.year, valueSelector2(entry));
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

            if (year1 >= 0 && year1 < serie1.dataCount)
            {
                var data1 = serie1.GetSerieData(year1);
                if (!data1.ignore)
                {
                    data1.state = SerieState.Emphasis;
                    highlightedIndex1 = year1;
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
            if (year2 >= 0 && year2 < serie2.dataCount)
            {
                var data2 = serie2.GetSerieData(year2);
                if (!data2.ignore)
                {
                    data2.state = SerieState.Emphasis;
                    highlightedIndex2 = year2;
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
        foreach (Serie serie in chart.series)
        {
            for (int j = 0; j < serie.dataCount; j++)
                serie.GetSerieData(j).state = SerieState.Normal;
        }

        int[] years = { year1, year2 };
        int[] highlightedIndexes = { highlightedIndex1, highlightedIndex2 };

        for (int i = 0; i + 1 < chart.series.Count && i / 2 < years.Length; i += 2)
        {
            int year = years[i / 2];
            ref int highlightedIndex = ref highlightedIndexes[i / 2];

            HighlightPair(chart.GetSerie(i), chart.GetSerie(i + 1), year, ref highlightedIndex);

            if (i == 0)
                highlightedIndex1 = highlightedIndex;
            else if (i == 2)
                highlightedIndex2 = highlightedIndex;
        }

        chart.RefreshChart();
    }

    private void HighlightPair(Serie serieA, Serie serieB, int year, ref int highlightedIndex)
    {
        if (highlightedIndex >= 0 &&
            highlightedIndex < serieA.dataCount &&
            highlightedIndex < serieB.dataCount)
        {
            serieA.GetSerieData(highlightedIndex).state = SerieState.Normal;
            serieB.GetSerieData(highlightedIndex).state = SerieState.Normal;
        }

        if (year >= 0 &&
            year < serieA.dataCount &&
            year < serieB.dataCount)
        {
            var dataA = serieA.GetSerieData(year);
            var dataB = serieB.GetSerieData(year);

            if (!dataA.ignore && !dataB.ignore)
            {
                dataA.state = SerieState.Emphasis;
                dataB.state = SerieState.Emphasis;
            }

            highlightedIndex = year;
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