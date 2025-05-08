using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class Parser : MonoBehaviour
{
    public TMP_Text feedbackText;
    public TMP_InputField intervalInputField;
    public Manager manager;

    private readonly string[] expectedHeaders = { "id_presc", "ciclo", "Year", "t", "id_arv", "Xarv", "Yarv", "d", "h", "cw", "estado" };
    private string[] lines;
    private string path;

    public void parse()
    {
        int interval = 0;
        if (string.IsNullOrEmpty(path))
        {
            ShowMessage("No file selected", Color.red);
            return;
        }

        if (!string.IsNullOrEmpty(intervalInputField.text) && !int.TryParse(intervalInputField.text, out interval))
        {
            ShowMessage("Interval is not a number", Color.red);
            return;
        }

        int starting_year = 0, ending_year = 0, numberOfTrees = 0;
        lines = File.ReadAllLines(path);

        starting_year = int.Parse(lines[1].Trim().Split(',')[3].Trim());
        ending_year = int.Parse(lines[lines.Length - 1].Trim().Split(',')[3].Trim());
        Debug.Log($"Interval: {interval}");

        if (interval > (ending_year - starting_year))
        {
            ShowMessage("Interval is greater than the planing horizon", Color.red);
            return;
        }

        for (int i = 1; i == int.Parse(lines[i].Trim().Split(',')[6].Trim()); i++)
        {
            numberOfTrees++;
        }

        Debug.Log($"Starting year: {starting_year}, Ending year: {ending_year}, Number of Trees: {numberOfTrees}");

        if (lines.Length == 0)
        {
            ShowMessage("File is empty", Color.red);
            return;
        }

        string[] headers = lines[0].Trim().Split(',');

        if (!VerifyHeaders(headers))
        {
            ShowMessage("Incorect headers", Color.red);
            return;
        }

        List<SortedDictionary<int, Tree>> treesInfoPerYear = new List<SortedDictionary<int, Tree>>();
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
                        treesInfoPerYear.Add(new SortedDictionary<int, Tree>());
                    }
                    Tree tree = new Tree(
                        int.Parse(treeInfo[1].Trim()),  //id_presc
                        int.Parse(treeInfo[2].Trim()),  //ciclo
                        int.Parse(treeInfo[3].Trim()),  //Year
                        float.Parse(treeInfo[4].Trim(), CultureInfo.InvariantCulture), //t
                        int.Parse(treeInfo[6].Trim()),  //id_arv
                        float.Parse(treeInfo[7].Trim(), CultureInfo.InvariantCulture),  //Xarv
                        float.Parse(treeInfo[8].Trim(), CultureInfo.InvariantCulture),  //Yarv
                        float.Parse(treeInfo[9].Trim(), CultureInfo.InvariantCulture),  //d
                        float.Parse(treeInfo[10].Trim(), CultureInfo.InvariantCulture), //h
                        float.Parse(treeInfo[11].Trim(), CultureInfo.InvariantCulture), //cw
                        int.Parse(treeInfo[24].Trim()), //estado
                        index != 0 ? treesInfoPerYear[0][int.Parse(treeInfo[6].Trim())].rotation : UnityEngine.Random.Range(0f, 360f)    //rotation
                    );

                    treesInfoPerYear[index][tree.id_arv] = tree;
                    treeCount++;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error parsing line {i}: {ex.Message}");
                }
            }

            manager.receiveData(treesInfoPerYear);
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
                        treesInfoPerYear.Add(new SortedDictionary<int, Tree>());
                    }
                    if (int.Parse(treeInfo[3].Trim()) == year)
                    {
                        Tree tree = new Tree(
                        int.Parse(treeInfo[1].Trim()),  // id_presc
                        int.Parse(treeInfo[2].Trim()),  // ciclo
                        int.Parse(treeInfo[3].Trim()),  // Year
                        float.Parse(treeInfo[4].Trim(), CultureInfo.InvariantCulture), // t
                        int.Parse(treeInfo[6].Trim()),  // id_arv
                        float.Parse(treeInfo[7].Trim(), CultureInfo.InvariantCulture),  // Xarv
                        float.Parse(treeInfo[8].Trim(), CultureInfo.InvariantCulture),  // Yarv
                        float.Parse(treeInfo[9].Trim(), CultureInfo.InvariantCulture),  // d
                        float.Parse(treeInfo[10].Trim(), CultureInfo.InvariantCulture), // h
                        float.Parse(treeInfo[11].Trim(), CultureInfo.InvariantCulture), // cw
                        int.Parse(treeInfo[24].Trim()), // estado
                        index != 0 ? treesInfoPerYear[0][int.Parse(treeInfo[6].Trim())].rotation : UnityEngine.Random.Range(0f, 360f) // rotation
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
            manager.receiveData(treesInfoPerYear);
        }

    }

    bool VerifyHeaders(string[] headers)
    {
        foreach (string h in expectedHeaders)
        {
            if (!headers.Contains(h))
            {
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

    public void receiveData(string path)
    {
        this.path = path;
    }

}
