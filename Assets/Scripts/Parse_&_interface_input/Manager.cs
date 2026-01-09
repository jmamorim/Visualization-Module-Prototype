using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Manages the overall application state, including data reception, user input handling, and UI updates.
public class Manager : MonoBehaviour
{
    public TMP_Text treeInfoText;
    public GameObject treeInfoBox, graphsBox;
    public Vector3 showingTreeInfoGraphsBoxPos;
    public float showingTreeInfoGraphsBoxWidth, showingTreeInfoGraphsBoxHeight;
    public Visualizer visualizer;
    public Canvas visulaizationCanvas;
    public GameObject Camera1, Camera2;
    public GraphGenerator graphGenerator;
    public InputAndParsedData inputAndParsedData;
    public bool isParalelCameraActive = false;
    public GameObject prescDropdown1, prescDropdown2;
    public bool canInteract = true;
    public TMP_Text sim1, sim2;
    public Slider yearSlider1;
    public Slider yearSlider2;
    public Transform multiVisSliderPos;
    public bool isExpanded = false;

    CameraBehaviour cameraBehaviour1, cameraBehaviour2;
    PrescsDropdown pDropdown1, pDropdown2;
    Camera cam1, cam2;
    SortedDictionary<string, SortedDictionary<string, List<SortedDictionary<int, TreeData>>>> outputSoloTreesData;
    SortedDictionary<string, SortedDictionary<string, List<YieldTableEntry>>> YieldTableData;
    SortedDictionary<string, SortedDictionary<string, List<DDEntry>>> DDTableData;
    int current_year1, current_year2;
    string selectedId_stand1, selectedId_stand2, selectedId_presc1, selectedId_presc2, selectedId_presc1YT, selectedId_presc2YT;
    GameObject lastSelectedTree;
    Vector3 originalGraphsBoxPos;
    float originalGraphsBoxWidth, originalGraphBoxHeight;

    private void Start()
    {
        cameraBehaviour1 = Camera1.GetComponent<CameraBehaviour>();
        cameraBehaviour2 = Camera2.GetComponent<CameraBehaviour>();
        cam1 = Camera1.GetComponent<Camera>();
        cam2 = Camera2.GetComponent<Camera>();
        pDropdown1 = prescDropdown1.GetComponent<PrescsDropdown>();
        pDropdown2 = prescDropdown2.GetComponent<PrescsDropdown>();
        originalGraphsBoxPos = graphsBox.transform.localPosition;
        originalGraphsBoxWidth = (int)graphsBox.GetComponent<RectTransform>().sizeDelta.x;
        originalGraphBoxHeight = (int)graphsBox.GetComponent<RectTransform>().sizeDelta.y;

        outputSoloTreesData = inputAndParsedData.outputSoloTreesData;
        YieldTableData = inputAndParsedData.outputYieldTable;
        DDTableData = inputAndParsedData.outputDDTable;

        sim1.text = "Simulação:" + inputAndParsedData.simIds[0];
        sim2.text = inputAndParsedData.simIds.Count > 1 ? "Simulação:" + inputAndParsedData.simIds[1] : "";

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

        if (yearSlider1 != null)
        {
            int maxYear1 = outputSoloTreesData[selectedId_stand1][selectedId_presc1].Count - 1;
            yearSlider1.minValue = 0;
            yearSlider1.maxValue = maxYear1;
            yearSlider1.value = 0;
            yearSlider1.wholeNumbers = true;
            yearSlider1.onValueChanged.AddListener(OnYearSlider1Changed);
        }

        if (yearSlider2 != null && outputSoloTreesData.Count > 1)
        {
            yearSlider1.transform.localPosition = multiVisSliderPos.localPosition;
            int maxYear2 = outputSoloTreesData[selectedId_stand2][selectedId_presc2].Count - 1;
            yearSlider2.minValue = 0;
            yearSlider2.maxValue = maxYear2;
            yearSlider2.value = 0;
            yearSlider2.wholeNumbers = true;
            yearSlider2.onValueChanged.AddListener(OnYearSlider2Changed);
        }
        
        receiveSoloTreesData(outputSoloTreesData);
        receiveYieldTableData(YieldTableData, DDTableData);
    }

