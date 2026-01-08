using System.Collections.Generic;
using UnityEngine;
using CustomUI;
using UnityEngine.UI;

public class GraphMultiSelectManager : MonoBehaviour
{
    public List<GameObject> allGraphs;
    public DropdownEx graphDropdown;

    public RectTransform contentRoot;

    private List<bool> selectedGraphs;

    private void Awake()
    {
        selectedGraphs = new List<bool>(new bool[allGraphs.Count]);
    }

    private void OnEnable()
    {
        if (graphDropdown != null)
        {
            graphDropdown.onItemSelected.AddListener(OnItemSelected);
            graphDropdown.onItemDeselected.AddListener(OnItemDeselected);
        }

        RefreshGraphs();
    }

    private void OnDisable()
    {
        if (graphDropdown != null)
        {
            graphDropdown.onItemSelected.RemoveListener(OnItemSelected);
            graphDropdown.onItemDeselected.RemoveListener(OnItemDeselected);
        }
    }

    private void OnItemSelected(uint index)
    {
        int i = (int)index;
        if (!IsValidIndex(i)) return;

        selectedGraphs[i] = true;
        RefreshGraphs();
    }

    private void OnItemDeselected(uint index)
    {
        int i = (int)index;
        if (!IsValidIndex(i)) return;

        selectedGraphs[i] = false;
        RefreshGraphs();
    }

    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < selectedGraphs.Count;
    }

    private void RefreshGraphs()
    {
        for (int i = 0; i < allGraphs.Count; i++)
        {
            allGraphs[i].SetActive(selectedGraphs[i]);
        }

        if (contentRoot != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
        }
    }
}
