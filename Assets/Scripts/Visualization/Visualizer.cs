using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

// Needs use of terrain and the speedtree models
public class Visualizer : MonoBehaviour
{
    //Pinheiro-bravo models yongest to seniour
    public List<GameObject> pbPrefabs;
    public GameObject plot;
    public TMP_Text yearText;
    public GraphGenerator graphGenerator;

    private int currentYear;
    private List<Tree> trees;
    private List<GameObject> treeInstances;

    [SerializeField]
    //ages 3-7-10-15-20
    float pbAdultStartingAge, pbYoungAdultAge, pbMidAdultAge, pbSeniourStartingAge;
    [SerializeField]
    //heights
    float thresholdPbYoungHeight, thresholdPbAdultHeight, thresholdPbSeniourHeight;

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

                GameObject prefab = getPrefabForCurrentHeight(tree.t);
                float factor = calculatePBFactor(tree.h, tree.t);

                GameObject instance = Instantiate(prefab, position, rotation, plot.transform);
                instance.transform.localScale = Vector3.one * factor;

                treeInstances.Add(instance);
            }
        }
    }

    private GameObject getPrefabForCurrentHeight(float currentAge)
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

    //creates objects that act as complementary data
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