    void Update()
    {
        if (!isExpanded)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                var cams = Camera.allCameras;
                foreach (Camera cam in cams)
                {
                    var behaviour = cam.GetComponent<CameraBehaviour>();
                    if (behaviour.IsMouseOverViewport())
                    {
                        behaviour.SetFreeCamera(!behaviour.IsFreeCamera());
                        break;
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                ResetSelected();
                cam1.orthographic = false;
                if (outputSoloTreesData.Count > 1)
                    cam2.orthographic = false;
                visualizer.StartLidarFlyover(1);
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                ResetSelected();
                cam1.orthographic = false;
                if (outputSoloTreesData.Count > 1)
                    cam2.orthographic = false;
                visualizer.StartOrbitalLidarFlyover(1);
            }
            if (outputSoloTreesData.Count > 1)
            {
                if (Input.GetKeyDown(KeyCode.K))
                {
                    ResetSelected();
                    cam1.orthographic = false;
                    cam2.orthographic = false;
                    visualizer.StartLidarFlyover(2);
                }
                if (Input.GetKeyDown(KeyCode.I))
                {
                    ResetSelected();
                    cam1.orthographic = false;
                    cam2.orthographic = false;
                    visualizer.StartOrbitalLidarFlyover(2);
                }
            }

            if (outputSoloTreesData != null && canInteract)
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
                if (Input.GetKeyDown(KeyCode.X))
                {
                    advancePlot1();
                    changeHightlight();
                }
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    reversePlot1();
                    changeHightlight();
                }

                if (outputSoloTreesData.Count > 1)
                {
                    if (Input.GetKeyDown(KeyCode.C))
                    {
                        advancePlot2();
                        changeHightlight();
                    }
                    if (Input.GetKeyDown(KeyCode.V))
                    {
                        reversePlot2();
                        changeHightlight();
                    }
                }


                if (Input.GetKeyDown(KeyCode.P))
                {
                    if (cameraBehaviour1.IsMouseOverViewport())
                        cameraBehaviour1.SetToTopographic();

                    if (outputSoloTreesData.Count > 1 && cameraBehaviour2.IsMouseOverViewport())
                        cameraBehaviour2.SetToTopographic();
                }
            }
        }
    }

    private void OnYearSlider1Changed(float value)
    {
        int newYear = Mathf.RoundToInt(value);
        if (newYear != current_year1)
        {
            changePlot1(newYear);
            changeHightlight();
        }
    }

    private void OnYearSlider2Changed(float value)
    {
        int newYear = Mathf.RoundToInt(value);
        if (newYear != current_year2)
        {
            changePlot2(newYear);
            changeHightlight();
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
            HideTreeInfo();
            visualizer.receiveTreeDataPlot1(outputSoloTreesData[selectedId_stand1][selectedId_presc1][current_year1], outputSoloTreesData[selectedId_stand1][selectedId_presc1][current_year1].Values.First().Year);
            graphGenerator.populateDDBarCharts(
                DDTableData,
                new string[] { selectedId_stand1, selectedId_stand2 },
                new string[] { selectedId_presc1YT, selectedId_presc2YT },
                new int[] { current_year1, current_year2 }
            );

            if (yearSlider1 != null)
            {
                yearSlider1.value = current_year1;
            }
        }
    }

    private void reversePlot1()
    {
        if (current_year1 > 0)
        {
            current_year1--;
            HideTreeInfo(); 
            visualizer.receiveTreeDataPlot1(outputSoloTreesData[selectedId_stand1][selectedId_presc1][current_year1], outputSoloTreesData[selectedId_stand1][selectedId_presc1][current_year1].Values.First().Year);
            graphGenerator.populateDDBarCharts(
                DDTableData,
                new string[] { selectedId_stand1, selectedId_stand2 },
                new string[] { selectedId_presc1YT, selectedId_presc2YT },
                new int[] { current_year1, current_year2 }
            );

            if (yearSlider1 != null)
            {
                yearSlider1.value = current_year1;
            }
        }
    }

    private void advancePlot2()
    {
        if (current_year2 < outputSoloTreesData[selectedId_stand2][selectedId_presc2].Count - 1)
        {
            current_year2++;
            HideTreeInfo(); 
            visualizer.receiveTreeDataPlot2(outputSoloTreesData[selectedId_stand2][selectedId_presc2][current_year2], outputSoloTreesData[selectedId_stand2][selectedId_presc2][current_year2].Values.First().Year);
            graphGenerator.populateDDBarCharts(
               DDTableData,
                new string[] { selectedId_stand1, selectedId_stand2 },
                new string[] { selectedId_presc1YT, selectedId_presc2YT },
                new int[] { current_year1, current_year2 }
            );

            if (yearSlider2 != null)
            {
                yearSlider2.value = current_year2;
            }
        }
    }

    private void reversePlot2()
    {
        if (current_year2 > 0)
        {
            current_year2--;
            HideTreeInfo(); 
            visualizer.receiveTreeDataPlot2(outputSoloTreesData[selectedId_stand2][selectedId_presc2][current_year2], outputSoloTreesData[selectedId_stand2][selectedId_presc2][current_year2].Values.First().Year);
            graphGenerator.populateDDBarCharts(
                DDTableData,
                new string[] { selectedId_stand1, selectedId_stand2 },
                new string[] { selectedId_presc1YT, selectedId_presc2YT },
                new int[] { current_year1, current_year2 }
            );

            if (yearSlider2 != null)
            {
                yearSlider2.value = current_year2;
            }
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
            HideTreeInfo(); 
            visualizer.receiveTreeDataPlot1(
                outputSoloTreesData[selectedId_stand1][selectedId_presc1][current_year1],
                outputSoloTreesData[selectedId_stand1][selectedId_presc1][current_year1].Values.First().Year
            );
            graphGenerator.populateDDBarCharts(
                DDTableData,
                new string[] { selectedId_stand1, selectedId_stand2 },
                new string[] { selectedId_presc1YT, selectedId_presc2YT },
                new int[] { current_year1, current_year2 }
            );
            if (yearSlider1 != null)
            {
                yearSlider1.value = current_year1;
            }
            changeHightlight();
        }
    }

    private void changePlot2(int year)
    {
        var outputPlot2 = outputSoloTreesData[selectedId_stand2][selectedId_presc2];

        current_year2 = year;

        if (current_year2 >= 0)
        {
            HideTreeInfo(); 
            visualizer.receiveTreeDataPlot2(
                outputSoloTreesData[selectedId_stand2][selectedId_presc2][current_year2],
                outputSoloTreesData[selectedId_stand2][selectedId_presc2][current_year2].Values.First().Year
            );
            graphGenerator.populateDDBarCharts(
                DDTableData,
                new string[] { selectedId_stand1, selectedId_stand2 },
                new string[] { selectedId_presc1YT, selectedId_presc2YT },
                new int[] { current_year1, current_year2 }
            );
            if (yearSlider2 != null)
            {
                yearSlider2.value = current_year2;
            }
            changeHightlight();
        }
    }

    public void receiveSoloTreesData(SortedDictionary<string, SortedDictionary<string, List<SortedDictionary<int, TreeData>>>> data)
    {
        outputSoloTreesData = data;
        current_year1 = 0;
        current_year2 = 0;

        positionViewPorts(data.Count > 1, null);

        visualizer.ConfigureTerrains();

        visualizer.receiveTreeDataPlot1(outputSoloTreesData[selectedId_stand1][selectedId_presc1][current_year1], outputSoloTreesData[selectedId_stand1][selectedId_presc1][current_year1].Values.First().Year);
        if (outputSoloTreesData.Count > 1)
            visualizer.receiveTreeDataPlot2(outputSoloTreesData[selectedId_stand2][selectedId_presc2][current_year2], outputSoloTreesData[selectedId_stand2][selectedId_presc2][current_year2].Values.First().Year);

    }

    public void positionViewPorts(bool isMulti, Camera cam = null)
    {
        if (isMulti)
        {
            Camera1.SetActive(true);
            Camera2.SetActive(true);
            cameraBehaviour1.isMultiVisualization = true;
            cameraBehaviour2.isMultiVisualization = true;
            Camera1.GetComponent<Camera>().rect = new Rect(-0.25f, 0.5f, 1, 1);
            Camera2.GetComponent<Camera>().rect = new Rect(-0.25f, -0.5f, 1, 1);
        }
        else if (cam != null)
        {
            var cameras = Camera.allCameras;
            foreach (Camera camera in cameras)
            {
                if (camera != cam)
                {
                    camera.gameObject.SetActive(false);
                }
                else
                {
                    cam.GetComponent<Camera>().rect = new Rect(0, 0, 1, 1);
                }
            }
        }
        else
        {
            Camera1.SetActive(true);
            Camera2.SetActive(false);
            cameraBehaviour1.isMultiVisualization = false;
            cameraBehaviour2.isMultiVisualization = false;
            Camera1.GetComponent<Camera>().rect = new Rect(-0.25f, 0, 1, 1);
        }
        Debug.Log("Viewports positioned. Multi Visualization: " + isMulti);
    }

    public void receiveYieldTableData(SortedDictionary<string, SortedDictionary<string, List<YieldTableEntry>>> dataYT, SortedDictionary<string, SortedDictionary<string, List<DDEntry>>> dataDD)
    {
        YieldTableData = dataYT;
        DDTableData = dataDD;
        graphGenerator.receiveData(dataYT, dataDD, current_year1, outputSoloTreesData.Count() > 1 ? current_year2 : -1, selectedId_stand1, selectedId_stand2, selectedId_presc1YT, selectedId_presc2YT);
    }

    public void updateSelectedPrescriptions(string id_presc, bool isMainPlot)
    {
        if (isMainPlot)
        {
            string[] prescIds = id_presc.Split('-');
            selectedId_presc1 = prescIds[0];
            selectedId_presc1YT = prescIds[1];
            current_year1 = 0;

            if (yearSlider1 != null)
            {
                int maxYear1 = outputSoloTreesData[selectedId_stand1][selectedId_presc1].Count - 1;
                yearSlider1.maxValue = maxYear1;
                yearSlider1.value = 0;
            }
        }
        else
        {
            string[] prescIds2 = id_presc.Split('-');
            selectedId_presc2 = prescIds2[0];
            selectedId_presc2YT = prescIds2[1];
            current_year2 = 0;

            if (yearSlider2 != null)
            {
                int maxYear2 = outputSoloTreesData[selectedId_stand2][selectedId_presc2].Count - 1;
                yearSlider2.maxValue = maxYear2;
                yearSlider2.value = 0;
            }
        }

        visualizer.receiveTreeDataPlot1(outputSoloTreesData[selectedId_stand1][selectedId_presc1][current_year1], outputSoloTreesData[selectedId_stand1][selectedId_presc1][current_year1].Values.First().Year);
        if (outputSoloTreesData.Count > 1)
            visualizer.receiveTreeDataPlot2(outputSoloTreesData[selectedId_stand2][selectedId_presc2][current_year2], outputSoloTreesData[selectedId_stand2][selectedId_presc2][current_year2].Values.First().Year);

        graphGenerator.receiveData(YieldTableData, DDTableData, current_year1, outputSoloTreesData.Count() > 1 ? current_year2 : -1, selectedId_stand1, selectedId_stand2, selectedId_presc1YT, selectedId_presc2YT);
    }

    public bool isMultiVisualizationActive()
    {
        return outputSoloTreesData.Count() > 1;
    }

    public void SelectTree(GameObject tree)
    {
        lastSelectedTree = tree;
    }

    public GameObject GetSelectedTree()
    {
        return lastSelectedTree;
    }

    public void ResetSelected()
    {
        if (lastSelectedTree != null)
        {
            lastSelectedTree.transform.Find("OutlineMesh").gameObject.SetActive(false);
            lastSelectedTree = null;
        }
    }

    public void DeselectTree()
    {
        ResetSelected();
        HideTreeInfo();

    }

    public void ShowTreeInfo(Tree t)
    {
        if (treeInfoText != null)
        {
            graphsBox.transform.localPosition = showingTreeInfoGraphsBoxPos;
            graphsBox.GetComponent<RectTransform>().sizeDelta = new Vector2(showingTreeInfoGraphsBoxWidth, showingTreeInfoGraphsBoxHeight);
            treeInfoBox.SetActive(true);
            treeInfoText.text = $"Tree information:\n" +
                                $"Cicle: {t.ciclo}\n" +
                                $"Year: {t.Year}\n" +
                                $"Age: {t.t} anos\n" +
                                $"Heigth: {t.h} m\n" +
                                $"Diameter: {t.d} cm\n" +
                                $"Heigth of crown base: {t.hbc} m\n" +
                                $"Crown width: {t.cw} cm";
        }
    }

    public void HideTreeInfo()
    {
        if (treeInfoText != null)
        {
            graphsBox.transform.localPosition = originalGraphsBoxPos;
            graphsBox.GetComponent<RectTransform>().sizeDelta = new Vector2(originalGraphsBoxWidth, originalGraphBoxHeight);
            treeInfoBox.SetActive(false);
            treeInfoText.text = "";
        }
    }
}
