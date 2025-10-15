using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

// Manages the overall application state, including data reception, user input handling, and UI updates.
// needs to change so it can adapt visualization and data representation for yield table data and multi visualization 
public class Manager : MonoBehaviour
{
    public TMP_Text feedbackText;
    public TMP_Text treeInfoText;
    public Visualizer visualizer;
    public Canvas dataCanvas, visulaizationCanvas;
    public GameObject Camera1;
    public GameObject Camera2;
    public CameraBahaviour cameraBehaviour1;
    public CameraBahaviour cameraBehaviour2;
    public GameObject mainCamera;
    public GameObject paralelCamera;

    private List<List<SortedDictionary<int, TreeData>>> outputSoloTreesData;
    private List<List<YieldTableEntry>> outputYieldTableData;
    private int current_year;
    private bool isVisualizationActive = false;
    private bool isParalelCameraActive = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) && outputSoloTreesData != null)
        {
            if (current_year < outputSoloTreesData[0].Count - 1)
            {
                current_year++;
                treeInfoText.text = "";
                if (!isVisualizationActive)
                {
                    showInfo(current_year);
                }
                else
                {
                    if(outputSoloTreesData.Count > 1)
                        visualizer.receiveTreeData(outputSoloTreesData[0][current_year], outputSoloTreesData[1][current_year], outputSoloTreesData[0][current_year].Values.First().Year);
                    else
                        visualizer.receiveTreeData(outputSoloTreesData[0][current_year], null, outputSoloTreesData[0][current_year].Values.First().Year);
                }
            }


        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) && outputSoloTreesData[0] != null)
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
                    if (outputSoloTreesData.Count > 1)
                        visualizer.receiveTreeData(outputSoloTreesData[0][current_year], outputSoloTreesData[1][current_year], outputSoloTreesData[0][current_year].Values.First().Year);
                    else
                        visualizer.receiveTreeData(outputSoloTreesData[0][current_year], null, outputSoloTreesData[0][current_year].Values.First().Year);
                }
            }

        }
        if (Input.GetKeyDown(KeyCode.Space) && isVisualizationActive)
        {
            isVisualizationActive = false;
            cameraBehaviour1.DisableRotation();
            cameraBehaviour2.DisableRotation();
            dataCanvas.gameObject.SetActive(true);
            visulaizationCanvas.gameObject.SetActive(false);
            showInfo(outputSoloTreesData[0][current_year].Values.First().Year);
        }
        else if (Input.GetKeyDown(KeyCode.Space) && !isVisualizationActive && outputSoloTreesData[0] != null)
        {
            isVisualizationActive = true;
            cameraBehaviour1.EnableRotation();
            cameraBehaviour2.EnableRotation();
            dataCanvas.gameObject.SetActive(false);
            visulaizationCanvas.gameObject.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.P) && isVisualizationActive && outputSoloTreesData[0] != null)
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

    public void receiveSoloTreesData(List<List<SortedDictionary<int, TreeData>>> data)
    {
        outputSoloTreesData = data;
        current_year = 0;
        showInfo(current_year);
        if(data.Count > 1)
        {
            Camera2.SetActive(true);
            cameraBehaviour1.isMultiVisualization = true;
            cameraBehaviour2.isMultiVisualization = true;
            Camera1.GetComponent<Camera>().rect = new Rect(0, 0, 0.5f, 1);
        }
        else
        {
            Camera2.SetActive(false);
            cameraBehaviour1.isMultiVisualization = false;
            cameraBehaviour2.isMultiVisualization = false;
            Camera1.GetComponent<Camera>().rect = new Rect(0, 0, 1, 1);
        }
    }

    public void receiveYieldTableData(List<List<YieldTableEntry>> data)
    {
        outputYieldTableData = data;
        if (outputYieldTableData != null)
        {
            foreach (var entry in outputYieldTableData[0])
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
        SortedDictionary<int, TreeData> trees = outputSoloTreesData[0][current_year];
        string msg = $"{trees.Values.First().Year}\n";
        foreach (KeyValuePair<int, TreeData> kvp in trees)
        {
            TreeData t = kvp.Value;
            msg += $"id_presc: {t.id_presc}, ciclo: {t.ciclo}, Year: {t.Year}, t: {t.t}, id_arv: {t.id_arv}, Xarv: {t.Xarv}, Yarv: {t.Yarv}, d: {t.d}, h: {t.h}, cw: {t.cw}, estado: {t.estado}\n";
        }
        ShowMessage(msg, Color.white);
        if (outputSoloTreesData.Count > 1)
            visualizer.receiveTreeData(trees, outputSoloTreesData[1][current_year], trees.Values.First().Year);
        else
            visualizer.receiveTreeData(trees, null, trees.Values.First().Year);
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
