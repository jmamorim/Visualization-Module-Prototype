using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Manages the overall application state, including data reception, user input handling, and UI updates.
public class Manager : MonoBehaviour
{
    // Public fields
    public TMP_Text treeInfoText;
    public GameObject treeInfoBox, graphsBox;
    public float showingTreeInfoGraphsBoxHeight;
    public Visualizer visualizer;
    public Canvas visulaizationCanvas;
    public GameObject Camera1, Camera2;
    public GraphGenerator graphGenerator;
    public GraphMultiSelectManager graphMultiSelectManager;
    public InputAndParsedData inputAndParsedData;
    public bool isParalelCameraActive = false;
    public GameObject prescDropdown1, prescDropdown2;
    public bool canInteract = true;
    public TMP_Text sim1, sim2;
    public Slider yearSlider1, yearSlider2;
    public Transform multiVisSliderPos;
    public bool isExpanded = false;
    public GameObject PrescBox1, PrescBox2, comparePrescsButton, changeLayoutButton;

    [SerializeField] Sprite inactiveSpriteLayoutButton;
    [SerializeField] Sprite activeSpriteLayoutButton;

    // Private fields
    CameraBehaviour cameraBehaviour1, cameraBehaviour2;
    PrescsDropdown pDropdown1, pDropdown2;
    Camera cam1, cam2;
    List<Dictionary<string, SortedDictionary<string, List<SortedDictionary<int, TreeData>>>>> outputSoloTreesData;
    List<Dictionary<string, SortedDictionary<string, List<YieldTableEntry>>>> YieldTableData;
    List<Dictionary<string, SortedDictionary<string, List<DDEntry>>>> DDTableData;
    int current_year1, current_year2;
    string selectedId_stand1, selectedId_stand2, selectedId_presc1, selectedId_presc2, selectedId_presc1YT, selectedId_presc2YT;
    GameObject lastSelectedTree;
    Vector3 originalGraphsBoxPos;
    float originalGraphsBoxWidth, originalGraphBoxHeight;
    bool isVerticalLayout = false;
    bool isComparingPresc = false;
    List<int> sortedYears;
    bool isFocusMode1 = false;
    bool isFocusMode2 = false;
    int selectedTreeCameraId = -1;
    Vector3 savedCameraPos1, savedCameraPos2;
    Quaternion savedCameraRot1, savedCameraRot2;
    Transform savedTarget1, savedTarget2;
    float focusDistance = 10.0f;

    #region Unity Methods

    private void Start()
    {
        // Camera and dropdown setup
        cameraBehaviour1 = Camera1.GetComponent<CameraBehaviour>();
        cameraBehaviour2 = Camera2.GetComponent<CameraBehaviour>();
        cam1 = Camera1.GetComponent<Camera>();
        cam2 = Camera2.GetComponent<Camera>();
        pDropdown1 = prescDropdown1.GetComponent<PrescsDropdown>();
        pDropdown2 = prescDropdown2.GetComponent<PrescsDropdown>();

        // Graphs box original state
        originalGraphsBoxPos = graphsBox.transform.localPosition;
        originalGraphsBoxWidth = (int)graphsBox.GetComponent<RectTransform>().sizeDelta.x;
        originalGraphBoxHeight = (int)graphsBox.GetComponent<RectTransform>().sizeDelta.y;

        // Data assignment
        outputSoloTreesData = inputAndParsedData.outputSoloTreesData;
        YieldTableData = inputAndParsedData.outputYieldTable;
        DDTableData = inputAndParsedData.outputDDTable;

        // Simulation labels
        sim1.text = "Simulação:" + inputAndParsedData.simIds[0];
        sim2.text = inputAndParsedData.simIds.Count > 1 ? "Simulação:" + inputAndParsedData.simIds[1] : "";

        // Dropdowns and selected IDs
        pDropdown1.initDropdown(
            outputSoloTreesData.First().First().Value.Keys.ToList(),
            YieldTableData.First().First().Value.Keys.ToList()
        );
        selectedId_stand1 = outputSoloTreesData.First().First().Value.First().Key;
        selectedId_presc1 = outputSoloTreesData.First().First().Value.First().Key;
        selectedId_presc1YT = YieldTableData.First().First().Value.First().Key;

        if (outputSoloTreesData.Count() > 1)
        {
            changeLayoutButton.SetActive(true);
            PrescBox2.SetActive(true);
            pDropdown2.initDropdown(
                outputSoloTreesData.ElementAt(1).First().Value.Keys.ToList(),
                YieldTableData.ElementAt(1).First().Value.Keys.ToList()
            );
            selectedId_stand2 = outputSoloTreesData.ElementAt(1).First().Value.First().Key;
            selectedId_presc2 = outputSoloTreesData.ElementAt(1).First().Value.First().Key;
            selectedId_presc2YT = YieldTableData.ElementAt(1).First().Value.First().Key;
        }
        else if (outputSoloTreesData.First().First().Value.First().Value.Count() > 1)
        {
            comparePrescsButton.SetActive(true);
            pDropdown2.initDropdown(
                outputSoloTreesData.First().First().Value.Keys.ToList(),
                YieldTableData.First().First().Value.Keys.ToList()
            );
            selectedId_stand2 = outputSoloTreesData.First().First().Value.First().Key;
            selectedId_presc2 = outputSoloTreesData.First().First().Value.First().Key;
            selectedId_presc2YT = YieldTableData.First().First().Value.First().Key;
        }

        // Sliders setup
        SetupYearSlider1();
        SetupYearSlider2();

        receiveSoloTreesData(outputSoloTreesData);
        receiveYieldTableData(YieldTableData, DDTableData);
    }

