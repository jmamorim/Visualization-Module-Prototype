using CustomUI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XCharts.Runtime;

public class GraphGenerator : MonoBehaviour
{
    public List<LineChart> lineCharts;
    public List<LineChart> MultiLineCharts;
    public List<BarChart> barCharts;
    public List<BarChart> ddBarCharts;
    public DropdownEx graphDropdown;

    int highlightedIndex1, highlightedIndex2;
    List<int> sortedYears;

    readonly string[] graphDesc = {
        "Número de Árvores",
        "Número de Cepos",
        "Número de Árvores Mortas",
        "Altura Dominante",
        "Diâmetro Médio Quadrático",
        "Área Basal",
        "Volume Total",
        "Incremento de Volume",
        "Volume do fuste sem cepo nem casca",
        "Valores Económicos",
    };

    readonly string[] addedGraphDesc = {
        "Volume extraído dívidido 5 classificações",
        "Biomassa Total dividida em 5 classes",
        "Distribuição de diâmetros"
    };

    // Inside GraphGenerator class
    readonly string[] DDCategories = {
    "0", "5", "10", "15", "20", "25", "30", "35", "40", "45", "50",
    "55", "60", "65", "70", "75", "80", "85", "90", "95", "100", ">102.5"
};

    public void receiveData(SortedDictionary<string, SortedDictionary<string, List<YieldTableEntry>>> tableData,
        SortedDictionary<string, SortedDictionary<string, List<DDEntry>>> DDtableData,
        int current_year1, int current_year2, string selectedId_stand1,
        string selectedId_stand2, string selectedId_presc1, string selectedId_presc2)
    {

        List<int> allYears = new List<int>();
        foreach (var entry in tableData[selectedId_stand1][selectedId_presc1])
            allYears.Add(entry.year);
        if (tableData.Count > 1)
        {
            foreach (var entry in tableData[selectedId_stand2][selectedId_presc2])
                allYears.Add(entry.year);
        }

        sortedYears = allYears;
        sortedYears.Sort();

        foreach (LineChart chart in lineCharts)
            prepareChart(chart);
        string[] id_stands = { selectedId_stand1, selectedId_stand2 };
        string[] id_prescs = { selectedId_presc1, selectedId_presc2 };
        prepareDropdown(id_stands);
        populateLineChart(lineCharts[0], tableData, e => e.N, id_stands, id_prescs);
        populateLineChart(lineCharts[1], tableData, e => e.Nst, id_stands, id_prescs);
        populateLineChart(lineCharts[2], tableData, e => e.Ndead, id_stands, id_prescs);
        populateLineChart(lineCharts[3], tableData, e => e.hdom, id_stands, id_prescs);
        populateLineChart(lineCharts[4], tableData, e => e.dg, id_stands, id_prescs);
        populateLineChart(lineCharts[5], tableData, e => e.G, id_stands, id_prescs);
        populateLineChart(lineCharts[6], tableData, e => e.V, id_stands, id_prescs);
        populateLineChart(lineCharts[7], tableData, e => e.Vu_st, id_stands, id_prescs);

        populateBarCharts(sortedYears, tableData, id_stands, id_prescs);

        populateDDBarCharts(DDtableData, id_stands, id_prescs,
            new int[] {
                current_year1,
                current_year2
            });

        populateMultiLineChart(tableData, id_stands, id_prescs);

        foreach (LineChart chart in lineCharts)
            highlightPoint(chart, current_year1, current_year2);
        foreach (LineChart chart in MultiLineCharts)
            highlightPointMultiLine(chart, current_year1, current_year2);
    }

    private void prepareDropdown(string[] idStands)
    {
        graphDropdown.ClearOptions(); 

        List<string> options = new List<string>();

        foreach (var desc in graphDesc)
            options.Add(desc);

        foreach (var id in idStands)
        {
            if (string.IsNullOrEmpty(id))
                continue;

            foreach (var desc in addedGraphDesc)
                options.Add($"{desc} ({id})");
        }

        graphDropdown.AddOptions(options);
        graphDropdown.RefreshShownValue();
    }



