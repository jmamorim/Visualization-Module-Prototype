using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class Visualizer : MonoBehaviour
{
    //Pinheiro-bravo models yongest to seniour
    [Header("Pinheiro-bravo Prefabs")]
    public List<GameObject> pbPrefabs;
    [Header("Pinheiro-manso Prefabs")]
    public List<GameObject> pmPrefabs;
    public string activeSpecie;

    [Header("Scene References")]
    public GameObject plot;
    public TMP_Text yearText;
    public GraphGenerator graphGenerator;

    [Header("Pinheiro-bravo Parameters"), Tooltip("Ages at which Pinheiro-bravo transitions between stages")]
    [SerializeField] float pbAdultStartingAge;
    [SerializeField] float pbYoungAdultAge;
    [SerializeField] float pbMidAdultAge;
    [SerializeField] float pbSeniourStartingAge;

    [Space(5)]
    [Header("Pinheiro-bravo Height Thresholds")]
    [SerializeField] float thresholdPbYoungHeight;
    [SerializeField] float thresholdPbAdultHeight;
    [SerializeField] float thresholdPbSeniourHeight;

    [Header("Pinheiro-manso Parameters"), Tooltip("Ages at which Pinheiro-manso transitions between stages")]
    [SerializeField] float pmMidYougAge;
    [SerializeField] float pmAdultStartingAge;
    [SerializeField] float pmYoungAdultAge;
    //[SerializeField] float pmMidAdultAge;
    [SerializeField] float pmSeniourStartingAge;

    [Space(5)]
    [Header("Pinheiro-manso Height Thresholds")]
    [SerializeField] float thresholdPmYoungHeight;
    [SerializeField] float thresholdPmAdultHeight;
    [SerializeField] float thresholdPmSeniourHeight;

    int currentYear;
    List<Tree> trees;
    List<GameObject> treeInstances;
    readonly List<string> species = new List<string> { "pb", "pm", "eg", "cs" };


    public void receiveTreeData(SortedDictionary<int, Tree> data, int currentYear)
    {
        trees = data.Values.ToList();
        this.currentYear = currentYear;
        createObjects();
        graphGenerator.receiveData(data, null);
        displayTrees();
    }

    public void displayTrees()
    {
        yearText.text = $"Year: {currentYear}";

        if (trees == null || trees.Count == 0) return;

        // Apaga instâncias antigas
        if (treeInstances != null)
        {
            foreach (GameObject obj in treeInstances)
            {
                if (obj != null) Destroy(obj);
            }
        }
        treeInstances = new List<GameObject>();

        float xOffset = trees.Average(tree => tree.Xarv);
        float yOffset = trees.Average(tree => tree.Yarv);

        foreach (Tree tree in trees)
        {
            // Só mostra árvores vivas por agora
            if (tree.estado == 0)
            {

                float adjustedX = (tree.Xarv - xOffset) * 3f;
                float adjustedZ = (tree.Yarv - yOffset) * 3f;
                Vector3 position = new Vector3(adjustedX, 0f, adjustedZ);
                Quaternion rotation = Quaternion.Euler(0, tree.rotation, 0);

                //needs to be changes so it takes into account the specie
                GameObject prefab = null;
                float factor = 0;

                GetFactorAndPrefabSpecie(activeSpecie, tree.h, tree.t, out factor, out prefab);

                GameObject instance = Instantiate(prefab, position, rotation, plot.transform);
                instance.transform.localScale = Vector3.one * factor;

                treeInstances.Add(instance);
            }
        }
    }

    private void GetFactorAndPrefabSpecie(string specie, float height, float age, out float factor, out GameObject prefab)
    {
        factor = 0f;
        prefab = null;

        if (specie.Equals(species[0]))
        {
            prefab = getPBPrefabForCurrentAge(age);
            factor = calculatePBFactor(height, age);
        }
        else if (specie.Equals(species[1]))
        {
            prefab = getPmPrefabForCurrentHeight(age);
            factor = calculatePMFactor(height, age);
        }
    }

    private GameObject getPBPrefabForCurrentAge(float currentAge)
    {
        if (currentAge < pbAdultStartingAge)
            return pbPrefabs[0];
        else if (currentAge >= pbAdultStartingAge && currentAge < pbYoungAdultAge)
            return pbPrefabs[1];
        else if (currentAge >= pbAdultStartingAge && currentAge < pbMidAdultAge)
            return pbPrefabs[2];
        else if (currentAge >= pbAdultStartingAge && currentAge < pbSeniourStartingAge)
            return pbPrefabs[3];
        else
            return pbPrefabs[4];
    }

    private GameObject getPmPrefabForCurrentHeight(float currentAge)
    {
        if (currentAge < pmMidYougAge)
            return pmPrefabs[0];
        else if (currentAge >= pmMidYougAge && currentAge < pmAdultStartingAge)
            return pmPrefabs[1];
        else if (currentAge >= pmAdultStartingAge && currentAge < pmSeniourStartingAge)
            return pmPrefabs[2];
        else
            return pmPrefabs[3];
    }

    private float calculatePBFactor(float currentHeight, float currentAge)
    {
        if (currentAge < pbAdultStartingAge)
        {
            return currentHeight / thresholdPbYoungHeight;
        }
        else if (currentAge >= pbAdultStartingAge && currentAge < pbSeniourStartingAge)
        {
            return currentHeight / thresholdPbAdultHeight;
        }
        else
        {
            return currentHeight / thresholdPbSeniourHeight;
        }
    }

    private float calculatePMFactor(float currentHeight, float currentAge)
    {
        if (currentAge < pmAdultStartingAge)
        {
            return currentHeight / thresholdPmYoungHeight;
        }
        else if (currentAge >= pmAdultStartingAge && currentAge < pmSeniourStartingAge)
        {
            return currentHeight / thresholdPmAdultHeight;
        }
        else
        {
            return currentHeight / thresholdPmSeniourHeight;
        }
    }

    //creates empty objects that act as an hitbox for each tree so when clicking on a tree displays tree data
    public void createObjects()
    {
        if (trees == null || trees.Count == 0) return;

        // Clean up previous objects
        if (plot != null)
        {
            foreach (Transform child in plot.transform)
            {
                Destroy(child.gameObject);
            }
        }

        float xOffset = trees.Average(tree => tree.Xarv);
        float yOffset = trees.Average(tree => tree.Yarv);

        foreach (Tree tree in trees)
        {
            if (tree.estado == 4 || tree.estado == 6) continue;

            float adjustedX = (tree.Xarv - xOffset) * 3f;
            float adjustedZ = (tree.Yarv - yOffset) * 3f;
            float treeHeight = tree.h * 0.25f;

            Vector3 position = new Vector3(adjustedX, treeHeight, adjustedZ);

            GameObject marker = new GameObject($"TreeMarker_{tree.id_arv}");
            marker.transform.position = position;
            marker.transform.rotation = Quaternion.Euler(0, tree.rotation, 0);
            marker.transform.SetParent(plot != null ? plot.transform : plot.transform);

            BoxCollider col = marker.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(tree.cw * 2, tree.h, tree.cw * 2);
            col.center = new Vector3(0, treeHeight, 0);

            marker.AddComponent<Tree>().initTree(tree);
        }
    }

}

