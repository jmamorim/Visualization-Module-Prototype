using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

[CreateAssetMenu(fileName = "SimMetadata", menuName = "Scriptable Objects/SimMetadata")]
public class SimMetadata : ScriptableObject
{
    //Dictionary of simulation names to their file paths and plot shape/diemensions
    [Header("Simulations metadata")]
    public Dictionary<string, SimulationInfo> simulations = new Dictionary<string, SimulationInfo>();
    public void Save()
    {
        string path = Path.Combine(Application.persistentDataPath, "simulations.json");
        File.WriteAllText(path, JsonConvert.SerializeObject(simulations, Formatting.Indented));
        Debug.Log("Simulations metadata saved to: " + path);
    }

    public void Load()
    {
        string path = Path.Combine(Application.persistentDataPath, "simulations.json");
        if (File.Exists(path))
        {
            simulations = JsonConvert.DeserializeObject<Dictionary<string, SimulationInfo>>(File.ReadAllText(path));
            Debug.Log("Simulations metadata loaded from: " + path);
        }
    }

}
