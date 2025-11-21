using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Rendering;

// Manages the overall application state, including data reception, user input handling, and UI updates.
// needs to change so it can adapt visualization and data representation for yield table data and multi visualization 
public class Manager : MonoBehaviour
{
    public TMP_Text treeInfoText;
    public Visualizer visualizer;
    public Canvas visulaizationCanvas;
    public GameObject Camera1, Camera2;
    public Transform paralelPos1, paralelPos2;
    public GraphGenerator graphGenerator;
    public InputAndParsedData inputAndParsedData;
    public bool isParalelCameraActive = false;
    public GameObject prescDropdown1, prescDropdown2;

    CameraBehaviour cameraBehaviour1, cameraBehaviour2;
    PrescsDropdown pDropdown1, pDropdown2;
    Camera cam1, cam2;
    SortedDictionary<string, SortedDictionary<string, List<SortedDictionary<int, TreeData>>>> outputSoloTreesData;
    SortedDictionary<string, SortedDictionary<string, List<YieldTableEntry>>> YieldTableData;
    int current_year1, current_year2;
    bool isVisualizationActive = true;
    Vector3 lastPosCamera1Position;
    Quaternion lastPosCamera1Rotation;
    Vector3 lastPosCamera2Position;
    Quaternion lastPosCamera2Rotation;
    string selectedId_stand1, selectedId_stand2, selectedId_presc1, selectedId_presc2, selectedId_presc1YT, selectedId_presc2YT;

