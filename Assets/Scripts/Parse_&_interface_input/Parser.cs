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
    public ShapeInputController si1;
    public ShapeInputController si2;

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

    public void parse()
    {
        if (!string.IsNullOrEmpty(intervalInputField.text) && !int.TryParse(intervalInputField.text, out interval))
        {
            ShowMessage("Interval is not a number\n");
            return;
        }

        List<List<SortedDictionary<int, TreeData>>> outputSoloTreesData = new List<List<SortedDictionary<int, TreeData>>>();
        List<List<YieldTableEntry>> outputYieldTableData = new List<List<YieldTableEntry>>();

        foreach (string s in soloTreePaths)
                parseSoloTrees(outputSoloTreesData, s);
        foreach (string s in yieldTablePaths)
                parseYieldTable(outputYieldTableData, s);

        so.outputSoloTreesData = outputSoloTreesData;
        so.outputYieldTable = outputYieldTableData;
        List<(int, List<float>)> shapeData = new List<(int, List<float>)>();
        var format1 = si1.GetSelectedShapeFormat();
        //check the data inputed too
        if (format1.Item1 == 0)
        {
            ShowMessage("Missing plot 1 shape data\n");
            throw new ArgumentException("Missing plot 1 shape data");
        }
        shapeData.Add(format1);
        if (outputSoloTreesData.Count > 1)
        {
            var format2 = si2.GetSelectedShapeFormat();
            //check the data inputed too
            if (format2.Item1 == 0)
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

    private void parseSoloTrees(List<List<SortedDictionary<int, TreeData>>> output, string soloTreePath)
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

        int starting_year = 0, ending_year = 0, numberOfTrees = 0;
        starting_year = int.Parse(lines[1].Trim().Split(',')[3].Trim());
        ending_year = int.Parse(lines[lines.Length - 1].Trim().Split(',')[3].Trim());

        if (interval > (ending_year - starting_year))
        {
            ShowMessage("Interval is greater than the planing horizon\n");
            throw new ArgumentException("Interval is greater than the planing horizon");
        }


        for (int i = 1; i == int.Parse(lines[i].Trim().Split(',')[6].Trim()); i++)
        {
            numberOfTrees++;
        }

        List<SortedDictionary<int, TreeData>> treesInfoPerYear = new List<SortedDictionary<int, TreeData>>();
        int index = 0;
        int treeCount = 0;

        if (interval == 0)
        {
            for (int i = 1; i < lines.Length; i++)
            {
                string[] treeInfo = lines[i].Trim().Split(',');
                if (int.Parse(treeInfo[6].Trim()) <= treeCount)
                {
                    index++;
                    treeCount = 0;
                }
                try
                {
                    if (index >= treesInfoPerYear.Count)
                    {
                        treesInfoPerYear.Add(new SortedDictionary<int, TreeData>());
                    }

                    int id_arv = int.Parse(treeInfo[6].Trim());
                    bool wasAlive = true;
                    float rotation = UnityEngine.Random.Range(0f, 360f);
                    if (index != 0 && treesInfoPerYear[index - 1].ContainsKey(id_arv))
                    {
                        rotation = treesInfoPerYear[index - 1][id_arv].rotation;
                    }

                    if (index != 0 && treesInfoPerYear[index - 1].ContainsKey(id_arv))
                    {
                        wasAlive = treesInfoPerYear[index - 1][id_arv].estado == 0;
                    }
                    TreeData tree = new TreeData(
                        treeInfo[idStand].Trim(),  //id_stand
                        treeInfo[idPresc].Trim(),  //id_presc
                        int.Parse(treeInfo[cicloIndex].Trim()),  //ciclo
                        int.Parse(treeInfo[yearIndex].Trim()),  //Year
                        float.Parse(treeInfo[tIndex].Trim(), CultureInfo.InvariantCulture), //t
                        id_arv,  //id_arv
                        float.Parse(treeInfo[XarvIndex].Trim(), CultureInfo.InvariantCulture),  //Xarv
                        float.Parse(treeInfo[YarvIndex].Trim(), CultureInfo.InvariantCulture),  //Yarv
                        treeInfo[speciesIndex].Trim(), //specie
                        float.Parse(treeInfo[dIndex].Trim(), CultureInfo.InvariantCulture),  //d
                        float.Parse(treeInfo[hIndex].Trim(), CultureInfo.InvariantCulture), //h
                        float.Parse(treeInfo[cwIndex].Trim(), CultureInfo.InvariantCulture), //cw
                        float.Parse(treeInfo[hbcIndex].Trim(), CultureInfo.InvariantCulture), //hbc
                        int.Parse(treeInfo[estadoIndex].Trim()), //estado
                        rotation,    //rotation
                        wasAlive   //arvore estava viva na ultima instancia
                    );
                    treesInfoPerYear[index][tree.id_arv] = tree;
                    treeCount++;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error parsing line {i}: {ex.Message}");
                }
            }

            output.Add(treesInfoPerYear);
        }
        else
        {
            int year = starting_year;
            for (int i = 1; i < lines.Length; i++)
            {
                string[] treeInfo = lines[i].Trim().Split(',');
                if (int.Parse(treeInfo[6].Trim()) < treeCount)
                {
                    index++;
                    year += interval;
                    treeCount = 0;
                    if (year > ending_year)
                        year = ending_year;
                }
                try
                {
                    if (index >= treesInfoPerYear.Count)
                    {
                        treesInfoPerYear.Add(new SortedDictionary<int, TreeData>());
                    }
                    if (int.Parse(treeInfo[3].Trim()) == year)
                    {
                        int id_arv = int.Parse(treeInfo[6].Trim());
                        float rotation = UnityEngine.Random.Range(0f, 360f);
                        if (index != 0 && treesInfoPerYear[index - 1].ContainsKey(id_arv))
                        {
                            rotation = treesInfoPerYear[index - 1][id_arv].rotation;
                        }
                        TreeData tree = new TreeData(
                        treeInfo[idStand].Trim(),  //id_stand
                        treeInfo[idPresc].Trim(),  //id_presc
                        int.Parse(treeInfo[cicloIndex].Trim()),  //ciclo
                        int.Parse(treeInfo[yearIndex].Trim()),  //Year
                        float.Parse(treeInfo[tIndex].Trim(), CultureInfo.InvariantCulture), //t
                        id_arv,  //id_arv
                        float.Parse(treeInfo[XarvIndex].Trim(), CultureInfo.InvariantCulture),  //Xarv
                        float.Parse(treeInfo[YarvIndex].Trim(), CultureInfo.InvariantCulture),  //Yarv
                        treeInfo[speciesIndex].Trim(), //specie
                        float.Parse(treeInfo[dIndex].Trim(), CultureInfo.InvariantCulture),  //d
                        float.Parse(treeInfo[hIndex].Trim(), CultureInfo.InvariantCulture), //h
                        float.Parse(treeInfo[cwIndex].Trim(), CultureInfo.InvariantCulture), //cw
                        float.Parse(treeInfo[hbcIndex].Trim(), CultureInfo.InvariantCulture), //hbc
                        int.Parse(treeInfo[estadoIndex].Trim()), //estado
                        rotation,    //rotation
                        false   //arvore estava viva na ultima instancia
                    );

                        treesInfoPerYear[index][tree.id_arv] = tree;
                        treeCount++;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error parsing line {i}: {ex.Message}");
                }
            }
            output.Add(treesInfoPerYear);
        }
    }

    private void parseYieldTable(List<List<YieldTableEntry>> output, string yieldTablePath)
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

        List<YieldTableEntry> yieldTable = new List<YieldTableEntry>();

        for (int i = 1; i < lines.Length; i++)
        {
            string[] entryInfo = lines[i].Split(',').Select(s => s.Trim()).ToArray();

            //fixes weird inaccuracies in the csv file like ending with a dot or empty fields
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
                YieldTableEntry entry = new YieldTableEntry(
                    entryInfo[tableId_stand].Trim(), // id_stand
                    entryInfo[tableId_presc].Trim(), // id_presc
                    int.Parse(entryInfo[tableYearIndex].Trim()), // year
                    Mathf.RoundToInt(float.Parse(entryInfo[tablenstIndex].Trim(), CultureInfo.InvariantCulture)), // Nst
                    Mathf.RoundToInt(float.Parse(entryInfo[tablenIndex].Trim(), CultureInfo.InvariantCulture)), // N
                    Mathf.RoundToInt(float.Parse(entryInfo[tablendeadIndex].Trim(), CultureInfo.InvariantCulture)), // Ndead
                    Mathf.RoundToInt(float.Parse(entryInfo[tableSIndex].Trim(), CultureInfo.InvariantCulture)), //S
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
                yieldTable.Add(entry);
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
        output.Add(yieldTable);
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
}