    void Update()
    {
        HandleKeyboardInput();
    }

    #endregion

    #region Setup Methods

    private void SetupYearSlider1()
    {
        if (yearSlider1 != null)
        {
            int maxYear1 = outputSoloTreesData.First().First().Value[selectedId_presc1].Count - 1;
            yearSlider1.minValue = 0;
            yearSlider1.maxValue = maxYear1;
            yearSlider1.value = 0;
            yearSlider1.wholeNumbers = true;
            yearSlider1.navigation = new Navigation { mode = Navigation.Mode.None };
            yearSlider1.onValueChanged.AddListener(OnYearSlider1Changed);

            EventTrigger trigger1 = yearSlider1.gameObject.GetComponent<EventTrigger>();
            if (trigger1 == null) trigger1 = yearSlider1.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerDown1 = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            pointerDown1.callback.AddListener((data) => { cameraBehaviour1.DisableCameraMovement(); cameraBehaviour2.DisableCameraMovement(); });
            trigger1.triggers.Add(pointerDown1);

            EventTrigger.Entry pointerUp1 = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            pointerUp1.callback.AddListener((data) => { cameraBehaviour1.EnableCameraMovement(); cameraBehaviour2.EnableCameraMovement(); });
            trigger1.triggers.Add(pointerUp1);
        }
    }

    private void SetupYearSlider2()
    {
        if (yearSlider2 != null && outputSoloTreesData.Count > 1)
        {
            yearSlider1.transform.SetParent(multiVisSliderPos.parent);
            yearSlider1.transform.localPosition = multiVisSliderPos.localPosition;
            int maxYear2 = outputSoloTreesData.ElementAt(1).First().Value[selectedId_presc2].Count - 1;
            yearSlider2.minValue = 0;
            yearSlider2.maxValue = maxYear2;
            yearSlider2.value = 0;
            yearSlider2.wholeNumbers = true;
            yearSlider2.navigation = new Navigation { mode = Navigation.Mode.None };
            yearSlider2.onValueChanged.AddListener(OnYearSlider2Changed);

            EventTrigger trigger2 = yearSlider2.gameObject.GetComponent<EventTrigger>();
            if (trigger2 == null) trigger2 = yearSlider2.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerDown2 = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            pointerDown2.callback.AddListener((data) => { cameraBehaviour1.DisableCameraMovement(); cameraBehaviour2.DisableCameraMovement(); });
            trigger2.triggers.Add(pointerDown2);

            EventTrigger.Entry pointerUp2 = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            pointerUp2.callback.AddListener((data) => { cameraBehaviour1.EnableCameraMovement(); cameraBehaviour2.EnableCameraMovement(); });
            trigger2.triggers.Add(pointerUp2);
        }
    }

    #endregion

    #region Input Handling

    private void HandleKeyboardInput()
    {
        HandleFocusModeToggle();
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
        }
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
                    if (Input.GetKeyDown(KeyCode.V))
                    {
                        advancePlot2();
                        changeHightlight();
                    }
                    if (Input.GetKeyDown(KeyCode.C))
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

