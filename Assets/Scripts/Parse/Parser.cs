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

    private readonly string[] expectedSoloTreesHeaders = { "id_presc", "ciclo", "Year", "t", "id_arv", "Xarv", "Yarv", "d", "h", "cw", "estado" };
    private readonly string[] expectedYieldTableHeaders = { "year", "hdom", "Nst", "N", "Ndead", "G", "dg", "Vu_st", "Vst", "Vu_as1", "Vu_as2", "Vu_as3", "Vu_as4", "Vu_as5", "maiV", "iV", "Ww", "Wb", "Wbr", "Wl", "Wa", "Wr", "NPVsum", "EEA" };
    private string[] lines;
    private List<string> soloTreePaths = new List<string>();
    private List<string> yieldTablePaths = new List<string>();
    private int interval = 0;

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
        Debug.Log($"Parsed {outputSoloTreesData.Count} solo tree files and {outputYieldTableData.Count} yield table files.");

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
        Debug.Log($"Interval: {interval}");

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

        Debug.Log($"Starting year: {starting_year}, Ending year: {ending_year}, Number of Trees: {numberOfTrees}");

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
                Debug.Log($"index: {index} treecount: {treeCount}");
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
                        int.Parse(treeInfo[1].Trim()),  //id_presc
                        int.Parse(treeInfo[2].Trim()),  //ciclo
                        int.Parse(treeInfo[3].Trim()),  //Year
                        float.Parse(treeInfo[4].Trim(), CultureInfo.InvariantCulture), //t
                        id_arv,  //id_arv
                        float.Parse(treeInfo[7].Trim(), CultureInfo.InvariantCulture),  //Xarv
                        float.Parse(treeInfo[8].Trim(), CultureInfo.InvariantCulture),  //Yarv
                        float.Parse(treeInfo[9].Trim(), CultureInfo.InvariantCulture),  //d
                        float.Parse(treeInfo[10].Trim(), CultureInfo.InvariantCulture), //h
                        float.Parse(treeInfo[11].Trim(), CultureInfo.InvariantCulture), //cw
                        int.Parse(treeInfo[24].Trim()), //estado
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
                        int.Parse(treeInfo[1].Trim()),  // id_presc
                        int.Parse(treeInfo[2].Trim()),  // ciclo
                        int.Parse(treeInfo[3].Trim()),  // Year
                        float.Parse(treeInfo[4].Trim(), CultureInfo.InvariantCulture), // t
                        id_arv,  // id_arv
                        float.Parse(treeInfo[7].Trim(), CultureInfo.InvariantCulture),  // Xarv
                        float.Parse(treeInfo[8].Trim(), CultureInfo.InvariantCulture),  // Yarv
                        float.Parse(treeInfo[9].Trim(), CultureInfo.InvariantCulture),  // d
                        float.Parse(treeInfo[10].Trim(), CultureInfo.InvariantCulture), // h
                        float.Parse(treeInfo[11].Trim(), CultureInfo.InvariantCulture), // cw
                        int.Parse(treeInfo[24].Trim()), // estado
                        rotation, // rotation
                        false //arvore estava viva na ultima instancia
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
                    int.Parse(entryInfo[6].Trim()), // year
                    Mathf.RoundToInt(float.Parse(entryInfo[14].Trim(), CultureInfo.InvariantCulture)), // Nst
                    Mathf.RoundToInt(float.Parse(entryInfo[15].Trim(), CultureInfo.InvariantCulture)), // N
                    Mathf.RoundToInt(float.Parse(entryInfo[16].Trim(), CultureInfo.InvariantCulture)), // Ndead
                    float.Parse(entryInfo[13].Trim(), CultureInfo.InvariantCulture), // hdom
                    float.Parse(entryInfo[19].Trim(), CultureInfo.InvariantCulture), // G
                    float.Parse(entryInfo[20].Trim(), CultureInfo.InvariantCulture), // dg
                    float.Parse(entryInfo[21].Trim(), CultureInfo.InvariantCulture), // Vu_st
                    float.Parse(entryInfo[23].Trim(), CultureInfo.InvariantCulture), // Vst
                    float.Parse(entryInfo[30].Trim(), CultureInfo.InvariantCulture), // Vu_as1
                    float.Parse(entryInfo[31].Trim(), CultureInfo.InvariantCulture), // Vu_as2
                    float.Parse(entryInfo[32].Trim(), CultureInfo.InvariantCulture), // Vu_as3
                    float.Parse(entryInfo[33].Trim(), CultureInfo.InvariantCulture), // Vu_as4
                    float.Parse(entryInfo[34].Trim(), CultureInfo.InvariantCulture), // Vu_as5
                    float.Parse(entryInfo[38].Trim(), CultureInfo.InvariantCulture), // maiV
                    float.Parse(entryInfo[39].Trim(), CultureInfo.InvariantCulture), // iV
                    float.Parse(entryInfo[40].Trim(), CultureInfo.InvariantCulture), // Ww
                    float.Parse(entryInfo[41].Trim(), CultureInfo.InvariantCulture), // Wb
                    float.Parse(entryInfo[42].Trim(), CultureInfo.InvariantCulture), // Wbr
                    float.Parse(entryInfo[43].Trim(), CultureInfo.InvariantCulture), // Wl
                    float.Parse(entryInfo[44].Trim(), CultureInfo.InvariantCulture), // Wa
                    float.Parse(entryInfo[45].Trim(), CultureInfo.InvariantCulture), // Wr
                    float.Parse(entryInfo[61].Trim(), CultureInfo.InvariantCulture), // NPVsum
                    float.Parse(entryInfo[62].Trim(), CultureInfo.InvariantCulture)  // EEA
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
