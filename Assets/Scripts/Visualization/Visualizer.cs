using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.ProBuilder;

// This class is responsible for visualizing the trees in the scene based on the parsed data
public class Visualizer : MonoBehaviour
{
    [Header("Pinheiro-bravo Prefabs")]
    public List<GameObject> pbPrefabs;
    [Header("Pinheiro-manso Prefabs")]
    public List<GameObject> pmPrefabs;
    [Header("Eucalipto Prefabs")]
    public List<GameObject> egPrefabs;
    [Header("Castanheiro Prefabs")]
    public List<GameObject> casPrefabs;
    public InputAndParsedData inputAndParsedData;

    [Header("Scene References")]

    public Camera cam1, cam2;
    public GameObject plot, plotReference1, plotReference2, paralelPos1, paralelPos2, box1, box2;
    public TMP_Text yearText1, yearText2, idStand1, idStand2;
    public Terrain terrain1;
    public Terrain terrain2;
    public Manager manager;

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
    [SerializeField] float pmSeniourStartingAge;

    [Space(5)]
    [Header("Pinheiro-manso Height Thresholds")]
    [SerializeField] float thresholdPmYoungHeight;
    [SerializeField] float thresholdPmAdultHeight;
    [SerializeField] float thresholdPmSeniourHeight;

    [Header("Eucalipto Parameters"), Tooltip("Ages at which Eucalipto transitions between stages")]
    [SerializeField] float egAdultStartingAge;
    [SerializeField] float egSeniourStartingAge;

    [Space(5)]
    [Header("Eucalipto Height Thresholds")]
    [SerializeField] float thresholdEgYoungHeight;
    [SerializeField] float thresholdEgAdultHeight;
    [SerializeField] float thresholdEgSeniourHeight;

    [Header("Castanheiro Parameters"), Tooltip("Ages at which Castanheiro transitions between stages")]
    [SerializeField] float casAdultStartingAge;
    [SerializeField] float casSeniourStartingAge;

    [Space(5)]
    [Header("Castanheiro Height Thresholds")]
    [SerializeField] float thresholdCasYoungHeight;
    [SerializeField] float thresholdCasAdultHeight;
    [SerializeField] float thresholdCasSeniourHeight;

    int currentYear;
    List<(int, List<float>)> plotShapeAndDimensions;
    readonly List<string> species = new List<string> { "Pb", "Pm", "Ec", "Ct" };
    int offsetToPbCa = 5, offsetToPmCa = 4, offsetToEgCa = 3, offsetToCasCa = 3;
    bool isCircularPlot1 = false;
    bool isCircularPlot2 = false;
    //used when plotshape is 0 (area specific)
    float terrainOffset = 10f;

    [SerializeField] float distanceScalingFactor = 0.6f;
    const float perspectiveAngleFactor = 0.7f;
    CameraBehaviour behaviour1, behaviour2;

    #region unity methods
    private void Start()
    {
        behaviour1 = cam1.GetComponent<CameraBehaviour>();
        behaviour2 = cam2.GetComponent<CameraBehaviour>();

        plotShapeAndDimensions = inputAndParsedData.plotShapeAndDimensions;

        // Reset terrains to avoid modifying original terrain data so unity doesnt serialize changes
        terrain1.terrainData = Instantiate(terrain1.terrainData);
        terrain2.terrainData = Instantiate(terrain2.terrainData);
    }
    #endregion

    #region Flyover methods
    //Lidar flyover simulation
    public void StartLidarFlyover(int plotNumber)
    {
        LidarFlyover lidar1 = cam1.gameObject.GetComponent<LidarFlyover>();
        LidarFlyover lidar2 = cam2.gameObject.GetComponent<LidarFlyover>();
        if (lidar1.isCurrentlyFlying())
        {
            lidar1.StopFlyover();
            return;
        }
        else if (lidar2.isCurrentlyFlying())
        {
            lidar2.StopFlyover();
            return;
        }

        Terrain targetTerrain = plotNumber == 1 ? terrain1 : terrain2;
        LidarFlyover lidarFlyover = plotNumber == 1 ? lidar1 : lidar2;
        Camera lidarCamera = plotNumber == 1 ? cam1 : cam2;

        Vector3 terrainPos = targetTerrain.transform.position;
        Vector3 terrainSize = targetTerrain.terrainData.size;

        lidarCamera.transform.position = new Vector3(
            terrainPos.x,
            terrainPos.y + 50f,
            terrainPos.z
        );

        lidarFlyover.StartFlyover(targetTerrain);
    }

