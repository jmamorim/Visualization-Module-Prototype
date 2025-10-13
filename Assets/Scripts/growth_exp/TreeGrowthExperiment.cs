using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TreeGrowthExperiment : MonoBehaviour
{
    //from yongest to seniour
    public List<GameObject> prefabs;
    public Transform treeLocation;
    public float stepHeight;

    const float minHeight = 0.5f;
    const float maxHeight = 35f;
    const int minAge = 1;


    GameObject treeInstance;
    GameObject currentPrefab;

    [SerializeField]
    int currentAge = 1;
    [SerializeField]
    float currentHeight = 3f;
    [SerializeField]
    float factor;
    [SerializeField]
    //ages 3-7-10-15-20
    float midYoungAge, adultStartingAge, youngAdultAge, midAdultAge, seniourStartingAge;
    [SerializeField]
    //heights
    float thresholdYoungHeight, thresholdAdultHeight, thresholdSeniourHeight;

    private void Start()
    {
        factor = calculateFactor();
        treeInstance = instantiateTree(getPrefabForCurrentHeight());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentHeight = Mathf.Min(currentHeight + stepHeight, maxHeight);
            currentAge += 1;
            instatiateTree();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentHeight = Mathf.Max(currentHeight - stepHeight, minHeight);
            currentAge = Mathf.Max(currentAge - 1, minAge);
            instatiateTree();
        }
    }

    private void instatiateTree()
    {
        factor = calculateFactor();

        GameObject newPrefab = getPrefabForCurrentHeight();

        if (newPrefab != currentPrefab)
        {
            if (treeInstance != null) Destroy(treeInstance);
            treeInstance = instantiateTree(newPrefab);
        }
        else
        {
            treeInstance.transform.localScale = Vector3.one * factor;
        }
    }

    //h = f*initialh 
    private float calculateFactor()
    {
        if (currentAge < adultStartingAge)
        {
            return currentHeight / thresholdYoungHeight;
        }
        else if (currentAge >= adultStartingAge && currentAge < seniourStartingAge)
        {
            return currentHeight / thresholdAdultHeight;
        }
        else
        {
            return currentHeight / thresholdSeniourHeight;
        }
    }

    private GameObject instantiateTree(GameObject prefab)
    {
        currentPrefab = prefab;
        GameObject instance = Instantiate(prefab, treeLocation.position, Quaternion.identity);
        instance.transform.localScale = Vector3.one * factor;
        return instance;
    }


    private GameObject getPrefabForCurrentHeight()
    {
        if (currentAge < midYoungAge)
            return prefabs[0];
        else if (currentAge >= midYoungAge && currentAge < adultStartingAge)
            return prefabs[1];
        else if (currentAge >= adultStartingAge && currentAge < youngAdultAge)
            return prefabs[2];
        else if (currentAge >= midAdultAge && currentAge < seniourStartingAge)
            return prefabs[3];
        else
            return prefabs[4];
    }
}
