using System;
using System.Collections.Generic;
using UnityEngine;

public class PlotButton : MonoBehaviour
{
    public InterfaceManager interfaceManager;
    public int plotIndex;

    public void OnPlotButtonClick()
    {
        interfaceManager.SetActivePlotIndex(plotIndex);
    }
}
