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
    public GameObject mainCamera;
    public GameObject paralelCamera;

    private List<SortedDictionary<int, Tree>> outputSoloTreesData;
    private List<YieldTableEntry> outputYieldTableData;
    private int current_year;
    private bool isVisualizationActive = false;
    private bool isParalelCameraActive = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) && outputSoloTreesData != null)
        {
            if (current_year < outputSoloTreesData.Count - 1)
            {
                current_year++;
                treeInfoText.text = "";
                if (!isVisualizationActive)
                {
                    showInfo(current_year);
                }
                else
                {
                    visualizer.receiveTreeData(outputSoloTreesData[current_year], outputSoloTreesData[current_year].Values.First().Year);
                    visualizer.displayTrees();
                }
            }


        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) && outputSoloTreesData != null)
        {
            if (current_year > 0)
            {
                current_year--;
                treeInfoText.text = "";
                if (!isVisualizationActive)
                {
                    showInfo(current_year);
                }
                else
                {
                    visualizer.receiveTreeData(outputSoloTreesData[current_year], outputSoloTreesData[current_year].Values.First().Year);
                    visualizer.displayTrees();
                }
            }

        }
        if (Input.GetKeyDown(KeyCode.Space) && isVisualizationActive)
        {
            isVisualizationActive = false;
            cameraBehaviour.DisableRotation();
            dataCanvas.gameObject.SetActive(true);
            visulaizationCanvas.gameObject.SetActive(false);
            showInfo(outputSoloTreesData[current_year].Values.First().Year);
        }
        else if (Input.GetKeyDown(KeyCode.Space) && !isVisualizationActive && outputSoloTreesData != null)
        {
            isVisualizationActive = true;
            cameraBehaviour.EnableRotation();
            dataCanvas.gameObject.SetActive(false);
            visulaizationCanvas.gameObject.SetActive(true);
            visualizer.displayTrees();
        }

        if (Input.GetKeyDown(KeyCode.P) && isVisualizationActive && outputSoloTreesData != null)
        {
            if (!isParalelCameraActive)
            {
                isParalelCameraActive = true;
                mainCamera.SetActive(false);
                paralelCamera.SetActive(true);
            }
            else
            {
                isParalelCameraActive = false;
                mainCamera.SetActive(true);
                paralelCamera.SetActive(false);
            }
        }
    }

    public void receiveSoloTreesData(List<SortedDictionary<int, Tree>> data)
    {
        outputSoloTreesData = data;
        current_year = 0;
        showInfo(current_year);
    }

    public void receiveYieldTableData(List<YieldTableEntry> data)
    {
        outputYieldTableData = data;
        if (outputYieldTableData != null)
        {
            foreach (var entry in outputYieldTableData)
            {
                Debug.Log(
                    $"YieldTableEntry: " +
                    $"year={entry.year}, Nst={entry.Nst}, N={entry.N}, Ndead={entry.Ndead}, " +
                    $"hdom={entry.hdom}, G={entry.G}, dg={entry.dg}, Vu_st={entry.Vu_st}, Vst={entry.Vst}, " +
                    $"Vu_as1={entry.Vu_as1}, Vu_as2={entry.Vu_as2}, Vu_as3={entry.Vu_as3}, Vu_as4={entry.Vu_as4}, Vu_as5={entry.Vu_as5}, " +
                    $"maiV={entry.maiV}, iV={entry.iV}, Ww={entry.Ww}, Wb={entry.Wb}, Wbr={entry.Wbr}, Wl={entry.Wl}, Wa={entry.Wa}, Wr={entry.Wr}, " +
                    $"NPVsum={entry.NPVsum}, EEA={entry.EEA}"
                );
            }
        }
    }

    void showInfo(int currentyear)
    {
        string msg = $"{outputSoloTreesData[current_year].Values.First().Year}\n";
        SortedDictionary<int, Tree> trees = outputSoloTreesData[current_year];
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
