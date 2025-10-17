using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using XCharts.Runtime;

//class to generate the line graphs for the yield table data
public class GraphGenerator : MonoBehaviour
{
    //public BarChart diameterChart, heightChart;
    public LineChart NChart, NstChart, NDeadChart;

    bool canExpand = true;

    public void receiveData(List<List<YieldTableEntry>> tableData)
    {
        NChart.ClearData();

        HashSet<int> allYears = new HashSet<int>();
        foreach (var seriesData in tableData)
        {
            foreach (var entry in seriesData)
                allYears.Add(entry.year);
        }

        List<int> sortedYears = new List<int>(allYears);
        sortedYears.Sort();

        PopulateLineChart(NChart, sortedYears, tableData, e => e.N);
        PopulateLineChart(NstChart, sortedYears, tableData, e => e.Nst);
        PopulateLineChart(NDeadChart, sortedYears, tableData, e => e.Ndead);
    }

    private void PopulateLineChart(
        LineChart chart,
        List<int> sortedYears,
        List<List<YieldTableEntry>> tableData,
        System.Func<YieldTableEntry, float> valueSelector)
    {
        chart.ClearData();

        // Add all years as X-axis labels
        foreach (int year in sortedYears)
            chart.AddXAxisData(year.ToString());

        // Create one line (series) per dataset
        for (int i = 0; i < tableData.Count; i++)
        {
            chart.AddSerie<Line>($"Plot{i + 1}");

            var serie = chart.GetSerie(i);
            serie.lineType = LineType.Normal;
            serie.symbol.show = true;
            serie.lineStyle.width = 2f;

            Color lineColor = Color.HSVToRGB((i * 0.25f) % 1f, 0.8f, 0.9f);
            serie.lineStyle.color = lineColor;

            Dictionary<int, float> yearToValue = new Dictionary<int, float>();
            foreach (var entry in tableData[i])
                yearToValue[entry.year] = valueSelector(entry);

            foreach (int year in sortedYears)
            {
                if (yearToValue.TryGetValue(year, out float value))
                    chart.AddData(i, value);
                else
                    chart.AddData(i, 0);
            }
        }

        chart.RefreshChart();
    }


    public bool canExpandGraph()
    {
        return canExpand;
    }

    public void setCanExpand(bool value)
    {
        canExpand = value;
    }

    /*private void ClearCharts()
    {
        diameterChart.ClearData();
        heightChart.ClearData();
    }

    private Dictionary<int, int> CalculateFrequencyDistribution(
        SortedDictionary<int, TreeData> data,
        System.Func<TreeData, float> valueSelector)
    {
        var frequencyData = new Dictionary<int, int>();

        foreach (TreeData tree in data.Values)
        {
            float value = valueSelector(tree);
            int classIndex = (int)(value / CLASS_WIDTH);

            if (!frequencyData.ContainsKey(classIndex))
                frequencyData[classIndex] = 0;

            frequencyData[classIndex]++;
        }

        return frequencyData;
    }

    private void PopulateChart(BarChart chart, Dictionary<int, int> data, string seriesLabel)
    {
        PrepareChart(chart);

        var sortedClasses = GetSortedClasses(data);
        var series = chart.AddSerie<Bar>(seriesLabel);

        AddDataToChart(chart, series, sortedClasses, data);
    }

    private void PrepareChart(BarChart chart)
    {
        chart.RemoveData();
        chart.series.Clear();
    }

    private List<int> GetSortedClasses(Dictionary<int, int> data)
    {
        var sortedClasses = new List<int>(data.Keys);
        sortedClasses.Sort();
        return sortedClasses;
    }

    private void AddDataToChart(BarChart chart, Bar series, List<int> sortedClasses, Dictionary<int, int> data)
    {
        foreach (int classIndex in sortedClasses)
        {
            string label = CreateClassLabel(classIndex);
            chart.AddXAxisData(label);
            series.AddData(data[classIndex]);
        }
    }

    private string CreateClassLabel(int classIndex)
    {
        int lowerBound = classIndex * CLASS_WIDTH;
        int upperBound = (classIndex + 1) * CLASS_WIDTH;
        return $"{lowerBound}-{upperBound}";
    }*/

}