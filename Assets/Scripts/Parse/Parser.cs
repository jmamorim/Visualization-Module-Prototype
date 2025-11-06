using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

//Multi visualization support needed!!!!
public class Parser : MonoBehaviour
{
    public TMP_Text feedbackText;
    public TMP_InputField intervalInputField;
    public Manager manager;

    readonly string[] expectedSoloTreesHeaders = { "id_presc", "ciclo", "Year", "t", "id_arv", "Xarv", "Yarv", "d", "h", "cw", "estado" };
    readonly string[] expectedYieldTableHeaders = { "year", "hdom", "Nst", "N", "Ndead", "G", "dg", "Vu_st", "Vst", "Vu_as1", "Vu_as2", 
        "Vu_as3", "Vu_as4", "Vu_as5", "maiV", "iV", "Ww", "Wb", "Wbr", "Wl", "Wa", "Wr", "NPVsum", "EEA" };
    string[] lines;
    List<string> soloTreePaths = new List<string>();
    List<string> yieldTablePaths = new List<string>();
    List<string> multiYieldTablePaths = new List<string>();
    int interval = 0;
    const int idIndex = 1, cicloIndex = 2, yearIndex = 3, tIndex = 4, XarvIndex = 7, YarvIndex = 8, dIndex = 9, 
        hIndex = 10, cwIndex = 11, estadoIndex = 24, tableIdIndex = 0, tableSIndex = 1, tableYearIndex = 6, nstIndex = 14, nIndex = 15, ndeadIndex = 16,
        hdomIndex = 13, gIndex = 19, dgIndex = 20, vu_stIndex = 21, vIndex = 24, vu_as1Index = 30, vu_as2Index = 31, 
        vu_as3Index = 32, vu_as4Index = 33, vu_as5Index = 34, maiVIndex = 38, iVIndex = 39, wwIndex = 40, wbIndex = 41, 
        wbrIndex = 42, wlIndex = 43, waIndex = 44, wrIndex = 45, npvsumIndex = 60, eeaIndex = 61;

    public void parse()
    {
        if (!string.IsNullOrEmpty(intervalInputField.text) && !int.TryParse(intervalInputField.text, out interval))
        {
            ShowMessage("Interval is not a number", Color.red);
            return;
        }

        List<List<SortedDictionary<int, TreeData>>> outputSoloTreesData = new List<List<SortedDictionary<int, TreeData>>>();
        List<List<YieldTableEntry>> outputYieldTableData = new List<List<YieldTableEntry>>();

        foreach (string s in soloTreePaths)
            parseSoloTrees(outputSoloTreesData, s);
        foreach (string s in yieldTablePaths)
            parseYieldTable(outputYieldTableData, s);
        foreach (string s in multiYieldTablePaths)
            receiveMultiYieldTable(outputYieldTableData, s);

        //send all info to manager
        sendDataToManager(outputSoloTreesData, outputYieldTableData);
    }

