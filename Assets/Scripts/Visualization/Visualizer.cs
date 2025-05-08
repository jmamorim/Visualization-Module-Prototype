using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Visualizer : MonoBehaviour
{
    public GameObject treePrefab;
    public GameObject plot;
    public TMP_Text yearText;

    private List<Tree> trees;

    public void receiveTreeData(SortedDictionary<int, Tree> data)
    {
        trees = data.Values.ToList();
    }

    public void displayTrees(int currentYear)
    {
        yearText.text = $"Year: {currentYear}";

        foreach (Transform child in plot.transform)
        {
            Destroy(child.gameObject);
        }

        float xOffset = trees.Average(tree => tree.Xarv);
        float yOffset = trees.Average(tree => tree.Yarv);

        foreach (Tree tree in trees)
        {
            if (tree.estado != 4 && tree.estado != 6)
            {
                float adjustedX = (tree.Xarv - xOffset) * 3f;
                float adjustedY = (tree.Yarv - yOffset) * 3f;

                GameObject treeObject = Instantiate(
                   treePrefab,
                   new Vector3(adjustedX, 0, adjustedY),
                   Quaternion.Euler(0, tree.rotation, 0),
                   plot.transform
                );

                Tree treeScript = treeObject.GetComponent<Tree>();
                treeScript.initTree(tree);
                treeScript.applyDataToTree();
                //experimental change of the tree size needs to be explored further with the parts of the tree
                //treeObject.transform.localScale = new Vector3((float)(tree.d * 0.25), (float)(tree.h * 0.25), (float)(tree.d * 0.25));
            }
        }
    }
}
