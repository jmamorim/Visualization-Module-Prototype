using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Parser : MonoBehaviour
{
    public TMP_Dropdown selectedSim1, selectedSim2;
    public TMP_InputField intervalInputField;
    public SimMetadata simMetadata;
    public InputAndParsedData so;
    //dropdowns
    public GameObject dpsolo, dp1, dp2;

    IdStandsDropdown idStandsDropdownSolo, idStandsDropdown1, idStandsDropdown2;

    readonly string[] expectedSoloTreesHeaders = { "id_stand", "id_presc", "ciclo", "Year", "t", "id_arv", "Xarv", "Yarv", "Species", "d", "h", "cw", " hbc", "status" };
    readonly string[] expectedYieldTableHeaders = { "year", "hdom", "Nst", "N", "Ndead", "G", "dg", "Vu_st", "Vst", "Vu_as1", "Vu_as2",
        "Vu_as3", "Vu_as4", "Vu_as5", "maiV", "iV", "Ww", "Wb", "Wbr", "Wl", "Wa", "Wr", "NPVsum", "EAA" };
    readonly string[] expectedDDTableHeaders = { "id_stand", "id_presc", "0", "5", "10", "15", "20", "25", "30", "35", "40", "45", "50", "55", "60", "65", "70", "75", "80",
        "85", "90", "95", "100",">102.5" };
    string[] lines;
    List<string> soloTreePaths = new List<string>();
    List<string> yieldTablePaths = new List<string>();
    List<string> DDTablePaths = new List<string>();
    int interval = 0;
    const int idStand = 0, idPresc = 1, cicloIndex = 2, yearIndex = 3, tIndex = 4, XarvIndex = 7, YarvIndex = 8, speciesIndex = 9, dIndex = 10, hIndex = 11, cwIndex = 12, hbcIndex = 13,
        estadoIndex = 15, tableId_stand = 0, tableId_presc = 3, tableYearIndex = 6, tablenstIndex = 14, tablenIndex = 15, tablendeadIndex = 16, tablehdomIndex = 13,
        tablegIndex = 19, tabledgIndex = 20, tablevu_stIndex = 21, tablevIndex = 24, tablevu_as1Index = 30, tablevu_as2Index = 31, tablevu_as3Index = 32, tablevu_as4Index = 33,
        tablevu_as5Index = 34, tablemaiVIndex = 38, tableiVIndex = 39, tablewwIndex = 40, tablewbIndex = 41, tablewbrIndex = 42, tablewlIndex = 43, tablewaIndex = 44,
        tablewrIndex = 45, tablenpvsumIndex = 66, tableeeaIndex = 67, ddId_standIndex = 0, ddId_prescIndex = 1, ddYearIndex = 2, dd0Index = 6, dd5Index = 7, dd10Index = 8,
        dd15index = 9, dd20Index = 10, dd25Index = 11, dd30Index = 12, dd35Index = 13, dd40Index = 14, dd45Index = 15, dd50Index = 16, dd55Index = 17, dd60Index = 18, dd65Index = 19,
        dd70Index = 20, dd75Index = 21, dd80Index = 22, dd85Index = 23, dd90Index = 24, dd95Index = 25, dd100Index = 26, dd102Index = 27;
    List<string> selectedIdStands = new List<string>();
    bool isSoloVis = false;

    private void Start()
    {
        isSoloVis = dpsolo != null;
        if (isSoloVis)
            idStandsDropdownSolo = dpsolo.GetComponent<IdStandsDropdown>();
        else
        {
            idStandsDropdown1 = dp1.GetComponent<IdStandsDropdown>();
            idStandsDropdown2 = dp2.GetComponent<IdStandsDropdown>();
        }
    }

    public void parse()
    {
        if (!string.IsNullOrEmpty(intervalInputField.text))
        {
            if (!int.TryParse(intervalInputField.text, out interval) || interval < 0)
            {
                return;
            }
        }

        List<Dictionary<string, SortedDictionary<string, List<SortedDictionary<int, TreeData>>>>> outputSoloTreesDataList = new List<Dictionary<string, SortedDictionary<string, List<SortedDictionary<int, TreeData>>>>>();
        List<Dictionary<string, SortedDictionary<string, List<YieldTableEntry>>>> outputYieldTableDataList = new List<Dictionary<string, SortedDictionary<string, List<YieldTableEntry>>>>();
        List<Dictionary<string, SortedDictionary<string, List<DDEntry>>>> outputDDTableDataList = new List<Dictionary<string, SortedDictionary<string, List<DDEntry>>>>();
        List<(int, List<float>)> shapeData = new List<(int, List<float>)>();

        if (isSoloVis)
        {
            var dropdownSolo = idStandsDropdownSolo.GetComponent<TMP_Dropdown>();
            if (dropdownSolo.options.Count() == 0)
            {
                return;
            }
            string selectedIdStandSolo = dropdownSolo.options[dropdownSolo.value].text;

            var siminfoSolo = simMetadata.simulations[selectedSim1.options[selectedSim1.value].text];

            var simPlotDimensionsSolo = siminfoSolo.plotDataByIdPar[selectedIdStandSolo];

            selectedIdStands.Add(selectedIdStandSolo);

            var plotDimensionsDataSolo = new List<float>();

            if (simPlotDimensionsSolo.plotShape == 0)
                plotDimensionsDataSolo.Add(simPlotDimensionsSolo.area);
            else if (simPlotDimensionsSolo.plotShape == 4)
            {
                plotDimensionsDataSolo.Add(simPlotDimensionsSolo.minX);
                plotDimensionsDataSolo.Add(simPlotDimensionsSolo.maxX);
                plotDimensionsDataSolo.Add(simPlotDimensionsSolo.minY);
                plotDimensionsDataSolo.Add(simPlotDimensionsSolo.maxY);
            }
            else
            {
                plotDimensionsDataSolo.Add(simPlotDimensionsSolo.length1);
                plotDimensionsDataSolo.Add(simPlotDimensionsSolo.length2);
            }

            var plotDataSolo = (simPlotDimensionsSolo.plotShape, plotDimensionsDataSolo);

            soloTreePaths.Add(siminfoSolo.soloTreesPath);
            yieldTablePaths.Add(siminfoSolo.yieldTablePath);
            DDTablePaths.Add(siminfoSolo.ddTablePath);

            shapeData.Add(plotDataSolo);
        }
        else
        {
            var dropdown1 = idStandsDropdown1.GetComponent<TMP_Dropdown>();
            var dropdown2 = idStandsDropdown2.GetComponent<TMP_Dropdown>();
            if (dropdown1.options.Count() == 0)
            {
                return;
            }
            if (dropdown2.options.Count() == 0)
            {
                return;
            }

            string selectedIdStand1 = dropdown1.options[dropdown1.value].text;

            var siminfo1 = simMetadata.simulations[selectedSim1.options[selectedSim1.value].text];

            var simPlotDimensions1 = siminfo1.plotDataByIdPar[selectedIdStand1];

            selectedIdStands.Add(selectedIdStand1);

            var plotDimensionsData1 = new List<float>();

            if (simPlotDimensions1.plotShape == 0)
                plotDimensionsData1.Add(simPlotDimensions1.area);
            else if (simPlotDimensions1.plotShape == 4)
            {
                plotDimensionsData1.Add(simPlotDimensions1.minX);
                plotDimensionsData1.Add(simPlotDimensions1.maxX);
                plotDimensionsData1.Add(simPlotDimensions1.minY);
                plotDimensionsData1.Add(simPlotDimensions1.maxY);
            }
            else
            {
                plotDimensionsData1.Add(simPlotDimensions1.length1);
                plotDimensionsData1.Add(simPlotDimensions1.length2);
            }

            var plotData1 = (simPlotDimensions1.plotShape, plotDimensionsData1);

            soloTreePaths.Add(siminfo1.soloTreesPath);
            yieldTablePaths.Add(siminfo1.yieldTablePath);
            DDTablePaths.Add(siminfo1.ddTablePath);

            shapeData.Add(plotData1);

            if (dropdown2.options.Count() > 0)
            {
                string selectedIdStand2 = dropdown2.options[dropdown2.value].text;
                var siminfo2 = simMetadata.simulations[selectedSim2.options[selectedSim2.value].text];

                var simPlotDimensions2 = siminfo2.plotDataByIdPar[selectedIdStand2];

                selectedIdStands.Add(selectedIdStand2);

                var plotDimensionsData2 = new List<float>();

                if (simPlotDimensions2.plotShape == 0)
                    plotDimensionsData2.Add(simPlotDimensions2.area);
                else if (simPlotDimensions2.plotShape == 4)
                {
                    plotDimensionsData2.Add(simPlotDimensions2.minX);
                    plotDimensionsData2.Add(simPlotDimensions2.maxX);
                    plotDimensionsData2.Add(simPlotDimensions2.minY);
                    plotDimensionsData2.Add(simPlotDimensions2.maxY);
                }
                else
                {
                    plotDimensionsData2.Add(simPlotDimensions2.length1);
                    plotDimensionsData2.Add(simPlotDimensions2.length2);
                }

                var plotData2 = (simPlotDimensions2.plotShape, plotDimensionsData2);

                soloTreePaths.Add(siminfo2.soloTreesPath);
                yieldTablePaths.Add(siminfo2.yieldTablePath);
                DDTablePaths.Add(siminfo2.ddTablePath);

                shapeData.Add(plotData2);
            }
        }

        for (int i = 0; i < soloTreePaths.Count; i++)
        {
            Dictionary<string, SortedDictionary<string, List<SortedDictionary<int, TreeData>>>> soloTreesData = new Dictionary<string, SortedDictionary<string, List<SortedDictionary<int, TreeData>>>>();
            Dictionary<string, SortedDictionary<string, List<YieldTableEntry>>> yieldTableData = new Dictionary<string, SortedDictionary<string, List<YieldTableEntry>>>();
            Dictionary<string, SortedDictionary<string, List<DDEntry>>> ddTableData = new Dictionary<string, SortedDictionary<string, List<DDEntry>>>();

            parseSoloTrees(soloTreesData, soloTreePaths[i], selectedIdStands[i]);
            parseYieldTable(yieldTableData, yieldTablePaths[i], selectedIdStands[i]);
            parseDDTable(ddTableData, DDTablePaths[i], selectedIdStands[i]);

            outputSoloTreesDataList.Add(soloTreesData);
            outputYieldTableDataList.Add(yieldTableData);
            outputDDTableDataList.Add(ddTableData);
        }

        so.simIds = new List<string> { selectedSim1.options[selectedSim1.value].text, selectedSim2 != null ? selectedSim2.options[selectedSim2.value].text : "" };
        so.outputSoloTreesData = outputSoloTreesDataList;
        so.outputYieldTable = outputYieldTableDataList;
        so.outputDDTable = outputDDTableDataList;
        so.plotShapeAndDimensions = shapeData;
        SceneManager.LoadScene(1);//GOTO VISUALIZATION SCENE
    }

    private void parseSoloTrees(Dictionary<string, SortedDictionary<string, List<SortedDictionary<int, TreeData>>>> output, string soloTreePath, string selectedIdStand)
    {
        if (string.IsNullOrEmpty(soloTreePath))
        {
            throw new ArgumentException("No solo trees file selected");
        }

        lines = File.ReadAllLines(soloTreePath);

        if (lines.Length == 0)
        {
            throw new ArgumentException("File is empty");
        }

        string[] headers = lines[0].Trim().Split(',');

        if (!VerifyHeaders(headers, expectedSoloTreesHeaders))
        {
            throw new ArgumentException("Incorrect headers");
        }

        int starting_year = int.Parse(lines[1].Trim().Split(',')[3].Trim());
        int ending_year = int.Parse(lines[lines.Length - 1].Trim().Split(',')[3].Trim());
        Debug.Log($"parseSoloTrees: selectedIdStand={selectedIdStand}, starting_year={starting_year}, ending_year={ending_year}");

        if (interval > (ending_year - starting_year))
        {
            throw new ArgumentException("Interval is greater than the planing horizon");
        }

        var standPrescGroups = new Dictionary<string, Dictionary<string, List<string[]>>>();

        for (int i = 1; i < lines.Length; i++)
        {
            string[] treeInfo = lines[i].Trim().Split(',');
            string id_stand = treeInfo[idStand].Trim();
            string id_presc = treeInfo[idPresc].Trim();
            if (id_stand == selectedIdStand)
            {

                if (!standPrescGroups.ContainsKey(id_stand))
                {
                    standPrescGroups[id_stand] = new Dictionary<string, List<string[]>>();
                }
                if (!standPrescGroups[id_stand].ContainsKey(id_presc))
                {
                    standPrescGroups[id_stand][id_presc] = new List<string[]>();
                }

                standPrescGroups[id_stand][id_presc].Add(treeInfo);
            }
        }

        foreach (var standKvp in standPrescGroups)
        {
            string id_stand = standKvp.Key;

            if (!output.ContainsKey(id_stand))
            {
                output[id_stand] = new SortedDictionary<string, List<SortedDictionary<int, TreeData>>>();
            }

            foreach (var prescKvp in standKvp.Value)
            {
                string id_presc = prescKvp.Key;
                List<string[]> treeLines = prescKvp.Value;

                List<SortedDictionary<int, TreeData>> treesInfoPerYear = ProcessTreeLines(treeLines, starting_year, ending_year);

                output[id_stand][id_presc] = treesInfoPerYear;
            }
        }
    }

    private List<SortedDictionary<int, TreeData>> ProcessTreeLines(List<string[]> treeLines, int starting_year, int ending_year)
    {
        Debug.Log($"ProcessTreeLines: starting_year={starting_year}, ending_year={ending_year}, interval={interval}");
        List<SortedDictionary<int, TreeData>> treesInfoPerYear = new List<SortedDictionary<int, TreeData>>();

        if (interval == 0)
        {
            int currentYearIndex = -1;
            int lastTreeId = -1;
            SortedDictionary<int, TreeData> currentYearTrees = null;
            Dictionary<int, float> treeRotations = new Dictionary<int, float>();
            Dictionary<int, bool> treeWasAlive = new Dictionary<int, bool>();

            foreach (string[] treeInfo in treeLines)
            {
                int id_arv = int.Parse(treeInfo[6].Trim());

                //for year repetition
                if (id_arv <= lastTreeId)
                {
                    currentYearIndex++;
                    currentYearTrees = new SortedDictionary<int, TreeData>();
                    treesInfoPerYear.Add(currentYearTrees);
                }
                //initialization
                else if (currentYearIndex == -1)
                {
                    currentYearIndex = 0;
                    currentYearTrees = new SortedDictionary<int, TreeData>();
                    treesInfoPerYear.Add(currentYearTrees);
                }

                lastTreeId = id_arv;

                float rotation;
                if (treeRotations.ContainsKey(id_arv))
                {
                    rotation = treeRotations[id_arv];
                }
                else
                {
                    rotation = UnityEngine.Random.Range(0f, 360f);
                    treeRotations[id_arv] = rotation;
                }

                bool wasAlive = treeWasAlive.ContainsKey(id_arv) ? treeWasAlive[id_arv] : false;

                int estado = int.Parse(treeInfo[estadoIndex].Trim());
                //presist alive status
                var previousYearInfo = currentYearIndex - 1 > 0 ? treesInfoPerYear[currentYearIndex - 1] : null;
                var previousYearWasAlive = previousYearInfo != null && (previousYearInfo[id_arv] != null ? previousYearInfo[id_arv].wasAlive : false);
                treeWasAlive[id_arv] = (estado == 0) || previousYearWasAlive;

                TreeData tree = new TreeData(
                        treeInfo[idStand].Trim(),
                        treeInfo[idPresc].Trim(),
                        int.Parse(treeInfo[cicloIndex].Trim()),
                        int.Parse(treeInfo[yearIndex].Trim()),
                        float.Parse(treeInfo[tIndex].Trim(), CultureInfo.InvariantCulture),
                        id_arv,
                        float.Parse(treeInfo[XarvIndex].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(treeInfo[YarvIndex].Trim(), CultureInfo.InvariantCulture),
                        treeInfo[speciesIndex].Trim(),
                        float.Parse(treeInfo[dIndex].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(treeInfo[hIndex].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(treeInfo[cwIndex].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(treeInfo[hbcIndex].Trim(), CultureInfo.InvariantCulture),
                        estado,
                        rotation,
                        wasAlive
                    );

                currentYearTrees[id_arv] = tree;
            }
        }
        else
        {
            var yearGroups = new SortedDictionary<int, SortedDictionary<int, TreeData>>();
            Dictionary<int, float> treeRotations = new Dictionary<int, float>();

            int year = starting_year;
            while (year <= ending_year)
            {
                yearGroups[year] = new SortedDictionary<int, TreeData>();
                Debug.Log($"yearGroups key added: {year}");
                year += interval;
            }
            if (!yearGroups.ContainsKey(ending_year))
            {
                yearGroups[ending_year] = new SortedDictionary<int, TreeData>();
            }

            foreach (string[] treeInfo in treeLines)
            {
                int treeYear = int.Parse(treeInfo[yearIndex].Trim());

                int targetYear = starting_year;

                foreach (int y in yearGroups.Keys)
                {
                    if (y <= treeYear)
                    {
                        targetYear = y;
                    }
                    else
                    {
                        break;
                    }
                }

                if (yearGroups.ContainsKey(targetYear))
                {
                    int id_arv = int.Parse(treeInfo[6].Trim());

                    float rotation;
                    if (treeRotations.ContainsKey(id_arv))
                    {
                        rotation = treeRotations[id_arv];
                    }
                    else
                    {
                        rotation = UnityEngine.Random.Range(0f, 360f);
                        treeRotations[id_arv] = rotation;
                    }

                    int treeCurYear = int.Parse(treeInfo[yearIndex].Trim());

                    if (treeCurYear == targetYear)
                    {
                        TreeData tree = new TreeData(
                            treeInfo[idStand].Trim(),
                            treeInfo[idPresc].Trim(),
                            int.Parse(treeInfo[cicloIndex].Trim()),
                            int.Parse(treeInfo[yearIndex].Trim()),
                            float.Parse(treeInfo[tIndex].Trim(), CultureInfo.InvariantCulture),
                            id_arv,
                            float.Parse(treeInfo[XarvIndex].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(treeInfo[YarvIndex].Trim(), CultureInfo.InvariantCulture),
                            treeInfo[speciesIndex].Trim(),
                            float.Parse(treeInfo[dIndex].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(treeInfo[hIndex].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(treeInfo[cwIndex].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(treeInfo[hbcIndex].Trim(), CultureInfo.InvariantCulture),
                            int.Parse(treeInfo[estadoIndex].Trim()),
                            rotation,
                            false
                        );

                        yearGroups[targetYear][id_arv] = tree;
                    }
                }
            }

            foreach (var yearDict in yearGroups.Values)
            {
                treesInfoPerYear.Add(yearDict);
            }
        }

        return treesInfoPerYear;
    }

    private SortedSet<int> BuildTargetYears(int starting_year, int ending_year)
    {
        if (interval <= 0) return null;

        var targetYears = new SortedSet<int>();
        int year = starting_year;
        while (year <= ending_year)
        {
            targetYears.Add(year);
            year += interval;
        }
        if (!targetYears.Contains(ending_year))
        {
            targetYears.Add(ending_year);
        }
        return targetYears;
    }

    private int GetTargetYear(int entryYear, int starting_year, SortedSet<int> targetYears)
    {
        int targetYear = starting_year;
        foreach (int y in targetYears)
        {
            if (y <= entryYear)
                targetYear = y;
            else
                break;
        }
        return targetYear;
    }

    private void parseYieldTable(Dictionary<string, SortedDictionary<string, List<YieldTableEntry>>> output, string yieldTablePath, string selectedIdStand)
    {
        if (string.IsNullOrEmpty(yieldTablePath))
        {
            throw new ArgumentException("No yield table file selected");
        }

        lines = File.ReadAllLines(yieldTablePath);

        if (lines.Length == 0)
        {
            throw new ArgumentException("File is empty");
        }

        string[] headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();

        if (!VerifyHeaders(headers, expectedYieldTableHeaders))
        {
            throw new ArgumentException("Incorrect headers");
        }

        int starting_year = int.Parse(lines[1].Trim().Split(',')[tableYearIndex].Trim());
        int ending_year = int.Parse(lines[lines.Length - 1].Trim().Split(',')[tableYearIndex].Trim());

        if (interval > (ending_year - starting_year))
        {
            throw new ArgumentException("Interval is greater than the planing horizon");
        }

        SortedSet<int> targetYears = BuildTargetYears(starting_year, ending_year);

        var standPrescGroups = new Dictionary<string, Dictionary<string, List<YieldTableEntry>>>();

        var intervalBuckets = new Dictionary<string, Dictionary<string, SortedDictionary<int, YieldTableEntry>>>();

        for (int i = 1; i < lines.Length; i++)
        {
            string[] entryInfo = lines[i].Split(',').Select(s => s.Trim()).ToArray();

            // Fixes weird inaccuracies in the csv file like ending with a dot or empty fields
            for (int j = 0; j < entryInfo.Length; j++)
            {
                string s = entryInfo[j];
                if (string.IsNullOrWhiteSpace(s))
                {
                    entryInfo[j] = "0";
                }
                else if (s.EndsWith("."))
                {
                    entryInfo[j] = s + "0";
                }
            }

            try
            {
                string id_stand = entryInfo[tableId_stand].Trim();
                string id_presc = entryInfo[tableId_presc].Trim();

                if (id_stand == selectedIdStand)
                {
                    int entryYear = int.Parse(entryInfo[tableYearIndex].Trim());

                    if ((interval != 0 && targetYears.Contains(entryYear)) || interval == 0)
                    {
                        YieldTableEntry entry = new YieldTableEntry(
                            id_stand,
                            id_presc,
                            entryYear,
                            Mathf.RoundToInt(float.Parse(entryInfo[tablenstIndex].Trim(), CultureInfo.InvariantCulture)),
                            Mathf.RoundToInt(float.Parse(entryInfo[tablenIndex].Trim(), CultureInfo.InvariantCulture)),
                            Mathf.RoundToInt(float.Parse(entryInfo[tablendeadIndex].Trim(), CultureInfo.InvariantCulture)),
                            float.Parse(entryInfo[tablehdomIndex].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[tablegIndex].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[tabledgIndex].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[tablevu_stIndex].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[tablevIndex].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[tablevu_as1Index].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[tablevu_as2Index].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[tablevu_as3Index].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[tablevu_as4Index].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[tablevu_as5Index].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[tablemaiVIndex].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[tableiVIndex].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[tablewwIndex].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[tablewbIndex].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[tablewbrIndex].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[tablewlIndex].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[tablewaIndex].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[tablewrIndex].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[tablenpvsumIndex].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[tableeeaIndex].Trim(), CultureInfo.InvariantCulture)
                        );

                        if (targetYears == null)
                        {
                            if (!standPrescGroups.ContainsKey(id_stand))
                                standPrescGroups[id_stand] = new Dictionary<string, List<YieldTableEntry>>();
                            if (!standPrescGroups[id_stand].ContainsKey(id_presc))
                                standPrescGroups[id_stand][id_presc] = new List<YieldTableEntry>();

                            standPrescGroups[id_stand][id_presc].Add(entry);
                        }
                        else
                        {
                            int targetYear = GetTargetYear(entryYear, starting_year, targetYears);

                            if (!intervalBuckets.ContainsKey(id_stand))
                                intervalBuckets[id_stand] = new Dictionary<string, SortedDictionary<int, YieldTableEntry>>();
                            if (!intervalBuckets[id_stand].ContainsKey(id_presc))
                                intervalBuckets[id_stand][id_presc] = new SortedDictionary<int, YieldTableEntry>();

                            intervalBuckets[id_stand][id_presc][targetYear] = entry;
                        }
                    }
                }
            }
            catch (FormatException fe)
            {
                Debug.LogError($"Format error parsing line {i}: {fe.Message}\nOffending data: {string.Join(" | ", entryInfo)}");
            }
            catch (IndexOutOfRangeException ioe)
            {
                Debug.LogError($"Index out of range on line {i}: {ioe.Message}\nLine has {entryInfo.Length} columns.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error parsing line {i}: {ex.Message}");
            }
        }

        if (targetYears != null)
        {
            foreach (var standKvp in intervalBuckets)
            {
                if (!standPrescGroups.ContainsKey(standKvp.Key))
                    standPrescGroups[standKvp.Key] = new Dictionary<string, List<YieldTableEntry>>();

                foreach (var prescKvp in standKvp.Value)
                {
                    standPrescGroups[standKvp.Key][prescKvp.Key] = prescKvp.Value.Values.ToList();
                }
            }
        }

        foreach (var standKvp in standPrescGroups)
        {
            string id_stand = standKvp.Key;

            if (!output.ContainsKey(id_stand))
            {
                output[id_stand] = new SortedDictionary<string, List<YieldTableEntry>>();
            }

            foreach (var prescKvp in standKvp.Value)
            {
                string id_presc = prescKvp.Key;
                output[id_stand][id_presc] = prescKvp.Value;
            }
        }
    }

    private void parseDDTable(Dictionary<string, SortedDictionary<string, List<DDEntry>>> output, string DDTablePath, string selectedIdStand)
    {
        if (string.IsNullOrEmpty(DDTablePath))
        {
            throw new ArgumentException("No DD table file selected");
        }

        lines = File.ReadAllLines(DDTablePath);

        if (lines.Length == 0)
        {
            throw new ArgumentException("File is empty");
        }

        var headers = File.ReadLines(DDTablePath).First().Trim().Split(',')
                    .Select(h => h.Trim())
                    .ToArray();

        if (!VerifyHeaders(headers, expectedDDTableHeaders))
        {
            throw new ArgumentException("Incorrect headers");
        }

        int starting_year = int.Parse(lines[1].Trim().Split(',')[ddYearIndex].Trim());
        int ending_year = int.Parse(lines[lines.Length - 1].Trim().Split(',')[ddYearIndex].Trim());

        if (interval > (ending_year - starting_year))
        {
            throw new ArgumentException("Interval is greater than the planing horizon");
        }

        SortedSet<int> targetYears = BuildTargetYears(starting_year, ending_year);

        var standPrescGroups = new Dictionary<string, Dictionary<string, List<DDEntry>>>();

        var intervalBuckets = new Dictionary<string, Dictionary<string, SortedDictionary<int, DDEntry>>>();

        for (int i = 1; i < lines.Length; i++)
        {
            string[] entryInfo = lines[i].Split(',').Select(s => s.Trim()).ToArray();

            for (int j = 0; j < entryInfo.Length; j++)
            {
                string s = entryInfo[j];
                if (string.IsNullOrWhiteSpace(s))
                {
                    entryInfo[j] = "0";
                }
                else if (s.EndsWith("."))
                {
                    entryInfo[j] = s + "0";
                }
            }

            try
            {
                string id_stand = entryInfo[ddId_standIndex].Trim();
                string id_presc = entryInfo[ddId_prescIndex].Trim();

                if (id_stand == selectedIdStand)
                {
                    int entryYear = int.Parse(entryInfo[ddYearIndex].Trim());
                    if ((interval != 0 && targetYears.Contains(entryYear)) || interval == 0)
                    {
                        DDEntry entry = new DDEntry(
                        id_stand,
                        id_presc,
                        float.Parse(entryInfo[dd0Index].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd5Index].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd10Index].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd15index].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd20Index].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd25Index].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd30Index].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd35Index].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd40Index].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd45Index].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd50Index].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd55Index].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd60Index].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd65Index].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd70Index].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd75Index].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd80Index].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd85Index].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd90Index].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd95Index].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd100Index].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd102Index].Trim(), CultureInfo.InvariantCulture)
                    );

                        if (targetYears == null)
                        {
                            if (!standPrescGroups.ContainsKey(id_stand))
                                standPrescGroups[id_stand] = new Dictionary<string, List<DDEntry>>();
                            if (!standPrescGroups[id_stand].ContainsKey(id_presc))
                                standPrescGroups[id_stand][id_presc] = new List<DDEntry>();

                            standPrescGroups[id_stand][id_presc].Add(entry);
                        }
                        else
                        {
                            int targetYear = GetTargetYear(entryYear, starting_year, targetYears);

                            if (!intervalBuckets.ContainsKey(id_stand))
                                intervalBuckets[id_stand] = new Dictionary<string, SortedDictionary<int, DDEntry>>();
                            if (!intervalBuckets[id_stand].ContainsKey(id_presc))
                                intervalBuckets[id_stand][id_presc] = new SortedDictionary<int, DDEntry>();

                            intervalBuckets[id_stand][id_presc][targetYear] = entry;
                        }
                    }
                }
            }
            catch (FormatException fe)
            {
                Debug.LogError($"Format error parsing line {i}: {fe.Message}\nOffending data: {string.Join(" | ", entryInfo)}");
            }
            catch (IndexOutOfRangeException ioe)
            {
                Debug.LogError($"Index out of range on line {i}: {ioe.Message}\nLine has {entryInfo.Length} columns.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error parsing line {i}: {ex.Message}");
            }
        }

        if (targetYears != null)
        {
            foreach (var standKvp in intervalBuckets)
            {
                if (!standPrescGroups.ContainsKey(standKvp.Key))
                    standPrescGroups[standKvp.Key] = new Dictionary<string, List<DDEntry>>();

                foreach (var prescKvp in standKvp.Value)
                {
                    standPrescGroups[standKvp.Key][prescKvp.Key] = prescKvp.Value.Values.ToList();
                }
            }
        }

        foreach (var standKvp in standPrescGroups)
        {
            string id_stand = standKvp.Key;

            if (!output.ContainsKey(id_stand))
            {
                output[id_stand] = new SortedDictionary<string, List<DDEntry>>();
            }

            foreach (var prescKvp in standKvp.Value)
            {
                string id_presc = prescKvp.Key;
                output[id_stand][id_presc] = prescKvp.Value;
            }
        }
    }

    bool VerifyHeaders(string[] headers, string[] expectedHeaders)
    {
        foreach (string h in expectedHeaders)
        {
            if (!headers.Contains(h))
            {
                Debug.LogError($"Missing header: {h}");
                return false;
            }
        }
        return true;
    }
}