    //Orbital flyover
    public void StartOrbitalLidarFlyover(int plotNumber, float duration = 30f)
    {
        LidarFlyover lidar1 = cam1.gameObject.GetComponent<LidarFlyover>();
        LidarFlyover lidar2 = cam2.gameObject.GetComponent<LidarFlyover>();
        if (lidar1.isCurrentlyFlying())
        {
            lidar1.StopFlyover();
            return;
        }
        else if (lidar2.isCurrentlyFlying())
        {
            lidar2.StopFlyover();
            return;
        }

        Terrain targetTerrain = plotNumber == 1 ? terrain1 : terrain2;
        LidarFlyover lidarFlyover = plotNumber == 1 ? lidar1 : lidar2;
        Vector3 terrainSize = targetTerrain.terrainData.size;
        float radius = Mathf.Max(terrainSize.x, terrainSize.z) * 0.6f;

        lidarFlyover.StartOrbitalFlyover(targetTerrain, radius, duration);
    }
    #endregion

    #region terrain and camera configuration
    public void ConfigureTerrains()
    {
        var plot1Shape = plotShapeAndDimensions[0];
        int shapeType1 = plot1Shape.Item1;
        List<float> dims1 = plot1Shape.Item2;
        if (dims1 != null && dims1.Count > 0)
        {
            ConfigureTerrainOrigin(terrain1, shapeType1, dims1, out isCircularPlot1, plotReference1, cam1);
        }

        if (plotShapeAndDimensions.Count > 1)
        {
            var plot2Shape = plotShapeAndDimensions[1];
            int shapeType2 = plot2Shape.Item1;
            List<float> dims2 = plot2Shape.Item2;
            if (dims2 != null && dims2.Count > 0)
            {
                ConfigureTerrainOrigin(terrain2, shapeType2, dims2, out isCircularPlot2, plotReference2, cam2);
            }
        }
        else
        {
            ConfigureTerrainOrigin(terrain2, shapeType1, dims1, out isCircularPlot2, plotReference2, cam2);
        }
            GenerateNoise();
    }
    private void GenerateNoise()
    {
        float[,] heights1 = terrain1.terrainData.GetHeights(0, 0, terrain1.terrainData.heightmapResolution, terrain1.terrainData.heightmapResolution);
        for (int x = 0; x < terrain1.terrainData.heightmapResolution; x++)
        {
            for (int y = 0; y < terrain1.terrainData.heightmapResolution; y++)
            {
                heights1[x, y] = Mathf.PerlinNoise((float)x / terrain1.terrainData.heightmapResolution * 2f, (float)y / terrain1.terrainData.heightmapResolution * 2f) * 0.01f;
            }
        }
        terrain1.terrainData.SetHeights(0, 0, heights1);

        float[,] heights2 = terrain2.terrainData.GetHeights(0, 0, terrain2.terrainData.heightmapResolution, terrain2.terrainData.heightmapResolution);
        for (int x = 0; x < terrain2.terrainData.heightmapResolution; x++)
        {
            for (int y = 0; y < terrain2.terrainData.heightmapResolution; y++)
            {
                heights2[x, y] = Mathf.PerlinNoise((float)x / terrain2.terrainData.heightmapResolution * 2f, (float)y / terrain2.terrainData.heightmapResolution * 2f) * 0.01f;
            }
        }
        terrain2.terrainData.SetHeights(0, 0, heights2);
    }

