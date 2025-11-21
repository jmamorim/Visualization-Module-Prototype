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
    public TMP_InputField intervalInputField;
    public InputAndParsedData so;
    public ShapeInputController si1, si2;
    public IdStandsDropdown idStandsDropdown1, IdStandsDropdown2;

    readonly string[] expectedSoloTreesHeaders = { "id_stand", "id_presc", "ciclo", "Year", "t", "id_arv", "Xarv", "Yarv", "Species", "d", "h", "cw", " hbc", "status" };
    readonly string[] expectedYieldTableHeaders = { "year", "hdom", "Nst", "N", "Ndead", "G", "dg", "Vu_st", "Vst", "Vu_as1", "Vu_as2",
        "Vu_as3", "Vu_as4", "Vu_as5", "maiV", "iV", "Ww", "Wb", "Wbr", "Wl", "Wa", "Wr", "NPVsum", "EAA" };
    string[] lines;
    List<string> soloTreePaths = new List<string> { null };
    List<string> yieldTablePaths = new List<string> { null };
    int interval = 0;
    const int idStand = 0, idPresc = 1, cicloIndex = 2, yearIndex = 3, tIndex = 4, XarvIndex = 7, YarvIndex = 8, speciesIndex = 9, dIndex = 10, hIndex = 11, cwIndex = 12, hbcIndex = 13, 
        estadoIndex = 15, tableId_stand = 0, tableSIndex = 1, tableId_presc = 3, tableYearIndex = 6, tablenstIndex = 14, tablenIndex = 15, tablendeadIndex = 16, tablehdomIndex = 13, 
        tablegIndex = 19, tabledgIndex = 20, tablevu_stIndex = 21, tablevIndex = 24, tablevu_as1Index = 30, tablevu_as2Index = 31, tablevu_as3Index = 32, tablevu_as4Index = 33, 
        tablevu_as5Index = 34, tablemaiVIndex = 38, tableiVIndex = 39, tablewwIndex = 40, tablewbIndex = 41, tablewbrIndex = 42, tablewlIndex = 43, tablewaIndex = 44, 
        tablewrIndex = 45, tablenpvsumIndex = 66, tableeeaIndex = 67;
    //max size 2 for now
    List<string> selectedIdStands = new List<string> { null, null };

    public void parse()
    {
        if (!string.IsNullOrEmpty(intervalInputField.text) && !int.TryParse(intervalInputField.text, out interval))
        {
            ShowMessage("Interval is not a number\n");
            return;
        }
        SortedDictionary<string, SortedDictionary<string, List<SortedDictionary<int, TreeData>>>> outputSoloTreesData = new SortedDictionary<string, SortedDictionary<string, List<SortedDictionary<int, TreeData>>>>();
        SortedDictionary<string, SortedDictionary<string, List<YieldTableEntry>>> outputYieldTableData = new SortedDictionary<string, SortedDictionary<string, List<YieldTableEntry>>>();
         
        for (int i = 0; i < soloTreePaths.Count; i++)
        {
            parseSoloTrees(outputSoloTreesData, soloTreePaths[i], selectedIdStands[i]);
            parseYieldTable(outputYieldTableData, yieldTablePaths[i], selectedIdStands[i]);
        }

        so.outputSoloTreesData = outputSoloTreesData;
        so.outputYieldTable = outputYieldTableData;
        List<(int, List<float>)> shapeData = new List<(int, List<float>)>();
        var format1 = si1.GetSelectedShapeFormat();
        //check the data inputed too
        if (format1.Item1 == 0 || format1.Item2 == null || format1.Item2.Count == 0 || format1.Item2.Any(x => float.IsNaN(x)))
        {
            ShowMessage("Missing plot 1 shape data\n");
            throw new ArgumentException("Missing plot 1 shape data");
        }
        shapeData.Add(format1);
        if (outputSoloTreesData.Count > 1)
        {
            var format2 = si2.GetSelectedShapeFormat();
            //check the data inputed too
            if (format2.Item1 == 0 || format2.Item2 == null || format2.Item2.Count == 0 || format2.Item2.Any(x => float.IsNaN(x)))
            {
                ShowMessage("Missing plot 2 shape data\n");
                throw new ArgumentException("Missing plot 2 shape data");
            }
            shapeData.Add(format2);
        }
        so.plotShapeAndDimensions = shapeData;
        SceneManager.LoadScene(1);//GOTO VISUALIZATION SCENE
        Debug.Log("Parsing completed.");
    }

    private void parseSoloTrees(SortedDictionary<string, SortedDictionary<string, List<SortedDictionary<int, TreeData>>>> output, string soloTreePath, string selectedIdStand)
    {
        Debug.Log($"Parsing solo trees from: {soloTreePath}");
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

    void ShowMessage(string msg)
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

    public void addEntryList()
    {
        soloTreePaths.Add(null);
        yieldTablePaths.Add(null);
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

    public void updateSelectedIdStand(string selectedIdStand, bool isMainPlot)
    {
        selectedIdStands[isMainPlot ? 0 : 1] = selectedIdStand;
    }
}
