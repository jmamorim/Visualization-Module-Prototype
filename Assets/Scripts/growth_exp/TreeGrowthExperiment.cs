using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TreeGrowthExperiment : MonoBehaviour
{
    public GameObject youngTreePrefab, adultTreePrefab, seniourTreePrefab;
    public Transform treeLocation;

    const float minHeight = 1f;
    const float stepHeight = 0.5f;

    float factor;
    float currentHeight = 1f;
    GameObject treeInstance;
    GameObject currentPrefab;

    [SerializeField]
    float thresholdYoungHeight, thresholdAdultHeight, thresholdSeniourHeight;

    private void Start()
    {
        factor = calculateFactor();
        treeInstance = instantiateTree(getPrefabForCurrentHeight());
    }

    private void Update()
    {
        if (currentHeight <= thresholdSeniourHeight && currentHeight >= minHeight) {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                currentHeight = Mathf.Min(currentHeight + stepHeight, thresholdSeniourHeight);
                instatiateTree();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                currentHeight = Mathf.Max(currentHeight - stepHeight, minHeight);
                instatiateTree();
            }
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
        if (currentHeight < thresholdYoungHeight)
        {
            return currentHeight / thresholdYoungHeight;
        }
        else if (thresholdYoungHeight <= currentHeight && currentHeight < thresholdAdultHeight)
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
        if (currentHeight < thresholdYoungHeight)
            return youngTreePrefab;
        else if (currentHeight < thresholdAdultHeight)
            return adultTreePrefab;
        else
            return seniourTreePrefab;
    }
}