    private void ConfigureTerrainOrigin(Terrain terrain, int shapeType, List<float> dims, out bool isCircular, GameObject plotReference, Camera cam)
    {
        Vector3 currentSize = terrain.terrainData.size;
        Vector3 newPosition = terrain.transform.position;
        Vector3 newSize = currentSize;
        isCircular = false;
        var paralelPos = cam.GetComponent<CameraBehaviour>().paralelPos;

        if (shapeType == 0) //Area specific its here just for safety but should not be used because plot shape should be always defined
        {
            var area = dims[0];
            var sideLength = Mathf.Sqrt(area) + terrainOffset;
            newPosition = new Vector3(0, terrain.transform.position.y, 0);
            newSize = new Vector3(sideLength, currentSize.y, sideLength);
            plotReference.transform.position = new Vector3(sideLength / 2, terrain.transform.position.y, sideLength / 2);
            paralelPos.transform.position = new Vector3(sideLength / 2, paralelPos.transform.position.y, sideLength / 2);
        }
        else if (shapeType == 1 && dims.Count >= 1) // Square
        {
            newPosition = new Vector3(0, terrain.transform.position.y, 0);
            newSize = new Vector3(dims[0], currentSize.y, dims[0]);
            plotReference.transform.position = new Vector3(dims[0] / 2, terrain.transform.position.y, dims[0] / 2);
            paralelPos.transform.position = new Vector3(dims[0] / 2, paralelPos.transform.position.y, dims[0] / 2);
        }
        else if (shapeType == 2) // Rectangular
        {
            newPosition = new Vector3(0, terrain.transform.position.y, 0);
            newSize = new Vector3(dims[0], currentSize.y, dims[1]);
            plotReference.transform.position = new Vector3(dims[0] / 2, terrain.transform.position.y, dims[1] / 2);
            paralelPos.transform.position = new Vector3(dims[0] / 2, paralelPos.transform.position.y, dims[1] / 2);
        }
        else if (shapeType == 3 && dims.Count >= 1) // Circular
        {
            float diameter = dims[0] * 2f;
            newPosition = new Vector3(0, terrain.transform.position.y, 0);
            newSize = new Vector3(diameter, currentSize.y, diameter);
            plotReference.transform.position = new Vector3(dims[0], terrain.transform.position.y, dims[0]);
            paralelPos.transform.position = new Vector3(dims[0], paralelPos.transform.position.y, dims[0]);
            isCircular = true;
        }
        else if (shapeType == 4 && dims.Count >= 2)// Custom
        {
            var minX = dims[0];
            var maxX = dims[1];
            var minY = dims[2];
            var maxY = dims[3];

            var lengthX = maxX - minX;
            var lengthY = maxY - minY;

            var centerX = (minX + maxX) / 2f;
            var centerY = (minY + maxY) / 2f;

            newPosition = new Vector3(0, terrain.transform.position.y, 0);
            newSize = new Vector3(lengthX, currentSize.y, lengthY);

            plotReference.transform.position = new Vector3(centerX, terrain.transform.position.y, centerY);
            paralelPos.transform.position = new Vector3(centerX, paralelPos.transform.position.y, centerY);
            terrain.transform.position = new Vector3(minX, terrain.transform.position.y, minY);
        }

        PositionCamera(cam, plotReference.transform, newSize);
        terrain.terrainData.size = newSize;
    }

    private void PositionCamera(Camera cam, Transform targetPosition, Vector3 terrainSize)
    {
        float diagonalSize = Mathf.Sqrt(terrainSize.x * terrainSize.x + terrainSize.z * terrainSize.z);
        var behaviour = cam.GetComponent<CameraBehaviour>();

        Vector3 perspectivePos;
        Quaternion perspectiveRot;
        Vector3 orthographicPos;
        Quaternion orthographicRot;

        float fov = cam.fieldOfView * Mathf.Deg2Rad;
        float distance = (diagonalSize * distanceScalingFactor) / Mathf.Tan(fov / 2f);
        float height = distance * perspectiveAngleFactor;
        float horizontalDistance = distance * perspectiveAngleFactor;
        Vector3 cameraOffset = new Vector3(0, height, -horizontalDistance);
        perspectivePos = targetPosition.position + cameraOffset;
        perspectiveRot = Quaternion.LookRotation(targetPosition.position - perspectivePos);

        float orthographicHeight = terrainSize.y + diagonalSize;
        orthographicPos = new Vector3(targetPosition.position.x, orthographicHeight, targetPosition.position.z);
        orthographicRot = Quaternion.Euler(90f, 0f, 0f);

        float maxDimension = Mathf.Max(terrainSize.x, terrainSize.z);
        behaviour.SetOrthographicSize(maxDimension / cam.aspect);

        cam.transform.position = perspectivePos;
        cam.transform.rotation = perspectiveRot;

        behaviour.InitializeCamera(
            perspectivePos,
            perspectiveRot,
            targetPosition,
            orthographicPos,
            orthographicRot
        );
    }
    #endregion

    #region data reception and 3D visualization of trees
    public void receiveTreeDataPlot1(SortedDictionary<int, TreeData> data, int currentYear)
    {
        clear("Plot1");
        var trees = data.Values.ToList();
        createObjects(trees, terrain1, false, isCircularPlot1);
        yearText1.text = "Ano: " + currentYear.ToString();
        idStand1.text = "Povoamento:" + trees[0].id_stand;
        box1.SetActive(true);
        displayTrees(trees, terrain1, isCircularPlot1);
    }