    #endregion

    #region Public Methods

    public bool IsFocusMode() => isFocusMode1 || isFocusMode2;

    public bool IsFocusMode1() => isFocusMode1;

    public bool IsFocusMode2() => isFocusMode2;
    public void ResetSelected()
    {
        if (isFocusMode1)
        {
            ExitFocusMode(1, cameraBehaviour1);
            isFocusMode1 = false;
        }
        if (isFocusMode2)
        {
            ExitFocusMode(2, cameraBehaviour2);
            isFocusMode2 = false;
        }

        selectedTreeCameraId = -1;

        if (lastSelectedTree != null)
        {
            lastSelectedTree.transform.Find("OutlineMesh").gameObject.SetActive(false);
            lastSelectedTree = null;
        }
    }

    public void DeselectTree()
    {
        if (isFocusMode1)
        {
            ExitFocusMode(1, cameraBehaviour1);
            isFocusMode1 = false;
        }
        if (isFocusMode2)
        {
            ExitFocusMode(2, cameraBehaviour2);
            isFocusMode2 = false;
        }

        selectedTreeCameraId = -1;
        ResetSelected();
        HideTreeInfo();
    }

    public void SelectTree(GameObject tree)
    {
        lastSelectedTree = tree;
        selectedTreeCameraId = -1;

        if (IsTreeVisibleToCamera(tree, cam1))
        {
            selectedTreeCameraId = 1;
        }
        else if (cam2 != null && cam2.gameObject.activeSelf && IsTreeVisibleToCamera(tree, cam2))
        {
            selectedTreeCameraId = 2;
        }
    }
    public void comparePresc()
    {
        if (IsFocusMode())
            ExitAnyFocusMode();

        isComparingPresc = !isComparingPresc;
        PrescBox2.SetActive(isComparingPresc);
        comparePrescsButton.GetComponent<Image>().color = isComparingPresc ? Color.green : Color.red;

        pDropdown1.SetComparisonMode(isComparingPresc);
        pDropdown2.SetComparisonMode(isComparingPresc);
        if (!isComparingPresc)
            graphMultiSelectManager.DeactivateLastTwoGraphs();

        graphGenerator.populateDDBarCharts(
            DDTableData,
            new string[] { selectedId_stand1, selectedId_stand2 },
            new string[] { selectedId_presc1YT, selectedId_presc2YT },
            new int[] { current_year1, current_year1 },
            isComparingPresc
        );
        changeLayoutButton.SetActive(isComparingPresc);
        positionViewPorts(outputSoloTreesData.Count > 1, null);
    }

    public bool IsComparingPresc() => isComparingPresc;

    public bool isParalelCameraActiveFunc() => isParalelCameraActive;

    public bool isMultiVisualizationActive() => outputSoloTreesData.Count() > 1;

