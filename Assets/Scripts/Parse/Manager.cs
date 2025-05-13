using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public TMP_Text feedbackText;
    public TMP_Text treeInfoText;
    public Visualizer visualizer;
    public Canvas dataCanvas, visulaizationCanvas;
    public CameraBahaviour cameraBehaviour;

    private List<SortedDictionary<int, Tree>> outputData;
    private int current_year;
    private bool isVisualizationActive = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (current_year < outputData.Count - 1)
            {
                current_year++;
                if (!isVisualizationActive)
                {
                    showInfo(current_year);
                }
                else
                {
                    visualizer.receiveTreeData(outputData[current_year], outputData[current_year].Values.First().Year);
                    visualizer.displayTrees();
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
                    visualizer.receiveTreeData(outputData[current_year], outputData[current_year].Values.First().Year);
                    visualizer.displayTrees();
                }
            }

        }
        else if (Input.GetKeyDown(KeyCode.Space) && !isVisualizationActive && outputData != null)
        {
            isVisualizationActive = true;
            cameraBehaviour.EnableRotation();
            dataCanvas.gameObject.SetActive(false);
            visulaizationCanvas.gameObject.SetActive(true);
            visualizer.displayTrees();
        }
        else if (Input.GetKeyDown(KeyCode.Space) && isVisualizationActive)
        {
            isVisualizationActive = false;
            cameraBehaviour.DisableRotation();
            dataCanvas.gameObject.SetActive(true);
            visulaizationCanvas.gameObject.SetActive(false);
            showInfo(outputData[current_year].Values.First().Year);
        }
    }

    public void receiveData(List<SortedDictionary<int, Tree>> data)
    {
        outputData = data;
        current_year = 0;
        showInfo(current_year);
    }

    void showInfo(int currentyear)
    {
        string msg = $"{outputData[current_year].Values.First().Year}\n";
        SortedDictionary<int, Tree> trees = outputData[current_year];
        foreach (KeyValuePair<int, Tree> kvp in trees)
        {
            Tree t = kvp.Value;
            msg += $"id_presc: {t.id_presc}, ciclo: {t.ciclo}, Year: {t.Year}, t: {t.t}, id_arv: {t.id_arv}, Xarv: {t.Xarv}, Yarv: {t.Yarv}, d: {t.d}, h: {t.h}, cw: {t.cw}, estado: {t.estado}\n";
        }
        ShowMessage(msg, Color.white);
        visualizer.receiveTreeData(trees, trees.Values.First().Year);
    }

    void ShowMessage(string msg, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = msg;
            feedbackText.color = color;
        }
    }

    public void ShowTreeInfo(Tree t)
    {
        if (treeInfoText != null)
        {
            treeInfoText.text = $"Informação da árvore:\n" +
                                $"Id árvore: {t.id_arv}\n" +
                                $"Id Prescrição: {t.id_presc}\n" +
                                $"Ciclo: {t.ciclo}\n" +
                                $"Ano: {t.Year}\n" +
                                $"Idade: {t.t}\n" +
                                $"Diâmetro: {t.d}\n" +
                                $"Altura: {t.h}\n" +
                                $"Largura da copa: {t.cw}";
        }
    }
}