    public void receiveTreeDataPlot2(SortedDictionary<int, TreeData> data, int currentYear, bool isMulti)
    {
        clear("Plot2");
        var trees = data.Values.ToList();
        createObjects(trees, terrain2, true, isCircularPlot2);
        if (isMulti) {
            yearText2.text = "Ano: " + currentYear.ToString();
            idStand2.text = "Povoamento:" + trees[0].id_stand;
            box2.SetActive(true);
        }
        displayTrees(trees, terrain2, isCircularPlot2);
    }

    public void displayTrees(List<TreeData> trees, Terrain terrain, bool isCircular)
    {
        if (trees == null || trees.Count == 0) return;

        terrain.terrainData.treeInstances = new TreeInstance[0];
        TreePrototype[] prototypes = new TreePrototype[pbPrefabs.Count + pmPrefabs.Count + egPrefabs.Count + casPrefabs.Count];
        for (int i = 0; i < pbPrefabs.Count; i++)
        {
            prototypes[i] = new TreePrototype { prefab = pbPrefabs[i] };
        }
        for (int i = 0; i < pmPrefabs.Count; i++)
        {
            prototypes[pbPrefabs.Count + i] = new TreePrototype { prefab = pmPrefabs[i] };
        }
        for (int i = 0; i < egPrefabs.Count; i++)
        {
            prototypes[pbPrefabs.Count + pmPrefabs.Count + i] = new TreePrototype { prefab = egPrefabs[i] };
        }
        for (int i = 0; i < casPrefabs.Count; i++)
        {
            prototypes[pbPrefabs.Count + pmPrefabs.Count + egPrefabs.Count + i] = new TreePrototype { prefab = casPrefabs[i] };
        }
        terrain.terrainData.treePrototypes = prototypes;

        List<TreeInstance> treeInstancesTerrain = new List<TreeInstance>();

        Vector3 terrainCenter = terrain.transform.position + terrain.terrainData.size / 2f;

        foreach (TreeData tree in trees)
        {
            if (tree.estado == 0)
            {
                float worldX = tree.Xarv;
                float worldZ = tree.Yarv;

                float normX, normZ;

                if (isCircular)
                {
                    // Origin is the center of the terrain
                    normX = (worldX + terrain.terrainData.size.x / 2f) / terrain.terrainData.size.x;
                    normZ = (worldZ + terrain.terrainData.size.z / 2f) / terrain.terrainData.size.z;
                }
                else
                {
                    // Origin is the bottom rigth corner of the terrain
                    normX = (worldX - terrain.transform.position.x) / terrain.terrainData.size.x;
                    normZ = (worldZ - terrain.transform.position.z) / terrain.terrainData.size.z;
                }

                GameObject prefab = null;
                float factor = 0;
                GetFactorAndPrefabSpecie(tree.specie, tree.h, tree.hbc, tree.t, out factor, out prefab);

                int protoIndex = System.Array.FindIndex(prototypes, p => p.prefab == prefab);
                if (protoIndex < 0) continue;
                float terrainHeight = terrain.terrainData.GetInterpolatedHeight(normX, normZ) / terrain.terrainData.size.y;

                TreeInstance ti = new TreeInstance
                {
                    position = new Vector3(normX, terrainHeight, normZ),
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

    //creates empty objects that act as an hitbox for each tree so when clicking on a tree displays tree data
    public void createObjects(List<TreeData> trees, Terrain terrain, bool isPlot2, bool isCircular)
    {
        if (trees == null || trees.Count == 0) return;
        Vector3 terrainCenter = terrain.transform.position + terrain.terrainData.size / 2f;
        foreach (TreeData tree in trees)
        {
            if (tree.estado == 4 || tree.estado == 6) continue;
            float worldX = tree.Xarv;
            float worldZ = tree.Yarv;

            float normX, normZ;
            if (isCircular)
            {
                normX = (worldX + terrain.terrainData.size.x / 2f) / terrain.terrainData.size.x;
                normZ = (worldZ + terrain.terrainData.size.z / 2f) / terrain.terrainData.size.z;
            }
            else
            {
                normX = (worldX - terrain.transform.position.x) / terrain.terrainData.size.x;
                normZ = (worldZ - terrain.transform.position.z) / terrain.terrainData.size.z;
            }
            float terrainHeight = terrain.terrainData.GetInterpolatedHeight(normX, normZ);

            Vector3 position;
            if (isCircular)
                position = new Vector3(terrainCenter.x + worldX, terrainHeight, terrainCenter.z + worldZ);
            else
                position = new Vector3(worldX, terrainHeight, worldZ);

            GameObject marker = new GameObject($"TreeMarker_{tree.id_arv}");
            marker.transform.position = position;
            marker.transform.rotation = Quaternion.Euler(0, tree.rotation, 0);
            marker.transform.SetParent(plot.transform);

            float finalHeight = tree.h;
            float finalWidth = tree.cw;

            BoxCollider col = marker.AddComponent<BoxCollider>();
            col.isTrigger = true;
            //calculations were weird so i did some experimenting
            col.size = new Vector3(finalWidth, finalHeight / 3, finalWidth);
            col.center = new Vector3(0, finalHeight / 6, 0);

            marker.AddComponent<Tree>().initTree(tree);
            marker.layer = isPlot2 ? LayerMask.NameToLayer("Plot2") : LayerMask.NameToLayer("Plot1");
            CreateOutline(marker);
        }
    }

    private void CreateOutline(GameObject marker)
    {
        // Create a child object to hold the wireframe
        GameObject wireframe = new GameObject("OutlineMesh");
        wireframe.layer = marker.layer;
        wireframe.transform.SetParent(marker.transform, false);
        wireframe.transform.localPosition = Vector3.zero;
        wireframe.transform.localRotation = Quaternion.identity;
        MeshFilter mf = wireframe.AddComponent<MeshFilter>();
        MeshRenderer mr = wireframe.AddComponent<MeshRenderer>();
        mr.material = new Material(Shader.Find("Sprites/Default"));
        mr.material.color = Color.green;

        BoxCollider col = marker.GetComponent<BoxCollider>();
        if (col == null) return;

        mf.mesh = GenerateWireCubeMesh(col);
        wireframe.SetActive(false);
    }

    private Mesh GenerateWireCubeMesh(BoxCollider col)
    {
        Bounds b = col.bounds;
        Vector3 center = col.center;
        Vector3 size = col.size;

        Vector3 half = size / 2f;

        // 8 corners in local space
        Vector3 v0 = new Vector3(-half.x, -half.y, -half.z) + center;
        Vector3 v1 = new Vector3(half.x, -half.y, -half.z) + center;
        Vector3 v2 = new Vector3(half.x, -half.y, half.z) + center;
        Vector3 v3 = new Vector3(-half.x, -half.y, half.z) + center;
        Vector3 v4 = new Vector3(-half.x, half.y, -half.z) + center;
        Vector3 v5 = new Vector3(half.x, half.y, -half.z) + center;
        Vector3 v6 = new Vector3(half.x, half.y, half.z) + center;
        Vector3 v7 = new Vector3(-half.x, half.y, half.z) + center;

        // Edges: 12 lines (24 vertices)
        Vector3[] vertices = new Vector3[]
        {
        v0, v1, v1, v2, v2, v3, v3, v0, // bottom
        v4, v5, v5, v6, v6, v7, v7, v4, // top
        v0, v4, v1, v5, v2, v6, v3, v7  // verticals
        };

        // Lines topology
        int[] indices = new int[vertices.Length];
        for (int i = 0; i < vertices.Length; i++) indices[i] = i;

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.SetIndices(indices, MeshTopology.Lines, 0);
        mesh.RecalculateBounds();

        return mesh;
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
    #endregion

    #region tree prefab and factor selection
    private void GetFactorAndPrefabSpecie(string specie, float height, float hbc, float age, out float factor, out GameObject prefab)
    {
        prefab = null;
        factor = 0f;

        if (specie.Equals(species[0]))
        {
            prefab = getPBPrefabForCurrentAge(age, height, hbc);
            factor = calculatePBFactor(height, age, prefab);
        }
        else if (specie.Equals(species[1]))
        {
            prefab = getPMPrefabForCurrentHeight(age, height, hbc);
            factor = calculatePMFactor(height, age, prefab);
        }
        else if (specie.Equals(species[2]))
        {
            prefab = getEGPrefabForCurrentHeight(age, height, hbc);
            factor = calculateEGFactor(height, age, prefab);
        }
        else if (specie.Equals(species[3]))
        {
            prefab = getCASPrefabForCurrentHeight(age, height, hbc);
            factor = calculateCASFactor(height, age, prefab);
        }
    }

    //-----Prefab selection methods-----//
    private GameObject getPBPrefabForCurrentAge(float currentAge, float h, float hbc)
    {
        if (currentAge < pbAdultStartingAge)
            return pbPrefabs[hbc >= h / 2 ? 0 + offsetToPbCa : 0];
        else if (currentAge >= pbAdultStartingAge && currentAge < pbYoungAdultAge)
            return pbPrefabs[hbc >= h / 2 ? 1 + offsetToPbCa : 1];
        else if (currentAge >= pbAdultStartingAge && currentAge < pbMidAdultAge)
            return pbPrefabs[hbc >= h / 2 ? 2 + offsetToPbCa : 2];
        else if (currentAge >= pbAdultStartingAge && currentAge < pbSeniourStartingAge)
            return pbPrefabs[hbc >= h / 2 ? 3 + offsetToPbCa : 3];
        else
            return pbPrefabs[hbc >= h / 2 ? 4 + offsetToPbCa : 4];
    }

    private GameObject getPMPrefabForCurrentHeight(float currentAge, float h, float hbc)
    {
        if (currentAge < pmMidYougAge)
            return pmPrefabs[hbc >= h / 2 ? 0 + offsetToPmCa : 0];
        else if (currentAge >= pmMidYougAge && currentAge < pmAdultStartingAge)
            return pmPrefabs[hbc >= h / 2 ? 1 + offsetToPmCa : 1];
        else if (currentAge >= pmAdultStartingAge && currentAge < pmSeniourStartingAge)
            return pmPrefabs[hbc >= h / 2 ? 2 + offsetToPmCa : 2];
        else
            return pmPrefabs[hbc >= h / 2 ? 3 + offsetToPmCa : 3];
    }

    private GameObject getEGPrefabForCurrentHeight(float currentAge, float h, float hbc)
    {
        if (currentAge < egAdultStartingAge)
            return egPrefabs[hbc >= h / 2 ? 0 + offsetToEgCa : 0];
        else if (currentAge >= egAdultStartingAge && currentAge < egSeniourStartingAge)
            return egPrefabs[hbc >= h / 2 ? 1 + offsetToEgCa : 1];
        else
            return egPrefabs[hbc >= h / 2 ? 2 + offsetToEgCa : 2];
    }

    private GameObject getCASPrefabForCurrentHeight(float currentAge, float h, float hbc)
    {
        if (currentAge < casAdultStartingAge)
            return casPrefabs[hbc >= h / 2 ? 0 + offsetToCasCa : 0];
        else if (currentAge >= casAdultStartingAge && currentAge < casSeniourStartingAge)
            return casPrefabs[hbc >= h / 2 ? 1 + offsetToCasCa : 1];
        else
            return casPrefabs[hbc >= h / 2 ? 2 + offsetToCasCa : 2];
    }

    //-----Scale factor calculation methods-----//
    private float calculatePBFactor(float currentHeight, float currentAge, GameObject prefab)
    {
        if (pbPrefabs[0] == prefab)
            return currentHeight / thresholdPbYoungHeight;
        else if (pbPrefabs[1] == prefab || pbPrefabs[2] == prefab || pbPrefabs[3] == prefab)
            return currentHeight / thresholdPbAdultHeight;
        else
            return currentHeight / thresholdPbSeniourHeight;
    }

    private float calculatePMFactor(float currentHeight, float currentAge, GameObject prefab)
    {
        if (pmPrefabs[0] == prefab || pmPrefabs[1] == prefab)
            return currentHeight / thresholdPmYoungHeight;
        else if (pmPrefabs[2] == prefab)
            return currentHeight / thresholdPmAdultHeight;
        else
            return currentHeight / thresholdPmSeniourHeight;
    }

    private float calculateEGFactor(float currentHeight, float currentAge, GameObject prefab)
    {
        if (egPrefabs[0] == prefab)
            return currentHeight / thresholdEgYoungHeight;
        else if (egPrefabs[1] == prefab)
            return currentHeight / thresholdEgAdultHeight;
        else
            return currentHeight / thresholdEgSeniourHeight;
    }

    private float calculateCASFactor(float currentHeight, float currentAge, GameObject prefab)
    {
        if (casPrefabs[0] == prefab)
            return currentHeight / thresholdCasYoungHeight;
        else if (casPrefabs[1] == prefab)
            return currentHeight / thresholdCasAdultHeight;
        else
            return currentHeight / thresholdCasSeniourHeight;
    }
    #endregion

}