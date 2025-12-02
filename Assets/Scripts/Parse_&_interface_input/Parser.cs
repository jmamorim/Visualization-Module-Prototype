using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Parser : MonoBehaviour
{
    public TMP_Text feedbackText;
    public TMP_Text selectedSim1, selectedSim2;
    public TMP_InputField intervalInputField;
    public SimMetadata simMetadata;
    public InputAndParsedData so;
    public GameObject dp1, dp2;

    IdStandsDropdown idStandsDropdown1, IdStandsDropdown2;

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
        estadoIndex = 15, tableId_stand = 0, tableSIndex = 1, tableId_presc = 3, tableYearIndex = 6, tablenstIndex = 14, tablenIndex = 15, tablendeadIndex = 16, tablehdomIndex = 13, 
        tablegIndex = 19, tabledgIndex = 20, tablevu_stIndex = 21, tablevIndex = 24, tablevu_as1Index = 30, tablevu_as2Index = 31, tablevu_as3Index = 32, tablevu_as4Index = 33, 
        tablevu_as5Index = 34, tablemaiVIndex = 38, tableiVIndex = 39, tablewwIndex = 40, tablewbIndex = 41, tablewbrIndex = 42, tablewlIndex = 43, tablewaIndex = 44, 
        tablewrIndex = 45, tablenpvsumIndex = 66, tableeeaIndex = 67, ddId_standIndex = 0, ddId_prescIndex = 1, ddYearIndex = 2, dd0Index = 6, dd5Index = 7, dd10Index = 8,
        dd15index = 9, dd20Index = 10, dd25Index = 11, dd30Index = 12, dd35Index = 13, dd40Index = 14, dd45Index = 15, dd50Index = 16, dd55Index = 17, dd60Index = 18, dd65Index = 19,
        dd70Index = 20, dd75Index = 21, dd80Index = 22, dd85Index = 23, dd90Index = 24, dd95Index = 25, dd100Index = 26, dd102Index = 27;
    //max size 2 for now
    List<string> selectedIdStands = new List<string>();

    private void Start()
    {
        idStandsDropdown1 = dp1.GetComponent<IdStandsDropdown>();
        IdStandsDropdown2 = dp2.GetComponent<IdStandsDropdown>();
    }

    public void parse()
    {
        if (!string.IsNullOrEmpty(intervalInputField.text) && !int.TryParse(intervalInputField.text, out interval))
        {
            ShowMessage("Interval is not a number\n");
            return;
        }
        SortedDictionary<string, SortedDictionary<string, List<SortedDictionary<int, TreeData>>>> outputSoloTreesData = new SortedDictionary<string, SortedDictionary<string, List<SortedDictionary<int, TreeData>>>>();
        SortedDictionary<string, SortedDictionary<string, List<YieldTableEntry>>> outputYieldTableData = new SortedDictionary<string, SortedDictionary<string, List<YieldTableEntry>>>();
        SortedDictionary<string, SortedDictionary<string, List<DDEntry>>> outputDDTableData = new SortedDictionary<string, SortedDictionary<string, List<DDEntry>>>();
        List<(int, List<float>)> shapeData = new List<(int, List<float>)>();
         
        var dropdown1 = idStandsDropdown1.GetComponent<TMP_Dropdown>();
        string selectedIdStand1 = dropdown1.options[dropdown1.value].text;

        var siminfo1 = simMetadata.simulations[selectedSim1.text];

        var simPlotDimensions1 = siminfo1.plotDataByIdPar[selectedIdStand1];

        selectedIdStands.Add(selectedIdStand1);

        var plotData1 = (simPlotDimensions1.plotShape, simPlotDimensions1.plotShape == 0 ? new List<float> { simPlotDimensions1.area } : new List<float> { simPlotDimensions1.length1, simPlotDimensions1.length2 });

        soloTreePaths.Add(siminfo1.soloTreesPath);
        yieldTablePaths.Add(siminfo1.yieldTablePath);
        DDTablePaths.Add(siminfo1.ddTablePath);

        shapeData.Add(plotData1);

        var dropdown2 = IdStandsDropdown2.GetComponent<TMP_Dropdown>();
        if (dropdown2.options.Count() > 0)
        {
            string selectedIdStand2 = dropdown2.options[dropdown2.value].text;
            var siminfo2 = simMetadata.simulations[selectedSim2.text];

            var simPlotDimensions2 = siminfo2.plotDataByIdPar[selectedIdStand2];

            selectedIdStands.Add(selectedIdStand2);

            var plotData2 = (simPlotDimensions2.plotShape, simPlotDimensions2.plotShape == 0 ? new List<float> { simPlotDimensions2.area } : new List<float> { simPlotDimensions2.length1, simPlotDimensions2.length2 });

            soloTreePaths.Add(siminfo2.soloTreesPath);
            yieldTablePaths.Add(siminfo2.yieldTablePath);
            DDTablePaths.Add(siminfo2.ddTablePath);

            shapeData.Add(plotData2);
        }

        for (int i = 0; i < soloTreePaths.Count; i++)
        {
            parseSoloTrees(outputSoloTreesData, soloTreePaths[i], selectedIdStands[i]);
            parseYieldTable(outputYieldTableData, yieldTablePaths[i], selectedIdStands[i]);
            parseDDTable(outputDDTableData, DDTablePaths[i], selectedIdStands[i]);
        }

        so.outputSoloTreesData = outputSoloTreesData;
        so.outputYieldTable = outputYieldTableData;
        so.outputDDTable = outputDDTableData;
        so.plotShapeAndDimensions = shapeData;
        SceneManager.LoadScene(1);//GOTO VISUALIZATION SCENE
    }

    private void parseSoloTrees(SortedDictionary<string, SortedDictionary<string, List<SortedDictionary<int, TreeData>>>> output, string soloTreePath, string selectedIdStand)
    {
        if (string.IsNullOrEmpty(soloTreePath))
        {
            ShowMessage("No solo trees file selected\n");
            throw new ArgumentException("No solo trees file selected");
        }

        lines = File.ReadAllLines(soloTreePath);

        if (lines.Length == 0)
        {
            ShowMessage($"File is empty on {soloTreePath}\n");
            throw new ArgumentException("File is empty");
        }

        string[] headers = lines[0].Trim().Split(',');

        if (!VerifyHeaders(headers, expectedSoloTreesHeaders))
        {
            ShowMessage($"Incorect headers on {soloTreePath}\n");
            throw new ArgumentException("Incorrect headers");
        }

        int starting_year = int.Parse(lines[1].Trim().Split(',')[3].Trim());
        int ending_year = int.Parse(lines[lines.Length - 1].Trim().Split(',')[3].Trim());

        if (interval > (ending_year - starting_year))
        {
            ShowMessage("Interval is greater than the planing horizon\n");
            throw new ArgumentException("Interval is greater than the planing horizon");
        }

        var standPrescGroups = new Dictionary<string, Dictionary<string, List<string[]>>>();

        for (int i = 1; i < lines.Length; i++)
        {
            string[] treeInfo = lines[i].Trim().Split(',');
            string id_stand = treeInfo[idStand].Trim();
            string id_presc = treeInfo[idPresc].Trim();
            if (id_stand == selectedIdStand) {

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

                if (id_arv <= lastTreeId)
                {
                    currentYearIndex++;
                    currentYearTrees = new SortedDictionary<int, TreeData>();
                    treesInfoPerYear.Add(currentYearTrees);
                }
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

                bool wasAlive = treeWasAlive.ContainsKey(id_arv) ? treeWasAlive[id_arv] : true;

                int estado = int.Parse(treeInfo[estadoIndex].Trim());
                treeWasAlive[id_arv] = (estado == 0);

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
                    if (treeYear == y)
                    {
                        targetYear = y;
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

            foreach (var yearDict in yearGroups.Values)
            {
                treesInfoPerYear.Add(yearDict);
            }
        }

        return treesInfoPerYear;
    }

    private void parseYieldTable(SortedDictionary<string, SortedDictionary<string, List<YieldTableEntry>>> output, string yieldTablePath, string selectedIdStand)
    {
        if (string.IsNullOrEmpty(yieldTablePath))
        {
            ShowMessage($"No yield table file selected\n");
            throw new ArgumentException("No yield table file selected");
        }

        lines = File.ReadAllLines(yieldTablePath);

        if (lines.Length == 0)
        {
            ShowMessage($"File is empty on {yieldTablePath}\n");
            throw new ArgumentException("File is empty");
        }

        string[] headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();

        if (!VerifyHeaders(headers, expectedYieldTableHeaders))
        {
            ShowMessage($"Incorrect headers on {yieldTablePath}\n");
            throw new ArgumentException("Incorrect headers");
        }

        var standPrescGroups = new Dictionary<string, Dictionary<string, List<YieldTableEntry>>>();

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
                YieldTableEntry entry = new YieldTableEntry(
                            id_stand, // id_stand
                            id_presc, // id_presc
                            int.Parse(entryInfo[tableYearIndex].Trim()), // year
                            Mathf.RoundToInt(float.Parse(entryInfo[tablenstIndex].Trim(), CultureInfo.InvariantCulture)), // Nst
                            Mathf.RoundToInt(float.Parse(entryInfo[tablenIndex].Trim(), CultureInfo.InvariantCulture)), // N
                            Mathf.RoundToInt(float.Parse(entryInfo[tablendeadIndex].Trim(), CultureInfo.InvariantCulture)), // Ndead
                            Mathf.RoundToInt(float.Parse(entryInfo[tableSIndex].Trim(), CultureInfo.InvariantCulture)), // S
                            float.Parse(entryInfo[tablehdomIndex].Trim(), CultureInfo.InvariantCulture), // hdom
                            float.Parse(entryInfo[tablegIndex].Trim(), CultureInfo.InvariantCulture), // G
                            float.Parse(entryInfo[tabledgIndex].Trim(), CultureInfo.InvariantCulture), // dg 
                            float.Parse(entryInfo[tablevu_stIndex].Trim(), CultureInfo.InvariantCulture), // Vu_st
                            float.Parse(entryInfo[tablevIndex].Trim(), CultureInfo.InvariantCulture), // Vst 
                            float.Parse(entryInfo[tablevu_as1Index].Trim(), CultureInfo.InvariantCulture), // Vu_as1 
                            float.Parse(entryInfo[tablevu_as2Index].Trim(), CultureInfo.InvariantCulture), // Vu_as2 
                            float.Parse(entryInfo[tablevu_as3Index].Trim(), CultureInfo.InvariantCulture), // Vu_as3 
                            float.Parse(entryInfo[tablevu_as4Index].Trim(), CultureInfo.InvariantCulture), // Vu_as4 
                            float.Parse(entryInfo[tablevu_as5Index].Trim(), CultureInfo.InvariantCulture), // Vu_as5 
                            float.Parse(entryInfo[tablemaiVIndex].Trim(), CultureInfo.InvariantCulture), // maiV 
                            float.Parse(entryInfo[tableiVIndex].Trim(), CultureInfo.InvariantCulture), // iV 
                            float.Parse(entryInfo[tablewwIndex].Trim(), CultureInfo.InvariantCulture), // Ww 
                            float.Parse(entryInfo[tablewbIndex].Trim(), CultureInfo.InvariantCulture), // Wb 
                            float.Parse(entryInfo[tablewbrIndex].Trim(), CultureInfo.InvariantCulture), // Wbr 
                            float.Parse(entryInfo[tablewlIndex].Trim(), CultureInfo.InvariantCulture), // Wl 
                            float.Parse(entryInfo[tablewaIndex].Trim(), CultureInfo.InvariantCulture), // Wa 
                            float.Parse(entryInfo[tablewrIndex].Trim(), CultureInfo.InvariantCulture), // Wr 
                            float.Parse(entryInfo[tablenpvsumIndex].Trim(), CultureInfo.InvariantCulture), // NPVsum 
                            float.Parse(entryInfo[tableeeaIndex].Trim(), CultureInfo.InvariantCulture)  // EEA 
                        );

                if (!standPrescGroups.ContainsKey(id_stand))
                {
                    standPrescGroups[id_stand] = new Dictionary<string, List<YieldTableEntry>>();
                }
                if (!standPrescGroups[id_stand].ContainsKey(id_presc))
                {
                    standPrescGroups[id_stand][id_presc] = new List<YieldTableEntry>();
                }

                standPrescGroups[id_stand][id_presc].Add(entry);
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

    private void parseDDTable(SortedDictionary<string, SortedDictionary<string, List<DDEntry>>> output, string DDTablePath, string selectedIdStand)
    {
        if (string.IsNullOrEmpty(DDTablePath))
        {
            ShowMessage($"No diamater distribution table file selected\n");
            throw new ArgumentException("No DD table file selected");
        }

        lines = File.ReadAllLines(DDTablePath);

        if (lines.Length == 0)
        {
            ShowMessage($"File is empty on {DDTablePath}\n");
            throw new ArgumentException("File is empty");
        }

        var headers = File.ReadLines(DDTablePath).First().Trim().Split(',')
                    .Select(h => h.Trim())
                    .ToArray();

        if (!VerifyHeaders(headers, expectedDDTableHeaders))
        {
            ShowMessage($"Incorrect headers on {DDTablePath}\n");
            throw new ArgumentException("Incorrect headers");
        }

        var standPrescGroups = new Dictionary<string, Dictionary<string, List<DDEntry>>>();

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
                string id_stand = entryInfo[ddId_standIndex].Trim();
                string id_presc = entryInfo[ddId_prescIndex].Trim();
                if (id_stand == selectedIdStand)
                {
                    DDEntry entry = new DDEntry(
                        id_stand, // id_stand
                        id_presc, // id_presc
                        float.Parse(entryInfo[dd0Index].Trim(), CultureInfo.InvariantCulture), // dd0
                        float.Parse(entryInfo[dd5Index].Trim(), CultureInfo.InvariantCulture), // dd5
                        float.Parse(entryInfo[dd10Index].Trim(), CultureInfo.InvariantCulture), // dd10
                        float.Parse(entryInfo[dd15index].Trim(), CultureInfo.InvariantCulture), // dd15
                        float.Parse(entryInfo[dd20Index].Trim(), CultureInfo.InvariantCulture), // dd20
                        float.Parse(entryInfo[dd25Index].Trim(), CultureInfo.InvariantCulture), // dd25
                        float.Parse(entryInfo[dd30Index].Trim(), CultureInfo.InvariantCulture), // dd30
                        float.Parse(entryInfo[dd35Index].Trim(), CultureInfo.InvariantCulture), // dd35
                        float.Parse(entryInfo[dd40Index].Trim(), CultureInfo.InvariantCulture), // dd40
                        float.Parse(entryInfo[dd45Index].Trim(), CultureInfo.InvariantCulture), // dd45
                        float.Parse(entryInfo[dd50Index].Trim(), CultureInfo.InvariantCulture), // dd50
                        float.Parse(entryInfo[dd55Index].Trim(), CultureInfo.InvariantCulture), // dd55
                        float.Parse(entryInfo[dd60Index].Trim(), CultureInfo.InvariantCulture), // dd60
                        float.Parse(entryInfo[dd65Index].Trim(), CultureInfo.InvariantCulture), // dd65
                        float.Parse(entryInfo[dd70Index].Trim(), CultureInfo.InvariantCulture), // dd70
                        float.Parse(entryInfo[dd75Index].Trim(), CultureInfo.InvariantCulture), // dd75
                        float.Parse(entryInfo[dd80Index].Trim(), CultureInfo.InvariantCulture), // dd80
                        float.Parse(entryInfo[dd85Index].Trim(), CultureInfo.InvariantCulture), // dd85
                        float.Parse(entryInfo[dd90Index].Trim(), CultureInfo.InvariantCulture), // dd90
                        float.Parse(entryInfo[dd95Index].Trim(), CultureInfo.InvariantCulture), // dd95
                        float.Parse(entryInfo[dd100Index].Trim(), CultureInfo.InvariantCulture), // dd100
                        float.Parse(entryInfo[dd102Index].Trim(), CultureInfo.InvariantCulture)  // dd102
                        );

                    if (!standPrescGroups.ContainsKey(id_stand))
                    {
                        standPrescGroups[id_stand] = new Dictionary<string, List<DDEntry>>();
                    }
                    if (!standPrescGroups[id_stand].ContainsKey(id_presc))
                    {
                        standPrescGroups[id_stand][id_presc] = new List<DDEntry>();
                    }

                    standPrescGroups[id_stand][id_presc].Add(entry);
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

    public void ShowMessage(string msg)
    {
        if (feedbackText != null)
        {
            feedbackText.text += msg;
        }
    }

    public void receiveSoloTreePath(int index, string path)
    {
        insertPath(soloTreePaths, path, index, false);
    }

    public void receiveYieldTablePath(int index, string path)
    {
        insertPath(yieldTablePaths, path, index, true);

        if(index == 0)
        {
            idStandsDropdown1.initDropdown(getIdsInFile(path));
        }
        else
        {
            IdStandsDropdown2.initDropdown(getIdsInFile(path));
        }
    }

    public List<string> getIdsInFile(string path)
    {
        List<string> ids = new List<string>();
        var lines = File.ReadAllLines(path);
        if (lines != null && lines.Length > 1)
        {
            for (int i = 1; i < lines.Length; i++)
            {
                string[] entryInfo = lines[i].Split(',').Select(s => s.Trim()).ToArray();
                string id_stand = entryInfo[tableId_stand].Trim();
                if (!ids.Contains(id_stand))
                {
                    ids.Add(id_stand);
                }
            }
        }
        return ids;
    }

    public void removeEntryList() {
        if (soloTreePaths.Count > 1)
            soloTreePaths.RemoveAt(soloTreePaths.Count - 1);
        if (yieldTablePaths.Count > 1)
            yieldTablePaths.RemoveAt(yieldTablePaths.Count - 1);
    }

    void insertPath(List<string> list, string path, int index, bool isYieldTable)
    {
        list[index] = path;
        string text = isYieldTable ? "Yield table" : "Solo trees";
        if (feedbackText != null)
            feedbackText.text += $"{text} selected for plot {index + 1}: {Path.GetFileName(path)}\n";
    }

}