    private void Start()
    {
        cameraBehaviour1 = Camera1.GetComponent<CameraBehaviour>();
        cameraBehaviour2 = Camera2.GetComponent<CameraBehaviour>();
        cam1 = Camera1.GetComponent<Camera>();
        cam2 = Camera2.GetComponent<Camera>();
        pDropdown1 = prescDropdown1.GetComponent<PrescsDropdown>();
        pDropdown2 = prescDropdown2.GetComponent<PrescsDropdown>();

        outputSoloTreesData = inputAndParsedData.outputSoloTreesData;
        YieldTableData = inputAndParsedData.outputYieldTable;

        pDropdown1.initDropdown(
            outputSoloTreesData.First().Value.Keys.ToList(),
            YieldTableData.First().Value.Keys.ToList()
        );
        selectedId_stand1 = outputSoloTreesData.First().Key;
        selectedId_presc1 = outputSoloTreesData[selectedId_stand1].First().Key;
        selectedId_presc1YT = YieldTableData[selectedId_stand1].First().Key;

        if (outputSoloTreesData.Count() > 1)
        {
            prescDropdown2.SetActive(true);
            pDropdown2.initDropdown(
                outputSoloTreesData.ElementAt(1).Value.Keys.ToList(),
                YieldTableData.ElementAt(1).Value.Keys.ToList()
            );
            selectedId_stand2 = outputSoloTreesData.ElementAt(1).Key;
            selectedId_presc2 = outputSoloTreesData[selectedId_stand2].First().Key;
            selectedId_presc2YT = YieldTableData[selectedId_stand2].First().Key;
        }

        receiveSoloTreesData(outputSoloTreesData);
        receiveYieldTableData(YieldTableData);
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
                    Camera1.transform.position = paralelPos1.position;
                    Camera1.transform.rotation = paralelPos1.rotation;

                    if (outputSoloTreesData.Count > 1)
                    {
                        lastPosCamera2Position = Camera2.transform.position;
                        lastPosCamera2Rotation = Camera2.transform.rotation;
                        cameraBehaviour2.DisableRotation();
                        cam2.orthographic = true;
                        cam2.orthographicSize = 70;
                        Camera2.transform.position = paralelPos2.position;
                        Camera2.transform.rotation = paralelPos2.rotation;
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
        graphGenerator.changeHighlightedYearGraphs(current_year1, outputSoloTreesData.Count() > 1 ? current_year2 : -1);
    }

    private void advancePlot1()
    {
        if (current_year1 < outputSoloTreesData[selectedId_stand1][selectedId_presc1].Count - 1)
        {
            current_year1++;
            treeInfoText.text = "";
            visualizer.receiveTreeDataPlot1(outputSoloTreesData[selectedId_stand1][selectedId_presc1][current_year1], outputSoloTreesData[selectedId_stand1][selectedId_presc1][current_year1].Values.First().Year);
        }
    }

    private void reversePlot1()
    {
        if (current_year1 > 0)
        {
            current_year1--;
            treeInfoText.text = "";
            visualizer.receiveTreeDataPlot1(outputSoloTreesData[selectedId_stand1][selectedId_presc1][current_year1], outputSoloTreesData[selectedId_stand1][selectedId_presc1][current_year1].Values.First().Year);
        }
    }

    private void advancePlot2()
    {
        if (current_year2 < outputSoloTreesData[selectedId_stand1][selectedId_presc1].Count - 1)
        {
            current_year2++;
            treeInfoText.text = "";
            visualizer.receiveTreeDataPlot2(outputSoloTreesData[selectedId_stand2][selectedId_presc2][current_year2], outputSoloTreesData[selectedId_stand2][selectedId_presc2][current_year2].Values.First().Year);
        }
    }

    private void reversePlot2()
    {
        if (current_year2 > 0)
        {
            current_year2--;
            treeInfoText.text = "";
            visualizer.receiveTreeDataPlot2(outputSoloTreesData[selectedId_stand2][selectedId_presc2][current_year2], outputSoloTreesData[selectedId_stand2][selectedId_presc2][current_year2].Values.First().Year);
        }
    }

    public void changeSimYearOnGraphClick(int serieId, int year, bool isMultiLine, bool isBar)
    {
        if (isBar)
        {
            changePlot1(year);
            if (YieldTableData.Count() > 1)
                changePlot2(year);
            return;
        }
        if (serieId == 0)
        {
            changePlot1(year);
            return;
        }
        else if (serieId == 1)
        {
            if (isMultiLine)
                changePlot1(year);
            else
                changePlot2(year);
            return;
        }
        if (isMultiLine)
        {
            if (serieId == 2)
            {
                changePlot2(year);
            }
            else if (serieId == 3)
            {
                changePlot2(year);
            }
            return;
        }
    }

    private void changePlot1(int year)
    {
        var outputPlot1 = outputSoloTreesData[selectedId_stand1][selectedId_presc1];

        current_year1 = year;

        if (current_year1 >= 0)
        {
            treeInfoText.text = "";
            visualizer.receiveTreeDataPlot1(
                outputSoloTreesData[selectedId_stand1][selectedId_presc1][current_year1],
                outputSoloTreesData[selectedId_stand1][selectedId_presc1][current_year1].Values.First().Year
            );
            changeHightlight();
        }
    }

    private void changePlot2(int year)
    {
        var outputPlot2 = outputSoloTreesData[selectedId_stand2][selectedId_presc2];

        current_year2 = year;

        if (current_year2 >= 0)
        {
            treeInfoText.text = "";
            visualizer.receiveTreeDataPlot2(
                outputSoloTreesData[selectedId_stand2][selectedId_presc2][current_year2],
                outputSoloTreesData[selectedId_stand2][selectedId_presc2][current_year2].Values.First().Year
            );
            changeHightlight();
        }
    }

    public void receiveSoloTreesData(SortedDictionary<string, SortedDictionary<string, List<SortedDictionary<int, TreeData>>>> data)
    {
        outputSoloTreesData = data;
        current_year1 = 0;
        current_year2 = 0;

        visualizer.receiveTreeDataPlot1(outputSoloTreesData[selectedId_stand1][selectedId_presc1][current_year1], outputSoloTreesData[selectedId_stand1][selectedId_presc1][current_year1].Values.First().Year);
        if (outputSoloTreesData.Count > 1)
            visualizer.receiveTreeDataPlot2(outputSoloTreesData[selectedId_stand2][selectedId_presc2][current_year2], outputSoloTreesData[selectedId_stand2][selectedId_presc2][current_year2].Values.First().Year);

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

    public void receiveYieldTableData(SortedDictionary<string, SortedDictionary<string, List<YieldTableEntry>>> data)
    {
        YieldTableData = data;
        graphGenerator.receiveData(data, current_year1, outputSoloTreesData.Count() > 1 ? current_year2 : -1, selectedId_stand1, selectedId_stand2, selectedId_presc1YT, selectedId_presc2YT);
    }

    public void updateSelectedPrescriptions(string id_presc, bool isMainPlot)
    {
        if (isMainPlot)
        {
            string[] prescIds = id_presc.Split('-');
            selectedId_presc1 = prescIds[0];
            selectedId_presc1YT = prescIds[1];
            current_year1 = 0;
        }
        else
        {
            string[] prescIds2 = id_presc.Split('-');
            selectedId_presc2 = prescIds2[0];
            selectedId_presc2YT = prescIds2[1];
            current_year2 = 0;
        }
        
        visualizer.receiveTreeDataPlot1(outputSoloTreesData[selectedId_stand1][selectedId_presc1][current_year1], outputSoloTreesData[selectedId_stand1][selectedId_presc1][current_year1].Values.First().Year);
        if (outputSoloTreesData.Count > 1)
            visualizer.receiveTreeDataPlot2(outputSoloTreesData[selectedId_stand2][selectedId_presc2][current_year2], outputSoloTreesData[selectedId_stand2][selectedId_presc2][current_year2].Values.First().Year);
        
        graphGenerator.receiveData(YieldTableData, current_year1, outputSoloTreesData.Count() > 1 ? current_year2 : -1, selectedId_stand1, selectedId_stand2, selectedId_presc1YT, selectedId_presc2YT);
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