    private void parseSoloTrees(List<List<SortedDictionary<int, TreeData>>> output, string soloTreePath)
    {
        if (string.IsNullOrEmpty(soloTreePath))
        {
            ShowMessage("No file selected", Color.red);
            return;
        }

        int starting_year = 0, ending_year = 0, numberOfTrees = 0;
        lines = File.ReadAllLines(soloTreePath);

        starting_year = int.Parse(lines[1].Trim().Split(',')[3].Trim());
        ending_year = int.Parse(lines[lines.Length - 1].Trim().Split(',')[3].Trim());

        if (interval > (ending_year - starting_year))
        {
            ShowMessage("Interval is greater than the planing horizon", Color.red);
            return;
        }

        string[] headers = lines[0].Trim().Split(',');

        if (!VerifyHeaders(headers, expectedSoloTreesHeaders))
        {
            ShowMessage("Incorect headers", Color.red);
            return;
        }

        if (lines.Length == 0)
        {
            ShowMessage("File is empty", Color.red);
            return;
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
                        int.Parse(treeInfo[idIndex].Trim()),  //id_presc
                        int.Parse(treeInfo[cicloIndex].Trim()),  //ciclo
                        int.Parse(treeInfo[yearIndex].Trim()),  //Year
                        float.Parse(treeInfo[tIndex].Trim(), CultureInfo.InvariantCulture), //t
                        id_arv,  //id_arv
                        float.Parse(treeInfo[XarvIndex].Trim(), CultureInfo.InvariantCulture),  //Xarv
                        float.Parse(treeInfo[YarvIndex].Trim(), CultureInfo.InvariantCulture),  //Yarv
                        float.Parse(treeInfo[dIndex].Trim(), CultureInfo.InvariantCulture),  //d
                        float.Parse(treeInfo[hIndex].Trim(), CultureInfo.InvariantCulture), //h
                        float.Parse(treeInfo[cwIndex].Trim(), CultureInfo.InvariantCulture), //cw
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
                        int.Parse(treeInfo[idIndex].Trim()),  //id_presc
                        int.Parse(treeInfo[cicloIndex].Trim()),  //ciclo
                        int.Parse(treeInfo[yearIndex].Trim()),  //Year
                        float.Parse(treeInfo[tIndex].Trim(), CultureInfo.InvariantCulture), //t
                        id_arv,  //id_arv
                        float.Parse(treeInfo[XarvIndex].Trim(), CultureInfo.InvariantCulture),  //Xarv
                        float.Parse(treeInfo[YarvIndex].Trim(), CultureInfo.InvariantCulture),  //Yarv
                        float.Parse(treeInfo[dIndex].Trim(), CultureInfo.InvariantCulture),  //d
                        float.Parse(treeInfo[hIndex].Trim(), CultureInfo.InvariantCulture), //h
                        float.Parse(treeInfo[cwIndex].Trim(), CultureInfo.InvariantCulture), //cw
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

    private void parseYieldTable(List<List<YieldTableEntry>> output,string yieldTablePath)
    {
        if (string.IsNullOrEmpty(yieldTablePath))
        {
            ShowMessage("No file selected", Color.red);
            return;
        }

        lines = File.ReadAllLines(yieldTablePath);

        if (lines.Length == 0)
        {
            ShowMessage("File is empty", Color.red);
            return;
        }

        string[] headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();

        if (!VerifyHeaders(headers, expectedYieldTableHeaders))
        {
            ShowMessage("Incorrect headers", Color.red);
            return;
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
                    entryInfo[tableIdIndex].Trim(), // id_stand
                    int.Parse(entryInfo[tableYearIndex].Trim()), // year
                    Mathf.RoundToInt(float.Parse(entryInfo[nstIndex].Trim(), CultureInfo.InvariantCulture)), // Nst
                    Mathf.RoundToInt(float.Parse(entryInfo[nIndex].Trim(), CultureInfo.InvariantCulture)), // N
                    Mathf.RoundToInt(float.Parse(entryInfo[ndeadIndex].Trim(), CultureInfo.InvariantCulture)), // Ndead
                    Mathf.RoundToInt(float.Parse(entryInfo[tableSIndex].Trim(), CultureInfo.InvariantCulture)), //S
                    float.Parse(entryInfo[hdomIndex].Trim(), CultureInfo.InvariantCulture), // hdom
                    float.Parse(entryInfo[gIndex].Trim(), CultureInfo.InvariantCulture), // G
                    float.Parse(entryInfo[dgIndex].Trim(), CultureInfo.InvariantCulture), // dg 
                    float.Parse(entryInfo[vu_stIndex].Trim(), CultureInfo.InvariantCulture), // Vu_st
                    float.Parse(entryInfo[vIndex].Trim(), CultureInfo.InvariantCulture), // Vst 
                    float.Parse(entryInfo[vu_as1Index].Trim(), CultureInfo.InvariantCulture), // Vu_as1 
                    float.Parse(entryInfo[vu_as2Index].Trim(), CultureInfo.InvariantCulture), // Vu_as2 
                    float.Parse(entryInfo[vu_as3Index].Trim(), CultureInfo.InvariantCulture), // Vu_as3 
                    float.Parse(entryInfo[vu_as4Index].Trim(), CultureInfo.InvariantCulture), // Vu_as4 
                    float.Parse(entryInfo[vu_as5Index].Trim(), CultureInfo.InvariantCulture), // Vu_as5 
                    float.Parse(entryInfo[maiVIndex].Trim(), CultureInfo.InvariantCulture), // maiV 
                    float.Parse(entryInfo[iVIndex].Trim(), CultureInfo.InvariantCulture), // iV 
                    float.Parse(entryInfo[wwIndex].Trim(), CultureInfo.InvariantCulture), // Ww 
                    float.Parse(entryInfo[wbIndex].Trim(), CultureInfo.InvariantCulture), // Wb 
                    float.Parse(entryInfo[wbrIndex].Trim(), CultureInfo.InvariantCulture), // Wbr 
                    float.Parse(entryInfo[wlIndex].Trim(), CultureInfo.InvariantCulture), // Wl 
                    float.Parse(entryInfo[waIndex].Trim(), CultureInfo.InvariantCulture), // Wa 
                    float.Parse(entryInfo[wrIndex].Trim(), CultureInfo.InvariantCulture), // Wr 
                    float.Parse(entryInfo[npvsumIndex].Trim(), CultureInfo.InvariantCulture), // NPVsum 
                    float.Parse(entryInfo[eeaIndex].Trim(), CultureInfo.InvariantCulture)  // EEA 
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

    //refactor needed!!!
    private void receiveMultiYieldTable(List<List<YieldTableEntry>> output, string yieldTablePath)
    {
        if (string.IsNullOrEmpty(yieldTablePath))
        {
            ShowMessage("No file selected", Color.red);
            return;
        }

        var lines = File.ReadAllLines(yieldTablePath);
        if (lines.Length == 0)
        {
            ShowMessage("File is empty", Color.red);
            return;
        }

        string[] headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();
        if (!VerifyHeaders(headers, expectedYieldTableHeaders))
        {
            ShowMessage("Incorrect headers", Color.red);
            return;
        }

        output.Add(new List<YieldTableEntry>()); 
        int index = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            string[] entryInfo = lines[i].Split(',').Select(s => s.Trim()).ToArray();

            for (int j = 0; j < entryInfo.Length; j++)
            {
                string s = entryInfo[j];
                if (string.IsNullOrWhiteSpace(s)) entryInfo[j] = "0";
                else if (s.EndsWith(".")) entryInfo[j] = s + "0";
            }

            try
            {
                var entry = new YieldTableEntry(
                    entryInfo[tableIdIndex].Trim(),
                    int.Parse(entryInfo[tableYearIndex].Trim()),
                    Mathf.RoundToInt(float.Parse(entryInfo[nstIndex].Trim(), CultureInfo.InvariantCulture)),
                    Mathf.RoundToInt(float.Parse(entryInfo[nIndex].Trim(), CultureInfo.InvariantCulture)),
                    Mathf.RoundToInt(float.Parse(entryInfo[ndeadIndex].Trim(), CultureInfo.InvariantCulture)),
                    Mathf.RoundToInt(float.Parse(entryInfo[tableSIndex].Trim(), CultureInfo.InvariantCulture)),
                    float.Parse(entryInfo[hdomIndex].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(entryInfo[gIndex].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(entryInfo[dgIndex].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(entryInfo[vu_stIndex].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(entryInfo[vIndex].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(entryInfo[vu_as1Index].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(entryInfo[vu_as2Index].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(entryInfo[vu_as3Index].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(entryInfo[vu_as4Index].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(entryInfo[vu_as5Index].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(entryInfo[maiVIndex].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(entryInfo[iVIndex].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(entryInfo[wwIndex].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(entryInfo[wbIndex].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(entryInfo[wbrIndex].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(entryInfo[wlIndex].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(entryInfo[waIndex].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(entryInfo[wrIndex].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(entryInfo[npvsumIndex].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(entryInfo[eeaIndex].Trim(), CultureInfo.InvariantCulture)
                );

                if (output[index].Count > 0 && entry.S != output[index].First().S)
                {
                    index++;
                    output.Add(new List<YieldTableEntry>());
                }

                output[index].Add(entry);
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

    void ShowMessage(string msg, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = msg;
            feedbackText.color = color;
        }
    }

    public void receiveSoloTreePath(string path, string prevPath)
    {
        insertPath(soloTreePaths, path, prevPath);
    }

    public void receiveYieldTablePath(string path, string prevPath)
    {
        insertPath(yieldTablePaths, path, prevPath);
    }

    public void receiveMultiYieldTablePath(string path, string prevPath)
    {
        insertPath(multiYieldTablePaths, path, prevPath);
    }

    void insertPath(List<string> list, string path, string prevPath)
    {
        int index = list.IndexOf(prevPath);
        if (index >= 0)
        {
            list[index] = path;
        }
        else
        {
            list.Add(path);
        }
        if (feedbackText != null)
            feedbackText.text += $"File selected: {Path.GetFileName(path)}\n";
    }

    void sendDataToManager(List<List<SortedDictionary<int, TreeData>>> outputSoloTrees, List<List<YieldTableEntry>> outputYieldTable)
    {
        if (outputSoloTrees.Any() && outputYieldTable.Any())
        {
            manager.receiveSoloTreesData(outputSoloTrees);
            manager.receiveYieldTableData(outputYieldTable);
        }
    }

}
