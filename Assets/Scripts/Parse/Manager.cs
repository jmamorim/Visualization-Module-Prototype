using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Rendering;

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
    public GraphGenerator graphGenerator;

    CameraBahaviour cameraBehaviour1;
    CameraBahaviour cameraBehaviour2;
    Camera cam1;
    Camera cam2;
    List<List<SortedDictionary<int, TreeData>>> outputSoloTreesData;
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
                changeHightlight();

            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                reversePlot1();
                if (outputSoloTreesData.Count > 1)
                    reversePlot2();
                changeHightlight();
            }

            //change year for each plot individualy
            if (Input.GetKeyDown(KeyCode.D))
            {
                advancePlot1();
                changeHightlight();
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                reversePlot1();
                changeHightlight();
            }

            if (outputSoloTreesData.Count > 1)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    advancePlot2();
                    changeHightlight();
                }
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    reversePlot2();
                    changeHightlight();
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

    public bool isParalelCameraActiveFunc()
    {
        return isParalelCameraActive;
    }

    private void changeHightlight()
    {
        graphGenerator.changeHightlightedYearGraphs(outputSoloTreesData[0][current_year1].Values.First().Year, outputSoloTreesData.Count() > 0 ? -1 : outputSoloTreesData[1][current_year2].Values.First().Year);
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

    public void changeSimYearOnGraphClick(int serieId, int year)
    {
        if (serieId == 0)
        {
            var outputPlot1 = outputSoloTreesData[0];

            int index = outputPlot1.FindIndex(e => e.Values.First().Year == year);

            if (index >= 0 && index + 1 < outputPlot1.Count && outputPlot1[index + 1].Values.First().Year == year)
            {
                index = index + 1;
            }

            current_year1 = index;

            if (current_year1 >= 0)
            {
                treeInfoText.text = "";
                visualizer.receiveTreeDataPlot1(
                    outputSoloTreesData[0][current_year1],
                    outputSoloTreesData[0][current_year1].Values.First().Year
                );
                changeHightlight();
            }
        }
        else if (serieId == 1)
        {
            var outputPlot2 = outputSoloTreesData[1];

            int index = outputPlot2.FindIndex(e => e.Values.First().Year == year);

            if (index >= 0 && index + 1 < outputPlot2.Count && outputPlot2[index + 1].Values.First().Year == year)
            {
                index = index + 1;
            }

            current_year2 = index;

            if (current_year2 >= 0)
            {
                treeInfoText.text = "";
                visualizer.receiveTreeDataPlot2(
                    outputSoloTreesData[1][current_year2],
                    outputSoloTreesData[1][current_year2].Values.First().Year
                );
                changeHightlight();
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
        graphGenerator.receiveData(data, outputSoloTreesData[0][current_year1].Values.First().Year, outputSoloTreesData.Count() > 0 ? -1 : outputSoloTreesData[1][current_year2].Values.First().Year);
    }


    //needs to change in the future
    void showInfo()
    {
        visualizer.receiveTreeDataPlot1(outputSoloTreesData[0][current_year1], outputSoloTreesData[0][current_year1].Values.First().Year);
        if (outputSoloTreesData.Count > 1)
            visualizer.receiveTreeDataPlot2(outputSoloTreesData[1][current_year2], outputSoloTreesData[1][current_year2].Values.First().Year);
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
