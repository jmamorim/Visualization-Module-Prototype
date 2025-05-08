using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public TMP_Text feedbackText;
    public Visualizer visualizer;
    public Canvas dataCanvas, visulaizationCanvas;
    public CameraBahaviour cameraBehaviour;

    private SortedDictionary<int, SortedDictionary<int, Tree>> outputData;
    private int current_year;
    private bool isVisualizationActive = false;
    int[] years;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (current_year < years.Length - 1)
            {
                current_year++;
                if (!isVisualizationActive)
                {
                    showInfo(current_year);
                }
                else
                {
                    visualizer.receiveTreeData(outputData[years[current_year]]);
                    visualizer.displayTrees(years[current_year]);
                }
            }


        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (current_year > 0)
            {
                current_year--;
                if (!isVisualizationActive)
                {
                    showInfo(current_year);
                }
                else
                {
                    visualizer.receiveTreeData(outputData[years[current_year]]);
                    visualizer.displayTrees(years[current_year]);
                }
            }

        }
        else if (Input.GetKeyDown(KeyCode.Space) && !isVisualizationActive && outputData != null)
        {
            isVisualizationActive = true;
            cameraBehaviour.EnableRotation();
            dataCanvas.gameObject.SetActive(false);
            visulaizationCanvas.gameObject.SetActive(true);
            visualizer.displayTrees(years[current_year]);
        }
        else if (Input.GetKeyDown(KeyCode.Space) && isVisualizationActive)
        {
            isVisualizationActive = false;
            cameraBehaviour.DisableRotation();
            dataCanvas.gameObject.SetActive(true);
            visulaizationCanvas.gameObject.SetActive(false);
            showInfo(current_year);
        }
    }

    public void receiveData(SortedDictionary<int, SortedDictionary<int, Tree>> data)
    {
        outputData = data;
        years = outputData.Keys.ToArray();
        current_year = 0;
        showInfo(current_year);
    }

    void showInfo(int currentyear)
    {
        string msg = $"{years[currentyear]}\n";
        SortedDictionary<int, Tree> trees = outputData[years[currentyear]];
        foreach (KeyValuePair<int, Tree> kvp in trees)
        {
            Tree t = kvp.Value;
            msg += $"id_presc: {t.id_presc}, ciclo: {t.ciclo}, Year: {t.Year}, t: {t.t}, id_arv: {t.id_arv}, Xarv: {t.Xarv}, Yarv: {t.Yarv}, d: {t.d}, h: {t.h}, cw: {t.cw}, estado: {t.estado}\n";
        }
        ShowMessage(msg, Color.white);
        visualizer.receiveTreeData(trees);
    }

    void ShowMessage(string msg, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = msg;
            feedbackText.color = color;
        }
    }
}
