using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.IMGUI.Controls;
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
    public Transform paralelPos;

    CameraBahaviour cameraBehaviour1;
    CameraBahaviour cameraBehaviour2;
    Camera cam1;
    Camera cam2;
    List<List<SortedDictionary<int, TreeData>>> outputSoloTreesData;
    List<List<YieldTableEntry>> outputYieldTableData;
    int current_year1;
    int current_year2;
    bool isVisualizationActive = false;
    bool isParalelCameraActive = false;
    Vector3 lastPosCamera1Position;
    Quaternion lastPosCamera1Rotation;
    Vector3 lastPosCamera2Position;
    Quaternion lastPosCamera2Rotation;

    private void Start()
    {
        cameraBehaviour1 = Camera1.GetComponent<CameraBahaviour>();
        cameraBehaviour2 = Camera2.GetComponent<CameraBahaviour>();
        cam1 = Camera1.GetComponent<Camera>();
        cam2 = Camera2.GetComponent<Camera>();
    }

    void Update()
    {
        if (outputSoloTreesData != null)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                advancePlot1();
                if (outputSoloTreesData.Count > 1)
                    advancePlot2();
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                reversePlot1();
                if (outputSoloTreesData.Count > 1)
                    reversePlot2();
            }

            //change year for each plot individualy
            if (Input.GetKeyDown(KeyCode.D))
            {
                advancePlot1();
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                reversePlot1();
            }

            if (outputSoloTreesData.Count > 1)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    advancePlot2();
                }
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    reversePlot2();
                }
            }

            if (Input.GetKeyDown(KeyCode.Space) && isVisualizationActive)
            {
                isVisualizationActive = false;
                cameraBehaviour1.DisableRotation();
                cameraBehaviour2.DisableRotation();
                dataCanvas.gameObject.SetActive(true);
                visulaizationCanvas.gameObject.SetActive(false);
                showInfo();
            }
            else if (Input.GetKeyDown(KeyCode.Space) && !isVisualizationActive)
            {
                isVisualizationActive = true;
                cameraBehaviour1.EnableRotation();
                cameraBehaviour2.EnableRotation();
                dataCanvas.gameObject.SetActive(false);
                visulaizationCanvas.gameObject.SetActive(true);
            }

            // No Update(), substitua o bloco de câmeras paralelas por:
            if (Input.GetKeyDown(KeyCode.P) && isVisualizationActive)
            {
                if (!isParalelCameraActive)
                {
                    isParalelCameraActive = true;

                    lastPosCamera1Position = Camera1.transform.position;
                    lastPosCamera1Rotation = Camera1.transform.rotation;
                    cameraBehaviour1.DisableRotation();
                    cam1.orthographic = true;
                    cam1.orthographicSize = 70;
                    Camera1.transform.position = paralelPos.position;
                    Camera1.transform.rotation = paralelPos.rotation;

                    if (outputSoloTreesData.Count > 1)
                    {
                        lastPosCamera2Position = Camera2.transform.position;
                        lastPosCamera2Rotation = Camera2.transform.rotation;
                        cameraBehaviour2.DisableRotation();
                        cam2.orthographic = true;
                        cam2.orthographicSize = 70;
                        Camera2.transform.position = paralelPos.position;
                        Camera2.transform.rotation = paralelPos.rotation;
                    }
                }
                else
                {
                    isParalelCameraActive = false;

                    cameraBehaviour1.EnableRotation();
                    cam1.orthographic = false;
                    Camera1.transform.position = lastPosCamera1Position;
                    Camera1.transform.rotation = lastPosCamera1Rotation;

                    if (outputSoloTreesData.Count > 1)
                    {
                        cameraBehaviour2.EnableRotation();
                        cam2.orthographic = false;
                        Camera2.transform.position = lastPosCamera2Position;
                        Camera2.transform.rotation = lastPosCamera2Rotation;
                    }
                }
            }
        }
    }

    private void advancePlot1()
    {
        if (current_year1 < outputSoloTreesData[0].Count - 1)
        {
            current_year1++;
            treeInfoText.text = "";
            if (!isVisualizationActive)
            {
                showInfo();
            }
            else
            {
                visualizer.receiveTreeDataPlot1(outputSoloTreesData[0][current_year1], outputSoloTreesData[0][current_year1].Values.First().Year);
            }
        }
    }

    private void reversePlot1()
    {
        if (current_year1 > 0)
        {
            current_year1--;
            treeInfoText.text = "";
            if (!isVisualizationActive)
            {
                showInfo();
            }
            else
            {
                visualizer.receiveTreeDataPlot1(outputSoloTreesData[0][current_year1], outputSoloTreesData[0][current_year1].Values.First().Year);
            }
        }
    }

    private void advancePlot2()
    {
        if (current_year2 < outputSoloTreesData[1].Count - 1)
        {
            current_year2++;
            treeInfoText.text = "";
            if (!isVisualizationActive)
            {
                showInfo();
            }
            else
            {
                visualizer.receiveTreeDataPlot2(outputSoloTreesData[1][current_year2], outputSoloTreesData[1][current_year2].Values.First().Year);
            }
        }
    }

    private void reversePlot2()
    {
        if (current_year2 > 0)
        {
            current_year2--;
            treeInfoText.text = "";
            if (!isVisualizationActive)
            {
                showInfo();
            }
            else
            {
                visualizer.receiveTreeDataPlot2(outputSoloTreesData[1][current_year2], outputSoloTreesData[1][current_year2].Values.First().Year);
            }
        }
    }

    public void receiveSoloTreesData(List<List<SortedDictionary<int, TreeData>>> data)
    {
        outputSoloTreesData = data;
        current_year1 = 0;
        current_year2 = 0;
        showInfo();

        //setup cameras viewport
        if (data.Count > 1)
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

    //needs to change in the future
    void showInfo()
    {
        SortedDictionary<int, TreeData> trees = outputSoloTreesData[0][current_year1];
        string msg = $"{trees.Values.First().Year}\n";
        foreach (KeyValuePair<int, TreeData> kvp in trees)
        {
            TreeData t = kvp.Value;
            msg += $"id_presc: {t.id_presc}, ciclo: {t.ciclo}, Year: {t.Year}, t: {t.t}, id_arv: {t.id_arv}, Xarv: {t.Xarv}, Yarv: {t.Yarv}, d: {t.d}, h: {t.h}, cw: {t.cw}, estado: {t.estado}\n";
        }
        ShowMessage(msg, Color.white);
        visualizer.receiveTreeDataPlot1(outputSoloTreesData[0][current_year1], outputSoloTreesData[0][current_year1].Values.First().Year);
        if (outputSoloTreesData.Count > 1)
            visualizer.receiveTreeDataPlot2(outputSoloTreesData[1][current_year2], outputSoloTreesData[1][current_year2].Values.First().Year);
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