    public GameObject GetSelectedTree() => lastSelectedTree;
    public void ShowTreeInfo(Tree t)
    {
        if (treeInfoText != null)
        {
            graphsBox.GetComponent<RectTransform>().sizeDelta = new Vector2(graphsBox.GetComponent<RectTransform>().sizeDelta.x, showingTreeInfoGraphsBoxHeight);
            treeInfoBox.SetActive(true);
            treeInfoText.text = $"<b>Informação da árvore:</b>\n" +
                                $"Espécie: {GetSpeciesName(t.specie)}\n" +
                                $"Ciclo: {t.ciclo}\n" +
                                $"Ano: {t.Year}\n" +
                                $"Idade: {t.t} anos\n" +
                                $"Altura: {t.h} m\n" +
                                $"Diâmetro: {t.d} cm\n" +
                                $"Altura da base da copa: {t.hbc} m\n" +
                                $"Largura da copa: {t.cw} cm";
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

    public void receiveSoloTreesData(List<Dictionary<string, SortedDictionary<string, List<SortedDictionary<int, TreeData>>>>> data)
    {
        outputSoloTreesData = data;
        current_year1 = 0;
        current_year2 = 0;

        positionViewPorts(data.Count > 1, null);
        visualizer.ConfigureTerrains();

        visualizer.receiveTreeDataPlot1(
            outputSoloTreesData.First().Values.First()[selectedId_presc1][current_year1],
            outputSoloTreesData.First().Values.First()[selectedId_presc1][current_year1].Values.First().Year
        );
        if (outputSoloTreesData.Count > 1)
            visualizer.receiveTreeDataPlot2(
                outputSoloTreesData.ElementAt(1).Values.First()[selectedId_presc2][current_year2],
                outputSoloTreesData.ElementAt(1).Values.First()[selectedId_presc2][current_year2].Values.First().Year,
                true
            );
        else if (outputSoloTreesData.First().First().Value.First().Value.Count() > 1)
            visualizer.receiveTreeDataPlot2(
                outputSoloTreesData.First().Values.First()[selectedId_presc2][current_year1],
                outputSoloTreesData.First().Values.First()[selectedId_presc2][current_year1].Values.First().Year,
                false
            );
    }

    public void receiveYieldTableData(List<Dictionary<string, SortedDictionary<string, List<YieldTableEntry>>>> dataYT, List<Dictionary<string, SortedDictionary<string, List<DDEntry>>>> dataDD)
    {
        YieldTableData = dataYT;
        DDTableData = dataDD;

        List<int> allYears = new List<int>();
        foreach (var entry in dataYT.First().Values.First().First().Value)
            allYears.Add(entry.year);
        sortedYears = allYears.Distinct().OrderBy(y => y).ToList();

        graphGenerator.receiveData(
            dataYT, dataDD, current_year1, outputSoloTreesData.Count() > 1 ? current_year2 : -1,
            selectedId_stand1, selectedId_stand2, selectedId_presc1YT, selectedId_presc2YT, isComparingPresc
        );
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
                int maxYear1 = outputSoloTreesData.First().Values.First()[selectedId_presc1].Count - 1;
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

            if (isComparingPresc)
            {
                current_year1 = 0;
                if (yearSlider1 != null)
                {
                    int maxYear1 = outputSoloTreesData.First().Values.First()[selectedId_presc1].Count - 1;
                    yearSlider1.maxValue = maxYear1;
                    yearSlider1.value = 0;
                }
            }
            else if (yearSlider2 != null && outputSoloTreesData.Count > 1)
            {
                int maxYear2 = outputSoloTreesData.ElementAt(1).Values.First()[selectedId_presc2].Count - 1;
                yearSlider2.maxValue = maxYear2;
                yearSlider2.value = 0;
            }
            else
            {
                current_year1 = 0;
                int maxYear1 = outputSoloTreesData.First().Values.First()[selectedId_presc1].Count - 1;
                yearSlider1.maxValue = maxYear1;
                yearSlider1.value = 0;
            }
        }

        visualizer.receiveTreeDataPlot1(
            outputSoloTreesData.First().Values.First()[selectedId_presc1][current_year1],
            outputSoloTreesData.First().Values.First()[selectedId_presc1][current_year1].Values.First().Year
        );
        if (outputSoloTreesData.Count > 1)
            visualizer.receiveTreeDataPlot2(
                outputSoloTreesData.ElementAt(1).Values.First()[selectedId_presc2][current_year2],
                outputSoloTreesData.ElementAt(1).Values.First()[selectedId_presc2][current_year2].Values.First().Year,
                true
            );
        else if (outputSoloTreesData.First().First().Value.First().Value.Count() > 1)
            visualizer.receiveTreeDataPlot2(
                outputSoloTreesData.First().Values.First()[selectedId_presc2][current_year1],
                outputSoloTreesData.First().Values.First()[selectedId_presc2][current_year1].Values.First().Year,
                false
            );

        graphGenerator.receiveData(
            YieldTableData, DDTableData, current_year1, outputSoloTreesData.Count() > 1 ? current_year2 : -1,
            selectedId_stand1, selectedId_stand2, selectedId_presc1YT, selectedId_presc2YT, isComparingPresc
        );
    }

    public void positionViewPorts(bool isMulti, Camera cam = null)
    {
        if (isMulti || isComparingPresc)
        {
            Camera1.SetActive(true);
            Camera2.SetActive(true);
            cameraBehaviour1.isMultiVisualization = true;
            cameraBehaviour2.isMultiVisualization = true;
            if (!isVerticalLayout)
            {
                Camera1.GetComponent<Camera>().rect = new Rect(0, 0.5f, 0.75f, 0.5f);
                Camera2.GetComponent<Camera>().rect = new Rect(0, 0, 0.75f, 0.5f);
            }
            else
            {
                Camera1.GetComponent<Camera>().rect = new Rect(0, 0, 0.38f, 1);
                Camera2.GetComponent<Camera>().rect = new Rect(0.38f, 0, 0.38f, 1);
            }
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
            Camera1.GetComponent<Camera>().rect = new Rect(0, 0, 0.75f, 1);
        }
    }

    public void changeLayout()
    {
        isVerticalLayout = !isVerticalLayout;
        changeLayoutButton.GetComponent<Image>().sprite = isVerticalLayout ? activeSpriteLayoutButton : inactiveSpriteLayoutButton;
        positionViewPorts(outputSoloTreesData.Count > 1, null);
    }

    public void changeSimYearOnGraphClick(int serieId, int valueFromGraph, bool isMultiLine, bool isBar)
    {
        if (isBar)
        {
            if (isComparingPresc)
            {
                if (valueFromGraph >= 0 && valueFromGraph < sortedYears.Count)
                {
                    int yearValue = sortedYears[valueFromGraph];
                    var outputPlot1 = outputSoloTreesData.First().Values.First()[selectedId_presc1];

                    int indexInData = -1;
                    for (int i = 0; i < outputPlot1.Count; i++)
                    {
                        if (outputPlot1[i].Values.First().Year == yearValue)
                        {
                            indexInData = i;
                            break;
                        }
                    }

                    if (indexInData >= 0)
                    {
                        changePlot1(yearValue);
                    }
                }
            }
            else
            {
                changePlot1(valueFromGraph);
                if (YieldTableData.Count() > 1)
                    changePlot2(valueFromGraph);
            }
            return;
        }

        if (isComparingPresc)
        {
            var outputPlot1 = outputSoloTreesData.First().Values.First()[selectedId_presc1];
            if (valueFromGraph >= 0 && valueFromGraph < outputPlot1.Count)
            {
                int yearValue = outputPlot1[valueFromGraph].Values.First().Year;
                changePlot1(yearValue);
            }
            return;
        }

        if (!isMultiLine)
        {
            changePlot1(valueFromGraph);
            return;
        }

        if (serieId == 0 || serieId == 1)
        {
            changePlot1(valueFromGraph);
        }
        else if (serieId == 2 || serieId == 3)
        {
            changePlot2(valueFromGraph);
        }
    }

    #endregion

    #region Private Methods
    private string GetSpeciesName(string spCode)
    {
        switch (spCode)
        {
            case "Pb":
                return "Pinheiro bravo (Pb)";
            case "Pm":
                return "Pinheiro manso (Pm)";
            case "Ec":
                return "Eucalipto (Ec)";
            case "Ct":
                return "Sobreiro (Ct)";
            default:
                return spCode;
        }
    }
    private void HandleFocusModeToggle()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (lastSelectedTree != null && selectedTreeCameraId != -1)
            {
                if (selectedTreeCameraId == 1)
                {
                    isFocusMode1 = !isFocusMode1;

                    if (isFocusMode1)
                    {
                        EnterFocusMode(1, cameraBehaviour1);
                    }
                    else
                    {
                        ExitFocusMode(1, cameraBehaviour1);
                    }
                }
                else if (selectedTreeCameraId == 2)
                {
                    isFocusMode2 = !isFocusMode2;

                    if (isFocusMode2)
                    {
                        EnterFocusMode(2, cameraBehaviour2);
                    }
                    else
                    {
                        ExitFocusMode(2, cameraBehaviour2);
                    }
                }
            }
        }
    }

    private void EnterFocusMode(int cameraId, CameraBehaviour camBehaviour)
    {
        if (lastSelectedTree == null) return;

        Transform treeTransform = lastSelectedTree.transform;

        if (cameraId == 1)
        {
            savedCameraPos1 = Camera1.transform.position;
            savedCameraRot1 = Camera1.transform.rotation;
            savedTarget1 = camBehaviour.target;
        }
        else if (cameraId == 2)
        {
            savedCameraPos2 = Camera2.transform.position;
            savedCameraRot2 = Camera2.transform.rotation;
            savedTarget2 = camBehaviour.target;
        }

        Vector3 direction = (camBehaviour.transform.position - treeTransform.position).normalized;
        if (direction.magnitude < 0.1f)
        {
            direction = Vector3.forward;
        }

        Vector3 focusPosition = treeTransform.position + direction * focusDistance;
        var halfTreeHeight = lastSelectedTree.GetComponent<BoxCollider>().bounds.max.y;
        focusPosition.y = treeTransform.position.y + halfTreeHeight;

        camBehaviour.transform.position = focusPosition;

        camBehaviour.ChangeLookAt(treeTransform);

        camBehaviour.SetFocusMode(true);

        canInteract = false;
    }
    private void ExitFocusMode(int cameraId, CameraBehaviour camBehaviour)
    {
        camBehaviour.SetFocusMode(false);

        if (cameraId == 1)
        {
            Camera1.transform.position = savedCameraPos1;
            Camera1.transform.rotation = savedCameraRot1;
            camBehaviour.target = savedTarget1;
            camBehaviour.ResetLookAt();
        }
        else if (cameraId == 2)
        {
            Camera2.transform.position = savedCameraPos2;
            Camera2.transform.rotation = savedCameraRot2;
            camBehaviour.target = savedTarget2;
            camBehaviour.ResetLookAt();
        }

        canInteract = true;
    }

    private bool IsTreeVisibleToCamera(GameObject tree, Camera cam)
    {
        if ((cam.cullingMask & (1 << tree.layer)) == 0)
            return false;
        Vector3 viewportPoint = cam.WorldToViewportPoint(tree.transform.position);
        if (viewportPoint.z > 0 &&
            viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
            viewportPoint.y >= 0 && viewportPoint.y <= 1)
        {
            return true;
        }

        return false;
    }
    private void OnYearSlider2Changed(float value)
    {
        int newYear = Mathf.RoundToInt(value);
        if (newYear != current_year2)
        {
            changePlot2(newYear);
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void OnYearSlider1Changed(float value)
    {
        int newIndex = Mathf.RoundToInt(value);

        if (isComparingPresc)
        {
            var outputPlot1 = outputSoloTreesData.First().Values.First()[selectedId_presc1];
            if (newIndex >= 0 && newIndex < outputPlot1.Count)
            {
                int yearValue = outputPlot1[newIndex].Values.First().Year;
                if (yearValue != outputPlot1[current_year1].Values.First().Year)
                {
                    changePlot1(yearValue);
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                }
            }
        }
        else
        {
            if (newIndex != current_year1)
            {
                changePlot1(newIndex);
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

    private void changeHightlight()
    {
        if (isComparingPresc)
        {
            graphGenerator.changeHighlightedYearGraphs(current_year1, current_year2);
        }
        else
        {
            graphGenerator.changeHighlightedYearGraphs(current_year1, outputSoloTreesData.Count() > 1 ? current_year2 : -1);
        }
    }

    private void advancePlot2()
    {
        if (current_year2 < outputSoloTreesData.ElementAt(1).Values.First()[selectedId_presc2].Count - 1)
        {
            current_year2++;
            HideTreeInfo();
            visualizer.receiveTreeDataPlot2(
                outputSoloTreesData.ElementAt(1).Values.First()[selectedId_presc2][current_year2],
                outputSoloTreesData.ElementAt(1).Values.First()[selectedId_presc2][current_year2].Values.First().Year,
                outputSoloTreesData.Count() > 1
            );
            graphGenerator.populateDDBarCharts(
                DDTableData,
                new string[] { selectedId_stand1, selectedId_stand2 },
                new string[] { selectedId_presc1YT, selectedId_presc2YT },
                new int[] { current_year1, current_year2 },
                isComparingPresc
            );

            if (yearSlider2 != null)
            {
                yearSlider2.SetValueWithoutNotify(current_year2);
            }
        }
    }

    private void reversePlot2()
    {
        if (current_year2 > 0)
        {
            current_year2--;
            HideTreeInfo();
            visualizer.receiveTreeDataPlot2(
                outputSoloTreesData.ElementAt(1).Values.First()[selectedId_presc2][current_year2],
                outputSoloTreesData.ElementAt(1).Values.First()[selectedId_presc2][current_year2].Values.First().Year,
                outputSoloTreesData.Count() > 1
            );
            graphGenerator.populateDDBarCharts(
                DDTableData,
                new string[] { selectedId_stand1, selectedId_stand2 },
                new string[] { selectedId_presc1YT, selectedId_presc2YT },
                new int[] { current_year1, current_year1 },
                isComparingPresc
            );

            if (yearSlider2 != null)
            {
                yearSlider2.SetValueWithoutNotify(current_year2);
            }
        }
    }

    private void advancePlot1()
    {
        var outputPlot1 = outputSoloTreesData.First().Values.First()[selectedId_presc1];
        if (current_year1 < outputPlot1.Count - 1)
        {
            HideTreeInfo();
            if (isComparingPresc)
            {
                var outputPlot2 = outputSoloTreesData.First().Values.First()[selectedId_presc2];

                bool hasNextYear = outputPlot1[current_year1 + 1] != null && outputPlot2[current_year2 + 1] != null;
                bool plot1RepeatsYear = hasNextYear && outputPlot1[current_year1].Values.First().Year == outputPlot1[current_year1 + 1].Values.First().Year;
                bool plot2RepeatsYear = hasNextYear && outputPlot2[current_year2].Values.First().Year == outputPlot2[current_year2 + 1].Values.First().Year;

                if (plot1RepeatsYear)
                    current_year1++;
                if (plot2RepeatsYear)
                    current_year2++;

                if (!plot1RepeatsYear && !plot2RepeatsYear)
                {
                    current_year1++;
                    current_year2++;
                }

                Debug.Log($"Advancing Plot1 to index {current_year1} (Year: {outputPlot1[current_year1].Values.First().Year}), Plot2 to index {current_year2} (Year: {outputPlot2[current_year2].Values.First().Year})");

                visualizer.receiveTreeDataPlot1(outputPlot1[current_year1], outputPlot1[current_year1].Values.First().Year);
                visualizer.receiveTreeDataPlot2(outputPlot2[current_year2], outputPlot2[current_year2].Values.First().Year, false);
            }
            else
            {
                current_year1++;
                visualizer.receiveTreeDataPlot1(outputPlot1[current_year1], outputPlot1[current_year1].Values.First().Year);
            }
            graphGenerator.populateDDBarCharts(
                DDTableData,
                new string[] { selectedId_stand1, selectedId_stand2 },
                new string[] { selectedId_presc1YT, selectedId_presc2YT },
                new int[] { current_year1, current_year2 },
                isComparingPresc
            );
            if (yearSlider1 != null)
            {
                yearSlider1.SetValueWithoutNotify(current_year1);
            }
        }
    }

    private void reversePlot1()
    {
        if (current_year1 > 0)
        {
            var outputPlot1 = outputSoloTreesData.First().Values.First()[selectedId_presc1];
            HideTreeInfo();
            if (isComparingPresc)
            {
                var outputPlot2 = outputSoloTreesData.First().Values.First()[selectedId_presc2];

                bool hasNextYear = current_year1 > 0 && current_year2 > 0 &&
                                  outputPlot1[current_year1 - 1] != null && outputPlot2[current_year2 - 1] != null;
                bool plot1RepeatsYear = hasNextYear && outputPlot1[current_year1].Values.First().Year == outputPlot1[current_year1 - 1].Values.First().Year;
                bool plot2RepeatsYear = hasNextYear && outputPlot2[current_year2].Values.First().Year == outputPlot2[current_year2 - 1].Values.First().Year;

                if (plot1RepeatsYear)
                    current_year1--;
                if (plot2RepeatsYear)
                    current_year2--;

                if (!plot1RepeatsYear && !plot2RepeatsYear)
                {
                    current_year1--;
                    current_year2--;
                }

                Debug.Log($"Reversing Plot1 to index {current_year1} (Year: {outputPlot1[current_year1].Values.First().Year}), Plot2 to index {current_year2} (Year: {outputPlot2[current_year2].Values.First().Year})");

                visualizer.receiveTreeDataPlot1(outputPlot1[current_year1], outputPlot1[current_year1].Values.First().Year);
                visualizer.receiveTreeDataPlot2(outputPlot2[current_year2], outputPlot2[current_year2].Values.First().Year, false);
            }
            else
            {
                current_year1--;
                visualizer.receiveTreeDataPlot1(outputPlot1[current_year1], outputPlot1[current_year1].Values.First().Year);
            }
            graphGenerator.populateDDBarCharts(
                DDTableData,
                new string[] { selectedId_stand1, selectedId_stand2 },
                new string[] { selectedId_presc1YT, selectedId_presc2YT },
                new int[] { current_year1, current_year2 },
                isComparingPresc
            );
            if (yearSlider1 != null)
            {
                yearSlider1.SetValueWithoutNotify(current_year1);
            }
        }
    }

    private void changePlot1(int year)
    {
        if (IsFocusMode())
            ExitAnyFocusMode();
        var outputPlot1 = outputSoloTreesData.First().Values.First()[selectedId_presc1];
        if (isComparingPresc)
        {
            int actualIndex1 = GetLastIndexForYear(outputPlot1, year);
            if (actualIndex1 >= 0)
            {
                current_year1 = actualIndex1;
                HideTreeInfo();
                visualizer.receiveTreeDataPlot1(
                    outputPlot1[actualIndex1],
                    outputPlot1[actualIndex1].Values.First().Year
                );
                var outputPlot2 = outputSoloTreesData.First().Values.First()[selectedId_presc2];
                int actualIndex2 = GetLastIndexForYear(outputPlot2, year);
                if (actualIndex2 >= 0)
                {
                    current_year2 = actualIndex2;
                    visualizer.receiveTreeDataPlot2(
                        outputPlot2[actualIndex2],
                        outputPlot2[actualIndex2].Values.First().Year,
                        false
                    );
                }
                graphGenerator.populateDDBarCharts(
                    DDTableData,
                    new string[] { selectedId_stand1, selectedId_stand2 },
                    new string[] { selectedId_presc1YT, selectedId_presc2YT },
                    new int[] { current_year1, current_year2 },
                    isComparingPresc
                );
                if (yearSlider1 != null)
                {
                    yearSlider1.SetValueWithoutNotify(current_year1);
                }
                changeHightlight();
            }
        }
        else
        {
            current_year1 = year;
            if (current_year1 >= 0 && current_year1 < outputPlot1.Count)
            {
                HideTreeInfo();
                visualizer.receiveTreeDataPlot1(
                    outputPlot1[current_year1],
                    outputPlot1[current_year1].Values.First().Year
                );
                graphGenerator.populateDDBarCharts(
                    DDTableData,
                    new string[] { selectedId_stand1, selectedId_stand2 },
                    new string[] { selectedId_presc1YT, selectedId_presc2YT },
                    new int[] { current_year1, current_year2 },
                    isComparingPresc
                );
                if (yearSlider1 != null)
                {
                    yearSlider1.SetValueWithoutNotify(current_year1);
                }
                changeHightlight();
            }
        }
    }
    private int GetLastIndexForYear(List<SortedDictionary<int, TreeData>> prescData, int targetYear)
    {
        int lastIndex = -1;
        for (int i = 0; i < prescData.Count; i++)
        {
            int year = prescData[i].Values.First().Year;
            if (year == targetYear)
            {
                lastIndex = i;
            }
            else if (year > targetYear)
            {
                break;
            }
        }
        return lastIndex;
    }

    private void changePlot2(int year)
    {
        if (IsFocusMode())
            ExitAnyFocusMode();

        var outputPlot2 = outputSoloTreesData.ElementAt(1).Values.First()[selectedId_presc2];

        current_year2 = year;

        if (current_year2 >= 0)
        {
            HideTreeInfo();
            visualizer.receiveTreeDataPlot2(
                outputSoloTreesData.ElementAt(1).Values.First()[selectedId_presc2][current_year2],
                outputSoloTreesData.ElementAt(1).Values.First()[selectedId_presc2][current_year2].Values.First().Year,
                outputSoloTreesData.Count() > 1
            );
            graphGenerator.populateDDBarCharts(
                DDTableData,
                new string[] { selectedId_stand1, selectedId_stand2 },
                new string[] { selectedId_presc1YT, selectedId_presc2YT },
                new int[] { current_year1, current_year2 },
                isComparingPresc
            );
            if (yearSlider2 != null)
            {
                yearSlider2.SetValueWithoutNotify(current_year2);
            }
            changeHightlight();
        }
    }

    private void ExitAnyFocusMode()
    {
        if (isFocusMode1)
        {
            ExitFocusMode(1, cameraBehaviour1);
            isFocusMode1 = false;
        }

        if (isFocusMode2)
        {
            ExitFocusMode(2, cameraBehaviour2);
            isFocusMode2 = false;
        }

        selectedTreeCameraId = -1;
    }

    #endregion
}
