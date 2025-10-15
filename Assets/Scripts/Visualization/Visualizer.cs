using System.Collections.Generic;
using System.Linq;
using TMPro;
using TreeEditor;
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
    public TMP_Text yearText1;
    public TMP_Text yearText2;
    public GraphGenerator graphGenerator;
    public Terrain terrain1;
    public Terrain terrain2;

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
    readonly List<string> species = new List<string> { "pb", "pm", "eg", "cs" };

    private void Start()
    {
        // Reset terrains to avoid modifying original terrain data so unity doesnt serialize changes
        terrain1.terrainData = Instantiate(terrain1.terrainData);
        terrain2.terrainData = Instantiate(terrain2.terrainData);
    }

    public void receiveTreeDataPlot1(SortedDictionary<int, TreeData> data, int currentYear)
    {
        clear("Plot1");
        var trees = data.Values.ToList();
        createObjects(trees, terrain1, false);
        yearText1.text = "Year: " + currentYear.ToString();
        displayTrees(trees, terrain1);
    }

    public void receiveTreeDataPlot2(SortedDictionary<int, TreeData> data, int currentYear)
    {
        clear("Plot2");
        var trees = data.Values.ToList();
        createObjects(trees, terrain2, true);
        yearText2.text = "Year: " + currentYear.ToString();
        displayTrees(trees, terrain2);
    }

    public void displayTrees(List<TreeData> trees, Terrain terrain)
    {
        if (trees == null || trees.Count == 0) return;

        //terrain handling
        terrain.terrainData.treeInstances = new TreeInstance[0];
        TreePrototype[] prototypes = new TreePrototype[pbPrefabs.Count + pmPrefabs.Count];
        for (int i = 0; i < pbPrefabs.Count; i++)
        {
            prototypes[i] = new TreePrototype { prefab = pbPrefabs[i] };
        }
        for (int i = 0; i < pmPrefabs.Count; i++)
        {
            prototypes[pbPrefabs.Count + i] = new TreePrototype { prefab = pmPrefabs[i] };
        }
        terrain.terrainData.treePrototypes = prototypes;

        List<TreeInstance> treeInstancesTerrain = new List<TreeInstance>();

        foreach (TreeData tree in trees)
        {
            if (tree.estado == 0)
            {
                float adjustedX = (tree.Xarv) * 3f;
                float adjustedZ = (tree.Yarv) * 3f;

                float worldX = terrain.transform.position.x + adjustedX;
                float worldZ = terrain.transform.position.z + adjustedZ;

                float normX = (worldX - terrain.transform.position.x) / terrain.terrainData.size.x;
                float normZ = (worldZ - terrain.transform.position.z) / terrain.terrainData.size.z;

                GameObject prefab = null;
                float factor = 0;
                GetFactorAndPrefabSpecie(activeSpecie, tree.h, tree.t, out factor, out prefab);

                int protoIndex = System.Array.FindIndex(prototypes, p => p.prefab == prefab);
                if (protoIndex < 0) continue;

                TreeInstance ti = new TreeInstance
                {
                    position = new Vector3(normX, 0, normZ),
                    prototypeIndex = protoIndex,
                    widthScale = factor,
                    heightScale = factor,
                    color = Color.white,
                    lightmapColor = Color.white,
                    rotation = tree.rotation
                };
                treeInstancesTerrain.Add(ti);
            }
        }

        terrain.terrainData.treeInstances = treeInstancesTerrain.ToArray();
    }

    private void GetFactorAndPrefabSpecie(string specie, float height, float age, out float factor, out GameObject prefab)
    {
        factor = 0f;
        prefab = null;

        if (specie.Equals(species[0]))
        {
            prefab = getPBPrefabForCurrentAge(age);
            factor = calculatePBFactor(height, age, prefab);
        }
        else if (specie.Equals(species[1]))
        {
            prefab = getPmPrefabForCurrentHeight(age);
            factor = calculatePMFactor(height, age, prefab);
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

    private float calculatePBFactor(float currentHeight, float currentAge, GameObject prefab)
    {
        if (pbPrefabs[0] == prefab)
        {
            return currentHeight / thresholdPbYoungHeight;
        }
        else if (pbPrefabs[1] == prefab || pbPrefabs[2] == prefab || pbPrefabs[3] == prefab)
        {
            return currentHeight / thresholdPbAdultHeight;
        }
        else
        {
            return currentHeight / thresholdPbSeniourHeight;
        }
    }

    private float calculatePMFactor(float currentHeight, float currentAge, GameObject prefab)
    {
        if (pmPrefabs[0] == prefab || pmPrefabs[1] == prefab)
        {
            return currentHeight / thresholdPmYoungHeight;
        }
        else if (pmPrefabs[2] == prefab)
        {
            return currentHeight / thresholdPmAdultHeight;
        }
        else
        {
            return currentHeight / thresholdPmSeniourHeight;
        }
    }

    //creates empty objects that act as an hitbox for each tree so when clicking on a tree displays tree data
    public void createObjects(List<TreeData> trees, Terrain terrain, bool isPlot2)
    {
        if (trees == null || trees.Count == 0) return;

        //if the origin of the plot is in the middle this is useful
        //float xOffset = trees.Average(tree => tree.Xarv);
        //float yOffset = trees.Average(tree => tree.Yarv);

        foreach (TreeData tree in trees)
        {
            if (tree.estado == 4 || tree.estado == 6) continue;

            float adjustedX = (tree.Xarv) * 3f;
            float adjustedZ = (tree.Yarv) * 3f;

            float worldX = terrain.transform.position.x + adjustedX;
            float worldZ = terrain.transform.position.z + adjustedZ;
            float treeHeight = terrain.transform.position.y;

            Vector3 position = new Vector3(worldX, treeHeight, worldZ);

            GameObject marker = new GameObject($"TreeMarker_{tree.id_arv}");
            marker.transform.position = position;
            marker.transform.rotation = Quaternion.Euler(0, tree.rotation, 0);
            marker.transform.SetParent(plot.transform);

            BoxCollider col = marker.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(tree.cw * 2, tree.h, tree.cw * 2);
            col.center = new Vector3(0, treeHeight, 0);

            marker.AddComponent<Tree>().initTree(tree);
            marker.layer = isPlot2 ? LayerMask.NameToLayer("Plot2") : LayerMask.NameToLayer("Plot1");
        }
    }

    private void clear(string layerName = null)
    {
        if (plot != null)
        {
            foreach (Transform child in plot.transform)
            {
                if (string.IsNullOrEmpty(layerName) || child.gameObject.layer == LayerMask.NameToLayer(layerName))
                    Destroy(child.gameObject);
            }
        }
    }
}

