using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IdStandsDropdown : MonoBehaviour
{
    public bool isMainPlot;
    public GameObject prescInfo, textSimInfo, simInfo, intervalField, parseButton;

    public TMP_Text prescInfoText;
    public TMP_Text simInfoText;

    //used to get the info from metadata
    public Initializer initializer;
    //if multi-visualization is enabled, to check if can enable the parsing button
    public TMP_Dropdown dropdown2 = null, simulationDropdown;

    private TMP_Dropdown dropdown;
    private string currentSimulationName;

    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    private void Start()
    {
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    private void OnDestroy()
    {
        if (dropdown != null)
        {
            dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
        }
    }

    public void initDropdown(List<string> idStands)
    {
        dropdown.ClearOptions();

        List<string> standIds = new List<string> { "Escolha um povoamento..." };
        standIds.AddRange(idStands);
        dropdown.AddOptions(standIds);
        dropdown.value = 0;
        dropdown.RefreshShownValue();

        if (simulationDropdown != null && simulationDropdown.value > 0)
        {
            currentSimulationName = simulationDropdown.options[simulationDropdown.value].text;
        }

        OnDropdownValueChanged(dropdown.value);
    }

    public void OnDropdownValueChanged(int value)
    {
        if (dropdown == null || dropdown.options.Count == 0) return;

        if (value != 0)
        {
            string selectedStandId = dropdown.options[value].text;

            DisplayStandInfo(selectedStandId);

            prescInfo.SetActive(true);
            textSimInfo.SetActive(true);
            simInfo.SetActive(true);

            if (dropdown2 == null)
            {
                intervalField.SetActive(true);
                parseButton.SetActive(true);
            }
            else
            {
                if (dropdown2.value != 0)
                {
                    intervalField.SetActive(true);
                    parseButton.SetActive(true);
                }
            }
        }
        else
        {
            prescInfo.SetActive(false);
            textSimInfo.SetActive(false);
            simInfo.SetActive(false);
            intervalField.SetActive(false);
            parseButton.SetActive(false);
        }
    }

    private void DisplayStandInfo(string standId)
    {
        if (initializer == null || string.IsNullOrEmpty(currentSimulationName))
        {
            Debug.LogWarning("Initializer ou nome da simulação não encontrado");
            return;
        }

        if (!initializer.simMetadata.simulations.ContainsKey(currentSimulationName))
        {
            Debug.LogWarning("Simulação não encontrada: " + currentSimulationName);
            return;
        }

        SimulationInfo simInfo = initializer.simMetadata.simulations[currentSimulationName];

        if (!simInfo.plotDataByIdPar.ContainsKey(standId))
        {
            Debug.LogWarning("Povoamento não encontrado: " + standId);
            return;
        }

        PlotData plotData = simInfo.plotDataByIdPar[standId];

        if (prescInfoText != null)
        {
            string prescText = "<b>Prescrições do Povoamento:</b>\n\n";

            if (plotData.prescriptions.Count > 0)
            {
                prescText += "Prescrições:\n";
                foreach (string presc in plotData.prescriptions)
                {
                    prescText += "• " + presc + "\n";
                }
            }
            else
            {
                prescText += "Nenhuma prescrição encontrada";
            }

            prescInfoText.text = prescText;
        }

        if (simInfoText != null)
        {
            string infoText = "<b>Informações do Povoamento:</b>\n\n";

            infoText += "<b>Identificação:</b>\n";
            infoText += "ID Povoamento: " + plotData.id_par + "\n";
            infoText += "Área: " + plotData.AreaUG.ToString("F2") + " ha\n";
            infoText += "ID Meteorologico: " + plotData.id_meteo + "\n\n";

            infoText += "<b>Localização:</b>\n";

            if (plotData.CoordX == 0 && plotData.CoordY == 0)
            {
                infoText += "Coordenadas: Desconhecido\n";
            }
            else
            {
                infoText += "Coordenadas: (" + plotData.CoordX.ToString("F2") + ", " + plotData.CoordY.ToString("F2") + ")\n";
            }

            infoText += "Altitude: " + plotData.Altitude.ToString("F0") + " m\n\n";

            infoText += "<b>Composição:</b>\n";
            infoText += "Espécie Principal: " + GetSpeciesName(plotData.Sp1) + "\n";
            infoText += "Espécie Secundária: " + GetSpeciesName(plotData.Sp2) + "\n";
            infoText += "Composição: " + GetCompositionName(plotData.composition) + "\n";
            infoText += "Estrutura: " + GetStructureName(plotData.Structure) + "\n\n";

            infoText += "<b>Características da Parcela:</b>\n";
            infoText += "Forma da Parcela: " + GetPlotShapeName(plotData.plotShape) + "\n";

            if (plotData.plotShape == 0)
            {
                infoText += "Área da Parcela: " + plotData.Aplot.ToString("F2") + " m²\n";
            }
            else if (plotData.plotShape == 1 || plotData.plotShape == 2 || plotData.plotShape == 3)
            {
                infoText += "Comprimento 1: " + plotData.length1.ToString("F2") + " m\n";
                infoText += "Comprimento 2: " + plotData.length2.ToString("F2") + " m\n";
            }
            else if (plotData.plotShape == 4)
            {
                infoText += "Coordenadas X: [" + plotData.CoordX1.ToString("F2") + ", " + plotData.CoordX2.ToString("F2") + ", " + plotData.CoordX3.ToString("F2") + ", " + plotData.CoordX4.ToString("F2") + "]\n";
                infoText += "Coordenadas Y: [" + plotData.CoordY1.ToString("F2") + ", " + plotData.CoordY2.ToString("F2") + ", " + plotData.CoordY3.ToString("F2") + ", " + plotData.CoordY4.ToString("F2") + "]\n";
            }

            infoText += "Nº Árvores/Parcela: " + plotData.Narvp + "\n\n";

            infoText += "<b>Parâmetros Temporais:</b>\n";
            infoText += "Ano: " + plotData.year + "\n";
            infoText += "Mês: " + plotData.month + "\n";
            infoText += "Idade (t): " + plotData.t.ToString("F1") + " anos\n";
            infoText += "Rotação: " + plotData.rot + " ano/s\n";

            simInfoText.text = infoText;
        }
    }

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

    private string GetCompositionName(string composition)
    {
        if (string.IsNullOrEmpty(composition))
            return "Desconhecido";

        switch (composition.ToLower())
        {
            case "pure":
                return "Puro";
            case "mixed":
                return "Misto";
            default:
                return composition;
        }
    }

    private string GetStructureName(string structure)
    {
        if (string.IsNullOrEmpty(structure))
            return "Desconhecido";

        switch (structure.ToUpper())
        {
            case "R":
                return "Regular";
            case "J":
                return "Irregular";
            default:
                return structure;
        }
    }


    private string GetPlotShapeName(int shape)
    {
        switch (shape)
        {
            case 0:
                return "Desconhecida";
            case 1:
                return "Quadrado";
            case 2:
                return "Rectangular";
            case 3:
                return "Circular";
            case 4:
                return "Polígono Irregular";
            default:
                return "Desconhecida";
        }
    }
}