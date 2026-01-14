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
        "Distribuição de diâmetros"
    };

    readonly string[] addedGraphDesc = {
        "Volume extraído dívidido 5 classificações",
        "Biomassa Total dividida em 5 classes"
    };

    readonly string[] DDCategories = {
        "0", "5", "10", "15", "20", "25", "30", "35", "40", "45", "50",
        "55", "60", "65", "70", "75", "80", "85", "90", "95", "100", ">102.5"
    };

    public void receiveData(List<Dictionary<string, SortedDictionary<string, List<YieldTableEntry>>>> tableData,
        List<Dictionary<string, SortedDictionary<string, List<DDEntry>>>> DDtableData,
        int current_year1, int current_year2, string selectedId_stand1,
        string selectedId_stand2, string selectedId_presc1, string selectedId_presc2,
        bool isComparingPresc = false)
    {
        List<int> allYears = new List<int>();
        foreach (var entry in tableData.First().Values.First()[selectedId_presc1])
            allYears.Add(entry.year);
        
        if (tableData.Count > 1)
        {
            foreach (var entry in tableData.ElementAt(1).Values.First()[selectedId_presc2])
                allYears.Add(entry.year);
        }
        else if (isComparingPresc && !string.IsNullOrEmpty(selectedId_presc2))
        {
            foreach (var entry in tableData.First().Values.First()[selectedId_presc2])
                allYears.Add(entry.year);
        }

        sortedYears = allYears.Distinct().OrderBy(y => y).ToList();

        foreach (LineChart chart in lineCharts)
            prepareChart(chart);
        
        string[] id_stands = { selectedId_stand1, selectedId_stand2 };
        string[] id_prescs = { selectedId_presc1, selectedId_presc2 };

        prepareDropdown(tableData, id_prescs, isComparingPresc);
        populateLineChart(lineCharts[0], tableData, e => e.N, id_stands, id_prescs, isComparingPresc);
        populateLineChart(lineCharts[1], tableData, e => e.Nst, id_stands, id_prescs, isComparingPresc);
        populateLineChart(lineCharts[2], tableData, e => e.Ndead, id_stands, id_prescs, isComparingPresc);
        populateLineChart(lineCharts[3], tableData, e => e.hdom, id_stands, id_prescs, isComparingPresc);
        populateLineChart(lineCharts[4], tableData, e => e.dg, id_stands, id_prescs, isComparingPresc);
        populateLineChart(lineCharts[5], tableData, e => e.G, id_stands, id_prescs, isComparingPresc);
        populateLineChart(lineCharts[6], tableData, e => e.V, id_stands, id_prescs, isComparingPresc);
        populateLineChart(lineCharts[7], tableData, e => e.Vu_st, id_stands, id_prescs, isComparingPresc);

        populateBarCharts(sortedYears, tableData, id_stands, id_prescs, isComparingPresc);

        populateDDBarCharts(DDtableData, id_stands, id_prescs,
            new int[] { current_year1, current_year2 }, isComparingPresc);

        populateMultiLineChart(tableData, id_stands, id_prescs, isComparingPresc);

        int y1 = current_year1;
        int y2 = isComparingPresc ? current_year1 : current_year2;

        foreach (LineChart chart in lineCharts)
            highlightPoint(chart, y1, y2);

        foreach (LineChart chart in MultiLineCharts)
            highlightPointMultiLine(chart, y1, y2);

    }

    private void prepareDropdown(
    List<Dictionary<string, SortedDictionary<string, List<YieldTableEntry>>>> tableData,
    string[] id_prescs,
    bool isComparingPresc)
    {
        graphDropdown.ClearOptions();
        List<string> options = new List<string>();

        foreach (var desc in graphDesc)
            options.Add(desc);

        if (isComparingPresc)
        {
            var plotData1 = tableData.First().Values.First()[id_prescs[0]];
            if (plotData1 != null && plotData1.Count > 0)
            {
                string standId = plotData1.First().id_stand;

                if (!string.IsNullOrEmpty(id_prescs[0]))
                {
                    foreach (var desc in addedGraphDesc)
                        options.Add($"{desc} ({standId} - {id_prescs[0]})");
                }

                if (!string.IsNullOrEmpty(id_prescs[1]))
                {
                    foreach (var desc in addedGraphDesc)
                        options.Add($"{desc} ({standId} - {id_prescs[1]})");
                }
            }
        }
        else
        {
            int chartsToPopulate = tableData.Count > 1 ? 2 : 1;

            for (int standIndex = 0; standIndex < chartsToPopulate; standIndex++)
            {
                var dataSource = tableData.Count > 1 ? tableData.ElementAt(standIndex) : tableData.First();
                var plotData = dataSource.Values.First()[id_prescs[standIndex]];

                if (plotData != null && plotData.Count > 0)
                {
                    string standId = plotData.First().id_stand;

                    foreach (var desc in addedGraphDesc)
                        options.Add($"{desc} ({standId})");
                }
            }
        }

        graphDropdown.AddOptions(options);
        graphDropdown.RefreshShownValue();
    }

    private void populateLineChart(
        LineChart chart,
        List<Dictionary<string, SortedDictionary<string, List<YieldTableEntry>>>> tableData,
        Func<YieldTableEntry, float> valueSelector,
        string[] id_stands,
        string[] id_prescs,
        bool isComparingPresc = false)
    {
        highlightedIndex1 = -1;
        highlightedIndex2 = -1;

        var entries1 = tableData.First().Values.First()[id_prescs[0]];
        if (entries1 != null && entries1.Count > 0)
        {
            string standId1 = entries1.First().id_stand;
            string serieName1 = isComparingPresc ? $"{standId1} - {id_prescs[0]}" : standId1;
            chart.AddSerie<Line>(serieName1);
            var serie1 = chart.GetSerie(0);

            Color lineColor1 = Color.HSVToRGB(0f, 1f, 1f);
            setupSerieStyle(serie1, lineColor1);

            foreach (var entry in entries1)
            {
                chart.AddData(0, entry.year, valueSelector(entry));
            }
        }
        if (tableData.Count > 1)
        {
            // Multi-simulation case
            var entries2 = tableData.ElementAt(1).Values.First()[id_prescs[1]];
            if (entries2 != null && entries2.Count > 0)
            {
                string standId2 = entries2.First().id_stand;
                chart.AddSerie<Line>(standId2);
                var serie2 = chart.GetSerie(1);

                Color lineColor2 = Color.HSVToRGB(0.15f, 1f, 1f);
                setupSerieStyle(serie2, lineColor2);

                foreach (var entry in entries2)
                {
                    chart.AddData(1, entry.year, valueSelector(entry));
                }
            }
        }
        else if (isComparingPresc && !string.IsNullOrEmpty(id_prescs[1]))
        {
            // Comparing prescriptions within same simulation
            var entries2 = tableData.First().Values.First()[id_prescs[1]];
            if (entries2 != null && entries2.Count > 0)
            {
                string standId2 = entries2.First().id_stand;
                string serieName2 = $"{standId2} - {id_prescs[1]}";
                chart.AddSerie<Line>(serieName2);
                var serie2 = chart.GetSerie(1);

                Color lineColor2 = Color.HSVToRGB(0.15f, 1f, 1f);
                setupSerieStyle(serie2, lineColor2);

                foreach (var entry in entries2)
                {
                    chart.AddData(1, entry.year, valueSelector(entry));
                }
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

        chart.RefreshChart();
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

    private void populateBarCharts(List<int> years, 
        List<Dictionary<string, SortedDictionary<string, List<YieldTableEntry>>>> tableData,
        string[] id_stands,
        string[] id_prescs,
        bool isComparingPresc = false)
    {
        string[] VolumeComponents = { "Vu_as1", "Vu_as2", "Vu_as3", "Vu_as4", "Vu_as5" };
        Color[] volumeColors = {
            new Color(0.36f, 0.20f, 0.09f),
            new Color(0.0f, 0.4f, 0.0f),
            new Color(0.2f, 0.6f, 0.2f),
            new Color(0.8f, 0.5f, 0.2f),
            new Color(0.6f, 0.9f, 0.5f)
        };

        int chartsToPopulate = tableData.Count > 1 ? 2 : (isComparingPresc ? 2 : 1);

        for (int standIndex = 0; standIndex < chartsToPopulate; standIndex++)
        {
            var chart = barCharts[standIndex];
            prepareBarChart(chart, years);

            var dataSource = tableData.Count > 1 ? tableData.ElementAt(standIndex) : tableData.First();
            var plotData = dataSource.Values.First()[id_prescs[standIndex]];
            
            if (plotData == null || plotData.Count == 0) continue;

            string standId = plotData[0].id_stand;
            string titleSuffix = isComparingPresc ? $" - {id_prescs[standIndex]}" : "";
            chart.GetChartComponent<Title>().text = $"Volume extraído {standId}{titleSuffix}";

            for (int c = 0; c < VolumeComponents.Length; c++)
            {
                string comp = VolumeComponents[c];
                var serie = chart.AddSerie<Bar>($"{standId} {comp}");
                serie.stack = $"volume_{standId}_{standIndex}";
                serie.itemStyle.color = volumeColors[c];

                for (int j = 0; j < years.Count; j++)
                {
                    chart.AddData(serie.index, 0f);
                    serie.GetSerieData(j).ignore = true;
                }

                foreach (var entry in plotData)
                {
                    int yearIndex = years.IndexOf(entry.year);
                    if (yearIndex < 0) continue;

                    float v = comp switch
                    {
                        "Vu_as1" => entry.Vu_as1,
                        "Vu_as2" => entry.Vu_as2,
                        "Vu_as3" => entry.Vu_as3,
                        "Vu_as4" => entry.Vu_as4,
                        "Vu_as5" => entry.Vu_as5,
                        _ => 0f
                    };

                    chart.UpdateData(serie.index, yearIndex, v);
                    serie.GetSerieData(yearIndex).ignore = false;

                }
            }

            chart.RefreshChart();
        }

        // Biomass charts
        string[] BiomassComponents = { "Wr", "Ww", "Wb", "Wbr", "Wl" };
        Color[] biomassColors = {
            new Color(0.36f, 0.20f, 0.09f),
            new Color(0.76f, 0.60f, 0.42f),
            new Color(0.55f, 0.27f, 0.07f),
            new Color(0.65f, 0.45f, 0.25f),
            new Color(0.3f, 0.7f, 0.3f),
        };

        for (int standIndex = 0; standIndex < chartsToPopulate; standIndex++)
        {
            var chart = barCharts[2 + standIndex];
            prepareBarChart(chart, years);

            var dataSource = tableData.Count > 1 ? tableData.ElementAt(standIndex) : tableData.First();
            var plotData = dataSource.Values.First()[id_prescs[standIndex]];
            
            if (plotData == null || plotData.Count == 0) continue;

            string standId = plotData[0].id_stand;
            string titleSuffix = isComparingPresc ? $" - {id_prescs[standIndex]}" : "";
            chart.GetChartComponent<Title>().text = $"Biomassa {standId}{titleSuffix}";

            for (int c = 0; c < BiomassComponents.Length; c++)
            {
                string comp = BiomassComponents[c];
                var serie = chart.AddSerie<Bar>($"{standId} {comp}");
                serie.stack = $"biomass_{standId}_{standIndex}";
                serie.itemStyle.color = biomassColors[c];

                for (int j = 0; j < years.Count; j++)
                {
                    chart.AddData(serie.index, 0f);
                    serie.GetSerieData(j).ignore = true;
                }

                foreach (var entry in plotData)
                {
                    int yearIndex = years.IndexOf(entry.year);
                    if (yearIndex < 0) continue;

                    float v = comp switch
                    {
                        "Ww" => entry.Ww,
                        "Wb" => entry.Wb,
                        "Wbr" => entry.Wbr,
                        "Wl" => entry.Wl,
                        "Wr" => entry.Wr,
                        _ => 0f
                    };

                    chart.UpdateData(serie.index, yearIndex, v);
                    serie.GetSerieData(yearIndex).ignore = false;

                }
            }

            chart.RefreshChart();
        }
    }
    public void populateDDBarCharts(
        List<Dictionary<string, SortedDictionary<string, List<DDEntry>>>> DDtableData,
        string[] id_stands,
        string[] id_prescs,
        int[] currentYears,
        bool isComparingPresc)
    {
        var chart = ddBarCharts[0];
        var behaviour = chart.GetComponent<GraphBehaviour>();
        prepareDDBarChart(chart);

        chart.GetChartComponent<Title>().text = "Distribuição de diâmetros";

        int seriesToPopulate = (DDtableData.Count > 1 || isComparingPresc) ? 2 : 1;

        List<List<float>> allValues = new List<List<float>>();

        for (int standIndex = 0; standIndex < seriesToPopulate; standIndex++)
        {
            var dataSource = DDtableData.Count > 1 ? DDtableData.ElementAt(standIndex) : DDtableData.First();
            var plotData = dataSource.Values.First()[id_prescs[standIndex]];

            if (plotData == null || plotData.Count == 0) continue;

            int yearIndex = currentYears[standIndex];

            if (yearIndex < 0 || yearIndex >= plotData.Count) continue;

            var entry = plotData[yearIndex];

            string standId = entry.id_stand;
            string serieName = isComparingPresc ? $"{standId} - {id_prescs[standIndex]}" : standId;

            var serie = chart.AddSerie<Bar>(serieName);
            serie.stack = "";

            Color serieColor = Color.HSVToRGB((standIndex * 0.3f) % 1f, 0.8f, 1f);
            serie.itemStyle.color = serieColor;

            List<float> values = new List<float>();

            for (int c = 0; c < DDCategories.Length; c++)
            {
                float v = GetDDValueByIndex(entry, c);
                values.Add(v);
                chart.AddData(serie.index, v);
            }

            allValues.Add(values);
        }

        chart.RefreshChart();
        behaviour.SaveDDOriginalValues(allValues);
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

    private void populateMultiLineChart(
        List<Dictionary<string, SortedDictionary<string, List<YieldTableEntry>>>> tableData,
        string[] id_stands,
        string[] id_prescs,
        bool isComparingPresc = false)
    {
        for (int chartIndex = 0; chartIndex < MultiLineCharts.Count; chartIndex++)
        {
            var chart = MultiLineCharts[chartIndex];
            if (chart == null) continue;

            prepareChart(chart);

            highlightedIndex1 = -1;
            highlightedIndex2 = -1;

            int datasetsToPlot = tableData.Count > 1 ? 2 : (isComparingPresc ? 2 : 1);

            for (int plotIndex = 0; plotIndex < datasetsToPlot; plotIndex++)
            {
                var dataSource = tableData.Count > 1 ? tableData.ElementAt(plotIndex) : tableData.First();
                var plotData = dataSource.Values.First()[id_prescs[plotIndex]];
                
                if (plotData == null || plotData.Count == 0)
                    continue;

                string id_stand = plotData.First().id_stand;
                Color baseColor = Color.HSVToRGB((plotIndex * 0.2f) % 1f, 0.8f, 1f);
                Color secondaryColor = Color.HSVToRGB(((plotIndex * 0.2f) + 0.4f) % 1f, 0.8f, 1f);

                string prescSuffix = isComparingPresc ? $" - {id_prescs[plotIndex]}" : "";
                string serieName1, serieName2;
                Func<YieldTableEntry, float> valueSelector1, valueSelector2;

                if (chartIndex == 0)
                {
                    serieName1 = $"iV {id_stand}{prescSuffix}";
                    serieName2 = $"maiV {id_stand}{prescSuffix}";
                    valueSelector1 = e => e.iV;
                    valueSelector2 = e => e.maiV;
                }
                else
                {
                    serieName1 = $"sumNPV {id_stand}{prescSuffix}";
                    serieName2 = $"EEA {id_stand}{prescSuffix}";
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

    public void changeHighlightedYearGraphs(int year1, int year2, bool isComparing)
    {
        foreach (var chart in lineCharts)
            removeHighlight(chart);
        foreach (var chart in MultiLineCharts)
            removeHighlightMultiLine(chart);
        
        int y1 = year1;
        int y2 = isComparing ? year1 : year2;

        foreach (var chart in lineCharts)
            highlightPoint(chart, y1, y2);

        foreach (var chart in MultiLineCharts)
            highlightPointMultiLine(chart, y1, y2);

    }
}