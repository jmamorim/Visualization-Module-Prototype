using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        SortedDictionary<int, SortedDictionary<int, Tree>> treesInfoPerYear = new SortedDictionary<int, SortedDictionary<int, Tree>>();

        if (interval == 0)
        {
            for (int i = 1; i < lines.Length; i++)
            {
                string[] treeInfo = lines[i].Trim().Split(',');

                try
                {
                    int year = int.Parse(treeInfo[3]);
                    if (!treesInfoPerYear.ContainsKey(year))
                        treesInfoPerYear[year] = new SortedDictionary<int, Tree>();

                    Tree tree = new Tree(
                        int.Parse(treeInfo[1].Trim()),
                        int.Parse(treeInfo[2].Trim()),
                        int.Parse(treeInfo[3].Trim()),
                        float.Parse(treeInfo[4].Trim(), CultureInfo.InvariantCulture),
                        int.Parse(treeInfo[6].Trim()),
                        float.Parse(treeInfo[7].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(treeInfo[8].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(treeInfo[9].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(treeInfo[10].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(treeInfo[11].Trim(), CultureInfo.InvariantCulture),
                        int.Parse(treeInfo[24].Trim())
                    );

                    treesInfoPerYear[year][tree.id_arv] = tree;
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
            int treeCount = 0;
            for (int i = 1; i < lines.Length; i++)
            {
                string[] treeInfo = lines[i].Trim().Split(',');
                try
                {
                    if (int.Parse(treeInfo[3].Trim()) == year)
                    {
                        if (!treesInfoPerYear.ContainsKey(year))
                            treesInfoPerYear[year] = new SortedDictionary<int, Tree>();

                        Tree tree = new Tree(
                            int.Parse(treeInfo[1].Trim()),
                            int.Parse(treeInfo[2].Trim()),
                            int.Parse(treeInfo[3].Trim()),
                            float.Parse(treeInfo[4].Trim(), CultureInfo.InvariantCulture),
                            int.Parse(treeInfo[6].Trim()),
                            float.Parse(treeInfo[7].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(treeInfo[8].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(treeInfo[9].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(treeInfo[10].Trim(), CultureInfo.InvariantCulture),
                            float.Parse(treeInfo[11].Trim(), CultureInfo.InvariantCulture),
                            int.Parse(treeInfo[24].Trim())
                        );
                        treesInfoPerYear[year][tree.id_arv] = tree;
                        treeCount++;
                    }
                    if (treeCount >= numberOfTrees)
                    {
                        year += interval;
                        treeCount = 0;
                        if (year > ending_year)
                            year = ending_year;
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

public class Tree
{
    public int id_presc, ciclo, Year, id_arv, estado;
    public float t, Xarv, Yarv, d, h, cw;

    public Tree(int id_presc, int ciclo, int Year, float t, int id_arv, float Xarv, float Yarv, float d, float h, float cw, int estado)
    {
        this.id_presc = id_presc;
        this.ciclo = ciclo;
        this.Year = Year;
        this.t = t;
        this.id_arv = id_arv;
        this.Xarv = Xarv;
        this.Yarv = Yarv;
        this.d = d;
        this.h = h;
        this.cw = cw;
        this.estado = estado;
    }
}