    private void populateLineChart(
        LineChart chart,
        SortedDictionary<string, SortedDictionary<string, List<YieldTableEntry>>> tableData,
        Func<YieldTableEntry, float> valueSelector,
        string[] id_stands,
        string[] id_prescs)
    {

        highlightedIndex1 = -1;
        highlightedIndex2 = -1;

        for (int i = 0; i < tableData.Count; i++)
        {
            var entries = tableData[id_stands[i]][id_prescs[i]];
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

    private void prepareDDBarChart(BaseChart chart)
    {
        chart.ClearData();
        chart.RemoveAllSerie();
        chart.RemoveData();
        chart.RefreshChart();

        var xAxis = chart.GetChartComponent<XAxis>();
        xAxis.data.Clear();
        xAxis.type = Axis.AxisType.Category;

        foreach (var cat in DDCategories)
        {
            chart.AddXAxisData(cat);
        }
    }

    private void prepareBarChart(BaseChart chart, List<int> years)
    {
        chart.ClearData();
        chart.RemoveAllSerie();
        chart.RemoveData();
        chart.RefreshChart();

        var xAxis = chart.GetChartComponent<XAxis>();
        xAxis.data.Clear();
        xAxis.type = Axis.AxisType.Category;

        foreach (var y in years)
        {
            chart.AddXAxisData(y.ToString());
        }
    }


    private void populateBarCharts(List<int> years, SortedDictionary<string, SortedDictionary<string, List<YieldTableEntry>>> tableData,
        string[] id_stands,
        string[] id_prescs)
    {
        string[] VolumeComponents = { "Vu_as1", "Vu_as2", "Vu_as3", "Vu_as4", "Vu_as5" };
        Color[] volumeColors = {
            new Color(0.36f, 0.20f, 0.09f),
            new Color(0.0f, 0.4f, 0.0f),
            new Color(0.2f, 0.6f, 0.2f),
            new Color(0.8f, 0.5f, 0.2f),
            new Color(0.6f, 0.9f, 0.5f)
        };

        for (int standIndex = 0; standIndex < tableData.Count; standIndex++)
        {
            var chart = barCharts[standIndex];
            prepareBarChart(chart, years);

            var plotData = tableData[id_stands[standIndex]][id_prescs[standIndex]];
            if (plotData == null || plotData.Count == 0) continue;

            string standId = plotData[0].id_stand;
            chart.GetChartComponent<Title>().text = $"Volume extraído {standId}";

            for (int c = 0; c < VolumeComponents.Length; c++)
            {
                string comp = VolumeComponents[c];
                var serie = chart.AddSerie<Bar>($"{standId} {comp}");
                serie.stack = $"volume_{standId}";
                serie.itemStyle.color = volumeColors[c];

                // initialize empty data
                for (int j = 0; j < years.Count; j++)
                {
                    chart.AddData(serie.index, 0f);
                    serie.GetSerieData(j).ignore = true;
                }

                foreach (var entry in plotData)
                {
                    int yearIndex = years.IndexOf(entry.year);
                    int correctedIndex = chart.GetData(serie.index, yearIndex) != 0f ? yearIndex + 1 : yearIndex;

                    float v = comp switch
                    {
                        "Vu_as1" => entry.Vu_as1,
                        "Vu_as2" => entry.Vu_as2,
                        "Vu_as3" => entry.Vu_as3,
                        "Vu_as4" => entry.Vu_as4,
                        "Vu_as5" => entry.Vu_as5,
                        _ => 0f
                    };

                    chart.UpdateData(serie.index, correctedIndex, v);
                    serie.GetSerieData(correctedIndex).ignore = false;
                }
            }

            chart.RefreshChart();
        }

        string[] BiomassComponents = { "Wr", "Ww", "Wb", "Wbr", "Wl" };
        Color[] biomassColors = {
            new Color(0.36f, 0.20f, 0.09f), // roots 
            new Color(0.76f, 0.60f, 0.42f), // wood 
            new Color(0.55f, 0.27f, 0.07f), // bark 
            new Color(0.65f, 0.45f, 0.25f), // branches 
            new Color(0.3f, 0.7f, 0.3f),    // leaves 
        };


        for (int standIndex = 0; standIndex < tableData.Count; standIndex++)
        {
            var chart = barCharts[2 + standIndex];
            prepareBarChart(chart, years);

            var plotData = tableData[id_stands[standIndex]][id_prescs[standIndex]];
            if (plotData == null || plotData.Count == 0) continue;

            string standId = plotData[0].id_stand;
            chart.GetChartComponent<Title>().text = $"Biomassa {standId}";

            for (int c = 0; c < BiomassComponents.Length; c++)
            {
                string comp = BiomassComponents[c];
                var serie = chart.AddSerie<Bar>($"{standId} {comp}");
                serie.stack = $"biomass_{standId}";
                serie.itemStyle.color = biomassColors[c];


                for (int j = 0; j < years.Count; j++)
                {
                    chart.AddData(serie.index, 0f);
                    serie.GetSerieData(j).ignore = true;
                }

                foreach (var entry in plotData)
                {
                    int yearIndex = years.IndexOf(entry.year);
                    int correctedIndex = chart.GetData(serie.index, yearIndex) != 0f ? yearIndex + 1 : yearIndex;

                    float v = comp switch
                    {
                        "Ww" => entry.Ww,
                        "Wb" => entry.Wb,
                        "Wbr" => entry.Wbr,
                        "Wl" => entry.Wl,
                        "Wr" => entry.Wr,
                        _ => 0f
                    };

                    chart.UpdateData(serie.index, correctedIndex, v);
                    serie.GetSerieData(correctedIndex).ignore = false;
                }
            }

            chart.RefreshChart();
        }
    }

    public void populateDDBarCharts(
    SortedDictionary<string, SortedDictionary<string, List<DDEntry>>> DDtableData,
    string[] id_stands,
    string[] id_prescs,
    int[] currentYears)
    {
        for (int standIndex = 0; standIndex < DDtableData.Count; standIndex++)
        {
            var chart = ddBarCharts[standIndex];
            var behaviour = chart.GetComponent<GraphBehaviour>();
            prepareDDBarChart(chart);

            chart.GetChartComponent<Title>().text = $"Distrubuição de diâmetros {id_stands[standIndex]}";
            var plotData = DDtableData[id_stands[standIndex]][id_prescs[standIndex]];
            if (plotData == null || plotData.Count == 0) continue;

            var entry = DDtableData[id_stands[standIndex]][id_prescs[standIndex]][currentYears[standIndex]];

            var serie = chart.AddSerie<Bar>(id_stands[standIndex]);
            serie.stack = $"DD_stand_{id_stands[standIndex]}";

            List<float> values = new List<float>();

            for (int c = 0; c < DDCategories.Length; c++)
            {
                float v = GetDDValueByIndex(entry, c);

                values.Add(v);
            }

            chart.RefreshChart();
            behaviour.SaveDDOriginalValues(values);
        }
    }

    private float GetDDValueByIndex(DDEntry entry, int index)
    {
        switch (index)
        {
            case 0: return entry.dd0;
            case 1: return entry.dd5;
            case 2: return entry.dd10;
            case 3: return entry.dd15;
            case 4: return entry.dd20;
            case 5: return entry.dd25;
            case 6: return entry.dd30;
            case 7: return entry.dd35;
            case 8: return entry.dd40;
            case 9: return entry.dd45;
            case 10: return entry.dd50;
            case 11: return entry.dd55;
            case 12: return entry.dd60;
            case 13: return entry.dd65;
            case 14: return entry.dd70;
            case 15: return entry.dd75;
            case 16: return entry.dd80;
            case 17: return entry.dd85;
            case 18: return entry.dd90;
            case 19: return entry.dd95;
            case 20: return entry.dd100;
            case 21: return entry.dd102;
            default: return 0f;
        }
    }

    private void populateMultiLineChart(SortedDictionary<string, SortedDictionary<string, List<YieldTableEntry>>> tableData,
        string[] id_stands,
        string[] id_prescs)
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
                var plotData = tableData[id_stands[plotIndex]][id_prescs[plotIndex]];
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