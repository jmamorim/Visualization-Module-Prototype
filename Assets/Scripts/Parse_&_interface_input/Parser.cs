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

    readonly string[] expectedSoloTreesHeaders = { "id_stand", "id_presc", "ciclo", "Year", "t", "id_arv", "Xarv", "Yarv", "Species", "d", "h", "cw", "hbc", "status" };
    readonly string[] expectedYieldTableHeaders = { "year", "hdom", "Nst", "N", "Ndead", "G", "dg", "Vu_st", "V", "Vu_as1", "Vu_as2",
        "Vu_as3", "Vu_as4", "Vu_as5", "maiV", "iV", "Ww", "Wb", "Wbr", "Wl", "Wa", "Wr", "NPVsum", "EAA" };
    readonly string[] expectedDDTableHeaders = { "id_stand", "id_presc", "0", "5", "10", "15", "20", "25", "30", "35", "40", "45", "50", "55", "60", "65", "70", "75", "80",
        "85", "90", "95", "100",">102.5" };
    string[] lines;
    List<string> soloTreePaths = new List<string>();
    List<string> yieldTablePaths = new List<string>();
    List<string> DDTablePaths = new List<string>();
    int interval = 0;
    List<string> selectedIdStands = new List<string>();
    bool isSoloVis = false;
    int idStandIdxSt, idPrescIdxSt, cicloIdxSt, yearIdxSt, tIdxSt, idArvIdxSt, xArvIdxSt, yArvIdxSt, speciesIdxSt, dIdxSt, hIdxSt, cwIdxSt, hbcIdxSt, statusIdxSt;

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

        string[] headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();

        if (!VerifyHeaders(headers, expectedSoloTreesHeaders))
        {
            throw new ArgumentException("Incorrect headers");
        }

        idStandIdxSt = System.Array.IndexOf(headers, "id_stand");
        idPrescIdxSt = System.Array.IndexOf(headers, "id_presc");
        cicloIdxSt = System.Array.IndexOf(headers, "ciclo");
        yearIdxSt = System.Array.IndexOf(headers, "Year");
        tIdxSt = System.Array.IndexOf(headers, "t");
        idArvIdxSt = System.Array.IndexOf(headers, "id_arv");
        xArvIdxSt = System.Array.IndexOf(headers, "Xarv");
        yArvIdxSt = System.Array.IndexOf(headers, "Yarv");
        speciesIdxSt = System.Array.IndexOf(headers, "Species");
        dIdxSt = System.Array.IndexOf(headers, "d");
        hIdxSt = System.Array.IndexOf(headers, "h");
        cwIdxSt = System.Array.IndexOf(headers, "cw");
        hbcIdxSt = System.Array.IndexOf(headers, "hbc");
        statusIdxSt = System.Array.IndexOf(headers, "status");

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
            string id_stand = treeInfo[idStandIdxSt].Trim();
            string id_presc = treeInfo[idPrescIdxSt].Trim();
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
                int id_arv = int.Parse(treeInfo[idArvIdxSt].Trim());

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

                int estado = int.Parse(treeInfo[statusIdxSt].Trim());
                //presist alive status
                var previousYearInfo = currentYearIndex - 1 > 0 ? treesInfoPerYear[currentYearIndex - 1] : null;
                var previousYearWasAlive = previousYearInfo != null && (previousYearInfo[id_arv] != null ? previousYearInfo[id_arv].wasAlive : false);
                treeWasAlive[id_arv] = (estado == 0) || previousYearWasAlive;

                TreeData tree = new TreeData(
                        treeInfo[idStandIdxSt].Trim(),
                        treeInfo[idPrescIdxSt].Trim(),
                        int.Parse(treeInfo[cicloIdxSt].Trim()),
                        int.Parse(treeInfo[yearIdxSt].Trim()),
                        float.Parse(treeInfo[tIdxSt].Trim(), CultureInfo.InvariantCulture),
                        id_arv,
                        float.Parse(treeInfo[xArvIdxSt].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(treeInfo[yArvIdxSt].Trim(), CultureInfo.InvariantCulture),
                        treeInfo[speciesIdxSt].Trim(),
                        float.Parse(treeInfo[dIdxSt].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(treeInfo[hIdxSt].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(treeInfo[cwIdxSt].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(treeInfo[hbcIdxSt].Trim(), CultureInfo.InvariantCulture),
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
                int treeYear = int.Parse(treeInfo[yearIdxSt].Trim());

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
                    int id_arv = int.Parse(treeInfo[idArvIdxSt].Trim());

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

                    int treeCurYear = int.Parse(treeInfo[yearIdxSt].Trim());

                    if (treeCurYear == targetYear)
                    {
                        TreeData tree = new TreeData(
                            treeInfo[idStandIdxSt].Trim(),
                            treeInfo[idPrescIdxSt].Trim(),
                            int.Parse(treeInfo[cicloIdxSt].Trim()),
                            int.Parse(treeInfo[yearIdxSt].Trim()),
                            float.Parse(treeInfo[tIdxSt].Trim(), CultureInfo.InvariantCulture),
                            id_arv,
                            float.Parse(treeInfo[xArvIdxSt].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(treeInfo[yArvIdxSt].Trim(), CultureInfo.InvariantCulture),
                            treeInfo[speciesIdxSt].Trim(),
                            float.Parse(treeInfo[dIdxSt].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(treeInfo[hIdxSt].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(treeInfo[cwIdxSt].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(treeInfo[hbcIdxSt].Trim(), CultureInfo.InvariantCulture),
                            int.Parse(treeInfo[statusIdxSt].Trim()),
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

        int idStandIdx = System.Array.IndexOf(headers, "id_stand");
        int idPrescIdx = System.Array.IndexOf(headers, "id_presc");
        int yearIdx = System.Array.IndexOf(headers, "year");
        int nstIdx = System.Array.IndexOf(headers, "Nst");
        int nIdx = System.Array.IndexOf(headers, "N");
        int ndeadIdx = System.Array.IndexOf(headers, "Ndead");
        int hdomIdx = System.Array.IndexOf(headers, "hdom");
        int gIdx = System.Array.IndexOf(headers, "G");
        int dgIdx = System.Array.IndexOf(headers, "dg");
        int vuStIdx = System.Array.IndexOf(headers, "Vu_st");
        int vIdx = System.Array.IndexOf(headers, "V");
        int vuAs1Idx = System.Array.IndexOf(headers, "Vu_as1");
        int vuAs2Idx = System.Array.IndexOf(headers, "Vu_as2");
        int vuAs3Idx = System.Array.IndexOf(headers, "Vu_as3");
        int vuAs4Idx = System.Array.IndexOf(headers, "Vu_as4");
        int vuAs5Idx = System.Array.IndexOf(headers, "Vu_as5");
        int maiVIdx = System.Array.IndexOf(headers, "maiV");
        int iVIdx = System.Array.IndexOf(headers, "iV");
        int wwIdx = System.Array.IndexOf(headers, "Ww");
        int wbIdx = System.Array.IndexOf(headers, "Wb");
        int wbrIdx = System.Array.IndexOf(headers, "Wbr");
        int wlIdx = System.Array.IndexOf(headers, "Wl");
        int waIdx = System.Array.IndexOf(headers, "Wa");
        int wrIdx = System.Array.IndexOf(headers, "Wr");
        int npvSumIdx = System.Array.IndexOf(headers, "NPVsum");
        int eaaIdx = System.Array.IndexOf(headers, "EAA");

        int starting_year = int.Parse(lines[1].Trim().Split(',')[yearIdx].Trim());
        int ending_year = int.Parse(lines[lines.Length - 1].Trim().Split(',')[yearIdx].Trim());

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
                string id_stand = entryInfo[idStandIdx].Trim();
                string id_presc = entryInfo[idPrescIdx].Trim();

                if (id_stand == selectedIdStand)
                {
                    int entryYear = int.Parse(entryInfo[yearIdx].Trim());

                    if ((interval != 0 && targetYears.Contains(entryYear)) || interval == 0)
                    {
                        YieldTableEntry entry = new YieldTableEntry(
                            id_stand,
                            id_presc,
                            entryYear,
                            Mathf.RoundToInt(float.Parse(entryInfo[nstIdx].Trim(), CultureInfo.InvariantCulture)),
                            Mathf.RoundToInt(float.Parse(entryInfo[nIdx].Trim(), CultureInfo.InvariantCulture)),
                            Mathf.RoundToInt(float.Parse(entryInfo[ndeadIdx].Trim(), CultureInfo.InvariantCulture)),
                            float.Parse(entryInfo[hdomIdx].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[gIdx].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[dgIdx].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[vuStIdx].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[vIdx].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[vuAs1Idx].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[vuAs2Idx].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[vuAs3Idx].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[vuAs4Idx].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[vuAs5Idx].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[maiVIdx].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[iVIdx].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[wwIdx].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[wbIdx].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[wbrIdx].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[wlIdx].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[waIdx].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[wrIdx].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[npvSumIdx].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(entryInfo[eaaIdx].Trim(), CultureInfo.InvariantCulture)
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

        var headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();

        if (!VerifyHeaders(headers, expectedDDTableHeaders))
        {
            throw new ArgumentException("Incorrect headers");
        }

        int idStandIdx = System.Array.IndexOf(headers, "id_stand");
        int idPrescIdx = System.Array.IndexOf(headers, "id_presc");
        int yearIdx = System.Array.IndexOf(headers, "year");
        int dd0Idx = System.Array.IndexOf(headers, "0");
        int dd5Idx = System.Array.IndexOf(headers, "5");
        int dd10Idx = System.Array.IndexOf(headers, "10");
        int dd15Idx = System.Array.IndexOf(headers, "15");
        int dd20Idx = System.Array.IndexOf(headers, "20");
        int dd25Idx = System.Array.IndexOf(headers, "25");
        int dd30Idx = System.Array.IndexOf(headers, "30");
        int dd35Idx = System.Array.IndexOf(headers, "35");
        int dd40Idx = System.Array.IndexOf(headers, "40");
        int dd45Idx = System.Array.IndexOf(headers, "45");
        int dd50Idx = System.Array.IndexOf(headers, "50");
        int dd55Idx = System.Array.IndexOf(headers, "55");
        int dd60Idx = System.Array.IndexOf(headers, "60");
        int dd65Idx = System.Array.IndexOf(headers, "65");
        int dd70Idx = System.Array.IndexOf(headers, "70");
        int dd75Idx = System.Array.IndexOf(headers, "75");
        int dd80Idx = System.Array.IndexOf(headers, "80");
        int dd85Idx = System.Array.IndexOf(headers, "85");
        int dd90Idx = System.Array.IndexOf(headers, "90");
        int dd95Idx = System.Array.IndexOf(headers, "95");
        int dd100Idx = System.Array.IndexOf(headers, "100");
        int dd102Idx = System.Array.IndexOf(headers, ">102.5");

        int starting_year = int.Parse(lines[1].Trim().Split(',')[yearIdx].Trim());
        int ending_year = int.Parse(lines[lines.Length - 1].Trim().Split(',')[yearIdx].Trim());

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
                string id_stand = entryInfo[idStandIdx].Trim();
                string id_presc = entryInfo[idPrescIdx].Trim();

                if (id_stand == selectedIdStand)
                {
                    int entryYear = int.Parse(entryInfo[yearIdx].Trim());
                    if ((interval != 0 && targetYears.Contains(entryYear)) || interval == 0)
                    {
                        DDEntry entry = new DDEntry(
                        id_stand,
                        id_presc,
                        float.Parse(entryInfo[dd0Idx].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd5Idx].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd10Idx].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd15Idx].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd20Idx].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd25Idx].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd30Idx].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd35Idx].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd40Idx].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd45Idx].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd50Idx].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd55Idx].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd60Idx].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd65Idx].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd70Idx].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd75Idx].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd80Idx].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd85Idx].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd90Idx].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd95Idx].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd100Idx].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(entryInfo[dd102Idx].Trim(), CultureInfo.InvariantCulture)
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