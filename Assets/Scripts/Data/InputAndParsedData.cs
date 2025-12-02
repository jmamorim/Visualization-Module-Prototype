using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "InputAndParsedData", menuName = "Scriptable Objects/InputAndParsedData")]
public class InputAndParsedData : ScriptableObject
{
    [Header("Plot shape settings")]
    public List<(int, List<float>)> plotShapeAndDimensions;

    [System.NonSerialized]
    public SortedDictionary<string, SortedDictionary<string, List<SortedDictionary<int, TreeData>>>> outputSoloTreesData;
    [System.NonSerialized]
    public SortedDictionary<string, SortedDictionary<string, List<YieldTableEntry>>> outputYieldTable;
    [System.NonSerialized]
    public SortedDictionary<string, SortedDictionary<string, List<DDEntry>>> outputDDTable;
}
