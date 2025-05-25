using System.Collections.Generic;
using UnityEngine;
using XCharts.Runtime;

public class GraphGenerator : MonoBehaviour
{
    public BarChart diameterChart, heightChart;
    private const int CLASS_WIDTH = 10;

    public void receiveData(SortedDictionary<int, Tree> data)
    {
        ClearCharts();

        var diameterData = CalculateFrequencyDistribution(data, tree => tree.d);
        var heightData = CalculateFrequencyDistribution(data, tree => tree.h);

        PopulateChart(diameterChart, diameterData, "Diameter");
        PopulateChart(heightChart, heightData, "Height");
    }

    private void ClearCharts()
    {
        diameterChart.ClearData();
        heightChart.ClearData();
    }

    private Dictionary<int, int> CalculateFrequencyDistribution(
        SortedDictionary<int, Tree> data,
        System.Func<Tree, float> valueSelector)
    {
        var frequencyData = new Dictionary<int, int>();

        foreach (Tree tree in data.Values)
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
    }
}