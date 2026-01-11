using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class Initializer : MonoBehaviour
{
    public SimMetadata simMetadata;
    public TMP_Dropdown[] simulationDropdowns;
    public GameObject[] standsText;
    public TMP_Dropdown[] idstandDropdown;
    public GameObject[] simReadme;
    public GameObject[] simPrescs;
    public GameObject[] simInfoBox;
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
                continue;
            }

            var simInfo = ProcessSimulationFolder(folder, folderName);
            if (simInfo != null)
            {
                simulations.Add(folderName, simInfo);
                Debug.Log("Registered new simulation: " + folderName);
            }
        }

        simMetadata.simulations = simulations;
        PopulateSimulationDropdowns(simulations);
        simMetadata.Save();
    }

    public void ReloadSimulation(string simName)
    {
        string simsRoot = Path.Combine(Application.streamingAssetsPath, "Simulations");
        string folderPath = Path.Combine(simsRoot, simName);

        if (!Directory.Exists(folderPath))
        {
            Debug.LogError("Simulation folder not found: " + folderPath);
            return;
        }

        if (simMetadata.simulations.ContainsKey(simName))
        {
            simMetadata.simulations.Remove(simName);
        }

        var simInfo = ProcessSimulationFolder(folderPath, simName);

        if (simInfo != null)
        {
            simMetadata.simulations.Add(simName, simInfo);
            simMetadata.Save();

            // Repopulate dropdowns
            PopulateSimulationDropdowns(simMetadata.simulations);

            Debug.Log("Reloaded simulation: " + simName);
            resetSelectedSims();
        }
        else
        {
            Debug.LogError("Failed to reload simulation: " + simName);
        }
    }

    SimulationInfo ProcessSimulationFolder(string folder, string folderName)
    {
        string[] csvs = Directory.GetFiles(folder, "*.csv");
        if (csvs.Length == 0)
        {
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
                currentInputPath = file;
            }
            else if (headers.SequenceEqual(soloTreesHeaders))
            {
                currentSoloTreesPath = file;
            }
            else if (headers.SequenceEqual(yieldTableHeaders))
            {
                currentYieldTablePath = file;
            }
            else if (headers.SequenceEqual(ddTableHeaders))
            {
                currentDDPath = file;
            }
        }

        if (String.IsNullOrEmpty(currentInputPath))
        {
            return null;
        }
        else if (String.IsNullOrEmpty(currentSoloTreesPath))
        {
            return null;
        }
        else if (String.IsNullOrEmpty(currentYieldTablePath))
        {
            return null;
        }
        else if (String.IsNullOrEmpty(currentDDPath))
        {
            return null;
        }

        if (!SimulationContainsOnlySupportedSpecies(currentInputPath))
        {
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

        // Parse input file and store input data
        if (!string.IsNullOrEmpty(currentInputPath))
        {
            ParseInputFile(currentInputPath, simInfo);
        }

        // Parse yield table to get prescriptions for each plot
        if (!string.IsNullOrEmpty(currentYieldTablePath))
        {
            ParsePrescriptionsFromYieldTable(currentYieldTablePath, simInfo);
        }

        string readmePath = Path.Combine(folder, "readme.txt");
        if (File.Exists(readmePath))
        {
            try
            {
                simInfo.readmeContent = File.ReadAllText(readmePath);
                Debug.Log("Loaded readme for simulation: " + folderName);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Failed to read readme.txt for " + folderName + ": " + e.Message);
                simInfo.readmeContent = "";
            }
        }
        else
        {
            Debug.LogWarning("No readme.txt found for simulation: " + folderName);
            simInfo.readmeContent = "";
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

    void PopulateSimulationDropdowns(Dictionary<string, SimulationInfo> simulations)
    {
        if (simulationDropdowns == null || simulationDropdowns.Length == 0)
        {
            Debug.LogWarning("Simulation dropdowns not assigned!");
            return;
        }

        List<string> simNames = new List<string> { "Escolha uma simulação..." };
        simNames.AddRange(simulations.Keys.OrderBy(k => k));

        for (int i = 0; i < simulationDropdowns.Length; i++)
        {
            TMP_Dropdown dropdown = simulationDropdowns[i];
            int index = i; // Capture for closure

            dropdown.ClearOptions();
            dropdown.AddOptions(simNames);
            dropdown.value = 0;

            dropdown.onValueChanged.RemoveAllListeners();

            dropdown.onValueChanged.AddListener((value) =>
            {
                if (value > 0)
                {
                    string selectedSimName = simNames[value];
                    SimulationInfo simInfo = simulations[selectedSimName];
                    standsText[index].SetActive(true);
                    OnSimulationSelected(selectedSimName, simInfo, idstandDropdown[index], simReadme[index], reloadButtons[index]);
                }
                else
                {
                    standsText[index].SetActive(false);
                    idstandDropdown[index].ClearOptions();
                    idstandDropdown[index].gameObject.SetActive(false);
                    simReadme[index].SetActive(false);
                    simPrescs[index].SetActive(false);
                    simInfoBox[index].SetActive(false);
                    reloadButtons[index].SetActive(false);
                    var curParser = parser[index > 0 ? 1 : 0];
                    curParser.gameObject.SetActive(false);
                    curParser.intervalInputField.gameObject.SetActive(false);
                }
            });
        }
    }

    void OnSimulationSelected(string simName, SimulationInfo simInfo, TMP_Dropdown dropdown, GameObject readme, GameObject reloadButton)
    {
        dropdown.gameObject.SetActive(true);
        reloadButton.SetActive(true);
        readme.SetActive(true);
        readme.GetComponentInChildren<TMP_Text>().text = simInfo.readmeContent;
        dropdown.GetComponent<IdStandsDropdown>().initDropdown(simInfo.plotDataByIdPar.Keys.ToList());
    }

    void ParseInputFile(string filePath, SimulationInfo simInfo)
    {
        string[] lines = File.ReadAllLines(filePath);
        if (lines.Length < 2) return;

        string[] headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();

        // Get all header indices
        int idParIndex = System.Array.IndexOf(headers, "id_par");
        int plotShapeIndex = System.Array.IndexOf(headers, "Plot_shape");

        // Store input data for each id_par
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Trim().Split(',');
            if (values.Length < headers.Length) continue;

            string idPar = values[idParIndex].Trim();

            var plotShape = int.Parse(values[plotShapeIndex].Trim());

            PlotData plotData = new PlotData
            {
                plotShape = plotShape
            };

            plotData.id_par = GetValue(headers, values, "id_par");
            plotData.AreaUG = GetFloatValue(headers, values, "AreaUG");
            plotData.id_presc = GetValue(headers, values, "id_presc");
            plotData.tlag = GetFloatValue(headers, values, "tlag");
            plotData.CoordX = GetFloatValue(headers, values, "CoordX");
            plotData.CoordY = GetFloatValue(headers, values, "CoordY");
            plotData.id_meteo = GetValue(headers, values, "id_meteo");
            plotData.Altitude = GetFloatValue(headers, values, "Altitude");
            plotData.year = GetIntValue(headers, values, "year");
            plotData.month = GetIntValue(headers, values, "month");
            plotData.composition = GetValue(headers, values, "composition");
            plotData.PlotType = GetValue(headers, values, "PlotType");
            plotData.Sp1 = GetValue(headers, values, "Sp1");
            plotData.Sp2 = GetValue(headers, values, "Sp2");
            plotData.Structure = GetValue(headers, values, "Structure");
            plotData.S = GetFloatValue(headers, values, "S");
            plotData.rot = GetIntValue(headers, values, "rot");
            plotData.t = GetFloatValue(headers, values, "t");
            plotData.tst = GetFloatValue(headers, values, "tst");
            plotData.tsd = GetFloatValue(headers, values, "tsd");
            plotData.Narvp = GetIntValue(headers, values, "Narvp");
            plotData.Aplot = GetFloatValue(headers, values, "Aplot");

            if (plotShape == 0)
            {
                plotData.area = plotData.Aplot;
            }
            else if (plotShape == 1 || plotShape == 2 || plotShape == 3)
            {
                plotData.length1 = GetFloatValue(headers, values, "lenght1");
                plotData.length2 = GetFloatValue(headers, values, "lenght2");
            }
            else if (plotShape == 4)
            {
                plotData.CoordX1 = GetFloatValue(headers, values, "CoordX1");
                plotData.CoordY1 = GetFloatValue(headers, values, "CoordY1");
                plotData.CoordX2 = GetFloatValue(headers, values, "CoordX2");
                plotData.CoordY2 = GetFloatValue(headers, values, "CoordY2");
                plotData.CoordX3 = GetFloatValue(headers, values, "CoordX3");
                plotData.CoordY3 = GetFloatValue(headers, values, "CoordY3");
                plotData.CoordX4 = GetFloatValue(headers, values, "CoordX4");
                plotData.CoordY4 = GetFloatValue(headers, values, "CoordY4");

                List<float> coordX = new List<float> { plotData.CoordX1, plotData.CoordX2, plotData.CoordX3, plotData.CoordX4 };
                List<float> coordY = new List<float> { plotData.CoordY1, plotData.CoordY2, plotData.CoordY3, plotData.CoordY4 };

                plotData.minX = coordX.Min();
                plotData.maxX = coordX.Max();
                plotData.minY = coordY.Min();
                plotData.maxY = coordY.Max();
            }

            simInfo.plotDataByIdPar[idPar] = plotData;
        }
    }

    void ParsePrescriptionsFromYieldTable(string filePath, SimulationInfo simInfo)
    {
        string[] lines = File.ReadAllLines(filePath);
        if (lines.Length < 2) return;

        string[] headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();

        int idStandIndex = System.Array.IndexOf(headers, "id_stand");
        int idPrescIndex = System.Array.IndexOf(headers, "id_presc");

        if (idStandIndex == -1 || idPrescIndex == -1) return;

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Trim().Split(',');
            if (values.Length <= Math.Max(idStandIndex, idPrescIndex)) continue;

            string idStand = values[idStandIndex].Trim();
            string idPresc = values[idPrescIndex].Trim();

            if (simInfo.plotDataByIdPar.ContainsKey(idStand))
            {
                if (!simInfo.plotDataByIdPar[idStand].prescriptions.Contains(idPresc))
                {
                    simInfo.plotDataByIdPar[idStand].prescriptions.Add(idPresc);
                }
            }
        }
    }

    string GetValue(string[] headers, string[] values, string header)
    {
        int index = System.Array.IndexOf(headers, header);
        if (index >= 0 && index < values.Length)
            return values[index].Trim();
        return "";
    }

    float GetFloatValue(string[] headers, string[] values, string header)
    {
        string val = GetValue(headers, values, header);
        float result;
        if (float.TryParse(val, out result))
            return result;
        return 0f;
    }

    int GetIntValue(string[] headers, string[] values, string header)
    {
        string val = GetValue(headers, values, header);
        int result;
        if (int.TryParse(val, out result))
            return result;
        return 0;
    }

    public void ReloadSims()
    {
        simMetadata.simulations.Clear();
        simMetadata.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void resetSelectedSims()
    {
        foreach (var dropdown in idstandDropdown)
        {
            dropdown.GetComponent<TMP_Dropdown>().ClearOptions();
            dropdown.gameObject.SetActive(false);
            dropdown.GetComponent<IdStandsDropdown>().textSimInfo.SetActive(false);
        }
        foreach (var reloadButton in reloadButtons)
        {
            reloadButton.SetActive(false);
        }
        foreach (var p in parser)
        {
            p.gameObject.SetActive(false);
            p.intervalInputField.gameObject.SetActive(false);
        }

        foreach (var simDropdown in simulationDropdowns)
        {
            simDropdown.value = 0;
        }

        foreach (var idDropdown in idstandDropdown)
        {
            idDropdown.value = 0;
        }
    }
}

public class SimulationInfo
{
    public string folderPath, inputPath, soloTreesPath, yieldTablePath, ddTablePath;
    public string readmeContent;
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

    public string id_par;
    public float AreaUG;
    public string id_presc;
    public float tlag;
    public float CoordX;
    public float CoordY;
    public string id_meteo;
    public float Altitude;
    public int year;
    public int month;
    public string composition;
    public string PlotType;
    public string Sp1;
    public string Sp2;
    public string Structure;
    public float S;
    public int rot;
    public float t;
    public float tst;
    public float tsd;
    public int Narvp;
    public float Aplot;

    public float CoordX1;
    public float CoordY1;
    public float CoordX2;
    public float CoordY2;
    public float CoordX3;
    public float CoordY3;
    public float CoordX4;
    public float CoordY4;

    public List<string> prescriptions = new List<string>();
}