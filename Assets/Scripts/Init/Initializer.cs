using System.IO;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class Initializer : MonoBehaviour
{
    public FeedbackController FeedbackController;
    public SimMetadata simMetadata;
    public GameObject buttonPrefab;
    public Transform[] buttonParent;
    public float buttonSpacing = 60f;
    public GameObject[] selectedSims;
    public GameObject[] dropdowns;
    public GameObject[] reloadButtons;
    public Parser[] parser;

    readonly List<string> species = new List<string> { "Pb", "Pm", "Ec", "Ct" };

    string[] inputHeaders = new string[] {
        "id_par", "AreaUG", "id_presc", "tlag", "CoordX", "CoordY", "id_meteo", "Altitude",
        "year", "month", "composition", "PlotType", "Sp1", "Sp2", "Structure", "S", "rot",
        "t", "tst", "tsd", "Narvp", "Aplot", "Plot_shape", "lenght1", "lenght2",
        "CoordX1", "CoordY1", "CoordX2", "CoordY2", "CoordX3", "CoordY3", "CoordX4", "CoordY4"
    };

    string[] soloTreesHeaders = {
        "id_stand", "id_presc", "ciclo", "Year", "t", "tarv", "id_arv", "Xarv", "Yarv",
        "Species", "d", "h", "cw", "hbc", "hmaxcw", "status"
    };

    string[] yieldTableHeaders = {
        "id_stand", "S", "AreaUG", "id_presc", "FMA", "opt", "year", "ttotal", "t", "rot",
        "FinalCut", "Thinning", "Debark", "hdom", "Nst", "N", "Ndead", "Nthin", "Ning",
        "Fw", "G", "dg", "Vu_st", "Vb_st", "Vst", "V", "V_as1", "V_as2", "V_as3", "V_as4",
        "V_as5", "Vu_as1", "Vu_as2", "Vu_as3", "Vu_as4", "Vu_as5", "Vharv", "Vuharv",
        "Vtot", "maiV", "iV", "Ww", "Wb", "Wbr", "Wl", "Wa", "Wr", "Wtot", "maiW", "iW",
        "WPmnuts", "Wtop_res", "Wb_res", "Wbr_res", "Wl_res", "Wres", "C_seq_prod",
        "PC_tot", "PC_in", "PC_out", "PC_lab", "R_wood", "R_biom", "R_Pmnuts", "R_resin",
        "NPV", "NPVsum", "EAA"
    };

    string[] ddTableHeaders = {
        "id_stand","id_presc","year","Thinning","Debark","t","0","5","10","15","20","25",
        "30","35","40","45","50","55","60","65","70","75","80","85","90","95","100",">102.5"
    };

    void Awake()
    {
        simMetadata.Load();
        Dictionary<string, SimulationInfo> simulations = simMetadata.simulations;
        string simsRoot = Path.Combine(Application.streamingAssetsPath, "Simulations");

        if (!Directory.Exists(simsRoot))
        {
            Debug.LogError("Simulations folder missing in build: " + simsRoot);
            return;
        }

        string[] simFolders = Directory.GetDirectories(simsRoot);
        HashSet<string> existingFolders = new HashSet<string>(
            simFolders.Select(f => Path.GetFileName(f))
        );

        var toRemove = simulations.Keys.Where(k => !existingFolders.Contains(k)).ToList();
        foreach (var key in toRemove)
        {
            simulations.Remove(key);
            Debug.Log("Removed missing simulation: " + key);
        }

        foreach (string folder in simFolders)
        {
            string folderName = Path.GetFileName(folder);

            if (simulations.ContainsKey(folderName))
            {
                Debug.Log("Simulation already registered: " + folderName);
                FeedbackController.ShowMessage("Simulation already saved: " + folderName + "\n");
                continue;
            }

            var simInfo = ProcessSimulationFolder(folder, folderName);
            if (simInfo != null)
            {
                simulations.Add(folderName, simInfo);
                Debug.Log("Registered new simulation: " + folderName);
                FeedbackController.ShowMessage("New simulation saved: " + folderName + "\n");
            }
        }

        simMetadata.simulations = simulations;
        CreateSimulationButtons(simulations);
        simMetadata.Save();
    }

    public void ReloadSimulation(string simName)
    {
        string simsRoot = Path.Combine(Application.streamingAssetsPath, "Simulations");
        string folderPath = Path.Combine(simsRoot, simName);

        if (!Directory.Exists(folderPath))
        {
            FeedbackController.ShowMessage("Error: Simulation folder not found: " + simName + "\n");
            Debug.LogError("Simulation folder not found: " + folderPath);
            return;
        }

        if (simMetadata.simulations.ContainsKey(simName))
        {
            simMetadata.simulations.Remove(simName);
            FeedbackController.ShowMessage("Removed old data for: " + simName + "\n");
        }

        var simInfo = ProcessSimulationFolder(folderPath, simName);

        if (simInfo != null)
        {
            simMetadata.simulations.Add(simName, simInfo);
            simMetadata.Save();

            // Destroy all buttons and recreate them to maintain proper positioning
            DestroyAllButtons();
            CreateSimulationButtons(simMetadata.simulations);

            FeedbackController.ShowMessage("Successfully reloaded simulation: " + simName + "\n");
            Debug.Log("Reloaded simulation: " + simName);
            resetSelectedSims();
        }
        else
        {
            FeedbackController.ShowMessage("Failed to reload simulation and removed: " + simName + "\n");
            Debug.LogError("Failed to reload simulation: " + simName);
        }
    }

    void DestroyAllButtons()
    {
        foreach (var parent in buttonParent)
        {
            foreach (Transform child in parent)
            {
                Destroy(child.gameObject);
            }
        }
    }

    SimulationInfo ProcessSimulationFolder(string folder, string folderName)
    {
        string[] csvs = Directory.GetFiles(folder, "*.csv");
        if (csvs.Length == 0)
        {
            FeedbackController.ShowMessage("No CSV files found in: " + folderName + "\n");
            return null;
        }

        string currentInputPath = "", currentSoloTreesPath = "", currentYieldTablePath = "", currentDDPath = "";

        foreach (var file in csvs)
        {
            var headers = File.ReadLines(file).First().Trim().Split(',')
                .Select(h => h.Trim())
                .ToArray();

            if (headers.SequenceEqual(inputHeaders))
            {
                if (!String.IsNullOrEmpty(currentInputPath))
                {
                    FeedbackController.ShowMessage("Warning: Multiple input files found in " + folderName + ". Changing to: " + file + "\n");
                }
                currentInputPath = file;
                FeedbackController.ShowMessage("Input file found for simulation: " + folderName + "\n");
            }
            else if (headers.SequenceEqual(soloTreesHeaders))
            {
                if (!String.IsNullOrEmpty(currentSoloTreesPath))
                {
                    FeedbackController.ShowMessage("Warning: Multiple solo trees files found in " + folderName + ". Changing to: " + file + "\n");
                }
                currentSoloTreesPath = file;
                FeedbackController.ShowMessage("Solo trees file found for simulation: " + folderName + "\n");
            }
            else if (headers.SequenceEqual(yieldTableHeaders))
            {
                if (!String.IsNullOrEmpty(currentYieldTablePath))
                {
                    FeedbackController.ShowMessage("Warning: Multiple yield table files found in " + folderName + ". Changing to: " + file + "\n");
                }
                currentYieldTablePath = file;
                FeedbackController.ShowMessage("Yield table file found for simulation: " + folderName + "\n");
            }
            else if (headers.SequenceEqual(ddTableHeaders))
            {
                if (!String.IsNullOrEmpty(currentDDPath))
                {
                    FeedbackController.ShowMessage("Warning: Multiple diameter distribution files found in " + folderName + ". Changing to: " + file + "\n");
                }
                currentDDPath = file;
                FeedbackController.ShowMessage("Diameter distribution file found for simulation: " + folderName + "\n");
            }
        }

        if (String.IsNullOrEmpty(currentInputPath))
        {
            FeedbackController.ShowMessage("Simulation ignored due to missing input file: " + folderName + "\n");
            return null;
        }
        else if (String.IsNullOrEmpty(currentSoloTreesPath))
        {
            FeedbackController.ShowMessage("Simulation ignored due to missing solo trees file: " + folderName + "\n");
            return null;
        }
        else if (String.IsNullOrEmpty(currentYieldTablePath))
        {
            FeedbackController.ShowMessage("Simulation ignored due to missing yield table file: " + folderName + "\n");
            return null;
        }
        else if (String.IsNullOrEmpty(currentDDPath))
        {
            FeedbackController.ShowMessage("Simulation ignored due to missing diameter distribution file: " + folderName + "\n");
            return null;
        }

        if (!SimulationContainsOnlySupportedSpecies(currentInputPath))
        {
            FeedbackController.ShowMessage("Simulation ignored due to unsupported species: " + folderName + "\n");
            return null;
        }
 
        var simInfo = new SimulationInfo()
        {
            folderPath = folder,
            inputPath = currentInputPath,
            soloTreesPath = currentSoloTreesPath,
            yieldTablePath = currentYieldTablePath,
            ddTablePath = currentDDPath
        };

        if (!string.IsNullOrEmpty(currentInputPath))
        {
            ParseInputFile(currentInputPath, simInfo);
        }

        return simInfo;
    }

    bool SimulationContainsOnlySupportedSpecies(string inputPath)
    {
        if (string.IsNullOrEmpty(inputPath)) return false;

        var lines = File.ReadAllLines(inputPath);
        if (lines.Length < 2) return false;

        string[] headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();

        int sp1Index = Array.IndexOf(headers, "Sp1");
        int sp2Index = Array.IndexOf(headers, "Sp2");

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');

            string sp1 = values[sp1Index].Trim();
            string sp2 = values[sp2Index].Trim();

            if (!species.Contains(sp1) || !species.Contains(sp2))
                return false;
        }

        return true;
    }

    void CreateSimulationButtons(Dictionary<string, SimulationInfo> simulations)
    {
        if (buttonPrefab == null || buttonParent == null)
        {
            Debug.LogWarning("Button prefab or parent not assigned!");
            return;
        }

        int buttonIndex = 0;
        Vector3 currentPos = new Vector3(0, 0, 0);

        foreach (var kvp in simulations)
        {
            string simName = kvp.Key;
            SimulationInfo simInfo = kvp.Value;

            for (int i = 0; i < buttonParent.Length; i++)
            {
                int index = i;  // Capture for closure

                GameObject buttonObj = Instantiate(buttonPrefab, buttonParent[i]);
                buttonObj.name = "Button_" + simName;

                RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.sizeDelta = new Vector2(200, 30);
                    rectTransform.anchoredPosition = new Vector2(currentPos.x, currentPos.y - (buttonIndex * buttonSpacing));
                }

                TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    buttonText.text = simName;
                    buttonText.color = Color.white;
                }

                Button button = buttonObj.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => OnSimulationButtonClicked(simName, simInfo, selectedSims[index], dropdowns[index], reloadButtons[index], parser[index > 0 ? 1 : 0]));
                }

                buttonIndex++;
            }
        }
    }

    void OnSimulationButtonClicked(string simName, SimulationInfo simInfo, GameObject selectedSim, GameObject dropdown, GameObject reloadButton, Parser parser)
    {
        selectedSim.SetActive(true);
        dropdown.SetActive(true);
        reloadButton.SetActive(true);

        selectedSim.GetComponentInChildren<TMP_Text>().text = simName;

        dropdown.GetComponent<IdStandsDropdown>().initDropdown(simInfo.plotDataByIdPar.Keys.ToList());

        if(parser.dpsolo != null)
        {
            if (!string.IsNullOrEmpty(parser.selectedSim1.text))
            {
                parser.gameObject.SetActive(true);
                parser.intervalInputField.gameObject.SetActive(true);
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(parser.selectedSim1.text) && !string.IsNullOrEmpty(parser.selectedSim2.text))
            {
                parser.gameObject.SetActive(true);
                parser.intervalInputField.gameObject.SetActive(true);
            }
        }
            FeedbackController.ShowMessage("Simulation '" + simName + "' selected.\n");
    }

    void ParseInputFile(string filePath, SimulationInfo simInfo)
    {
        string[] lines = File.ReadAllLines(filePath);
        if (lines.Length < 2) return;

        string[] headers = lines[0].Split(',');

        int idParIndex = System.Array.IndexOf(headers, "id_par");
        int plotShapeIndex = System.Array.IndexOf(headers, "Plot_shape");

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Trim().Split(',');

            string idPar = values[idParIndex].Trim();

            var plotShape = int.Parse(values[plotShapeIndex].Trim());
            if (plotShape == 0)
            {
                int areaIndex = System.Array.IndexOf(headers, "Aplot");
                simInfo.plotDataByIdPar[idPar] = new PlotData
                {
                    plotShape = plotShape,
                    area = float.Parse(values[areaIndex].Trim())
                };
            }
            else if (plotShape == 1 || plotShape == 2 || plotShape == 3)
            {
                int length1Index = System.Array.IndexOf(headers, "lenght1");
                int length2Index = System.Array.IndexOf(headers, "lenght2");
                simInfo.plotDataByIdPar[idPar] = new PlotData
                {
                    plotShape = plotShape,
                    length1 = int.Parse(values[length1Index].Trim()),
                    length2 = int.Parse(values[length2Index].Trim())
                };
            }
            else if (plotShape == 4)
            {
                int xcoord1Index = System.Array.IndexOf(headers, "CoordX1");
                int ycoord1Index = System.Array.IndexOf(headers, "CoordY1");
                int xcoord2Index = System.Array.IndexOf(headers, "CoordX2");
                int ycoord2Index = System.Array.IndexOf(headers, "CoordY2");
                int xcoord3Index = System.Array.IndexOf(headers, "CoordX3");
                int ycoord3Index = System.Array.IndexOf(headers, "CoordY3");
                int xcoord4Index = System.Array.IndexOf(headers, "CoordX4");
                int ycoord4Index = System.Array.IndexOf(headers, "CoordY4");

                List<float> coordX = new List<float>
                {
                    float.Parse(values[xcoord1Index].Trim()),
                    float.Parse(values[xcoord2Index].Trim()),
                    float.Parse(values[xcoord3Index].Trim()),
                    float.Parse(values[xcoord4Index].Trim())
                };

                List<float> coordY = new List<float>
                {
                    float.Parse(values[ycoord1Index].Trim()),
                    float.Parse(values[ycoord2Index].Trim()),
                    float.Parse(values[ycoord3Index].Trim()),
                    float.Parse(values[ycoord4Index].Trim())
                };

                simInfo.plotDataByIdPar[idPar] = new PlotData
                {
                    plotShape = plotShape,
                    minX = coordX.Min(),
                    maxX = coordX.Max(),
                    minY = coordY.Min(),
                    maxY = coordY.Max()
                };
            }
        }
    }

    public void ReloadSims()
    {
        simMetadata.simulations.Clear();
        simMetadata.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void resetSelectedSims()
    {
        foreach (var selectedSim in selectedSims)
        {
            selectedSim.GetComponentInChildren<TMP_Text>().text = "";
            selectedSim.SetActive(false);
        }
        foreach (var dropdown in dropdowns)
        {
            dropdown.GetComponent<TMP_Dropdown>().ClearOptions();
            dropdown.SetActive(false);
        }
        foreach(var reloadButton in reloadButtons)
        {
            reloadButton.SetActive(false);
        }
        foreach (var p in parser)
        {
            p.gameObject.SetActive(false);
            p.intervalInputField.gameObject.SetActive(false);
        }
    }
}

public class SimulationInfo
{
    public string folderPath, inputPath, soloTreesPath, yieldTablePath, ddTablePath;
    public Dictionary<string, PlotData> plotDataByIdPar = new Dictionary<string, PlotData>();
}

public class PlotData
{
    public int plotShape;
    public float area;
    public float length1;
    public float length2;
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;
}