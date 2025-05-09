using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Visualizer : MonoBehaviour
{
    public GameObject treePrefab; // Must have children: "trunk" and "leafs"
    public GameObject plot;
    public TMP_Text yearText;
    public Material trunkMaterial;
    public Material leafsMaterial;

    private int currentYear;
    private List<Tree> trees;

    private Mesh trunkMesh;
    private Mesh leafsMesh;

    private Matrix4x4[] trunkMatrices;
    private Matrix4x4[] leafsMatrices;
    public void Start()
    {
        var temp = Instantiate(treePrefab);
        trunkMesh = temp.transform.Find("Trunk").GetComponent<MeshFilter>().sharedMesh;
        leafsMesh = temp.transform.Find("Leafs").GetComponent<MeshFilter>().sharedMesh;
        Destroy(temp);
    }

    private void Update()
    {
        displayTrees();
    }

    public void receiveTreeData(SortedDictionary<int, Tree> data, int currentYear)
    {
        trees = data.Values.ToList();
        this.currentYear = currentYear;
    }

    public void displayTrees()
    {
        yearText.text = $"Year: {currentYear}";

        if (trees == null || trees.Count == 0) return;

        float xOffset = trees.Average(tree => tree.Xarv);
        float yOffset = trees.Average(tree => tree.Yarv);

        trunkMatrices = new Matrix4x4[trees.Count];
        leafsMatrices = new Matrix4x4[trees.Count];

        int count = 0;
        foreach (Tree tree in trees)
        {
            if (tree.estado == 4 || tree.estado == 6) continue;

            float adjustedX = (tree.Xarv - xOffset) * 3f;
            float adjustedZ = (tree.Yarv - yOffset) * 3f;
            float treeHeight = tree.h * 0.25f;
            float trunkRadius = tree.d * 0.1f;

            Vector3 basePosition = new Vector3(adjustedX, trunkMesh.bounds.size.y/2 * treeHeight, adjustedZ);

            Quaternion rotation = Quaternion.Euler(0, tree.rotation, 0);
            Vector3 trunkScale = new Vector3(trunkRadius, treeHeight, trunkRadius);
            trunkMatrices[count] = Matrix4x4.TRS(basePosition, rotation, trunkScale);

            float crownWidth = tree.cw;
            Vector3 leafScale = new Vector3(crownWidth, treeHeight, crownWidth);
            Vector3 leafOffset = new Vector3(0, trunkMesh.bounds.size.y * treeHeight, 0);
            leafsMatrices[count] = Matrix4x4.TRS(basePosition + leafOffset, rotation, leafScale);

            count++;
        }

        // Batch draw instanced meshes (limit 1023 per batch)
        for (int i = 0; i < trees.Count; i += 1023)
        {
            int batchSize = Mathf.Min(1023, trees.Count - i);
            Graphics.DrawMeshInstanced(trunkMesh, 0, trunkMaterial, trunkMatrices, batchSize);
            Graphics.DrawMeshInstanced(leafsMesh, 0, leafsMaterial, leafsMatrices, batchSize);
        }
    }
}

