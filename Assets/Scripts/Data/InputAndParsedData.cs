using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "InputAndParsedData", menuName = "Scriptable Objects/InputAndParsedData")]
public class InputAndParsedData : ScriptableObject
{
    [Header("Selected stand")]
    public string selectedId_stand1;
    public string selectedId_stand2;

    [Header("Plot shape settings")]
    public List<(int, List<float>)> plotShapeAndDimensions;

    [System.NonSerialized]
    public SortedDictionary<string, SortedDictionary<string, List<SortedDictionary<int, TreeData>>>> outputSoloTreesData;
    [System.NonSerialized]
    public SortedDictionary<string, SortedDictionary<string, List<YieldTableEntry>>> outputYieldTable;
}
