using UnityEngine;

public class Tree : MonoBehaviour
{
    public int ciclo, Year, id_arv, estado;
    public float t, Xarv, Yarv, d, h, cw, rotation, hbc;
    public string id_stand, id_presc, specie;
    public bool wasAlive;

    private Manager manager;
    private MeshRenderer[] renderers;
    private Color[] originalColors;
    private Material[] originalMaterials;

    private void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        CacheRenderers();
    }

    private void CacheRenderers()
    {
        renderers = GetComponentsInChildren<MeshRenderer>();
        originalColors = new Color[renderers.Length];
        originalMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material != null)
            {
                originalMaterials[i] = renderers[i].material;
                originalColors[i] = renderers[i].material.color;
            }
        }
    }

    public void initTree(TreeData tree)
    {
        this.id_stand = tree.id_stand;
        this.id_presc = tree.id_presc;
        this.ciclo = tree.ciclo;
        this.Year = tree.Year;
        this.t = tree.t;
        this.id_arv = tree.id_arv;
        this.Xarv = tree.Xarv;
        this.Yarv = tree.Yarv;
        this.specie = tree.specie;
        this.d = tree.d;
        this.h = tree.h;
        this.cw = tree.cw;
        this.hbc = tree.hbc;
        this.estado = tree.estado;
        this.rotation = tree.rotation;
        this.wasAlive = tree.wasAlive;
    }

    public void SetFocusMode(bool isFocused)
    {
        if (renderers == null || renderers.Length == 0)
            CacheRenderers();

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null)
            {
                if (isFocused)
                {
                    renderers[i].material.color = originalColors[i];
                }
                else
                {
                    Color dimmedColor = originalColors[i];
                    dimmedColor.r *= 0.3f;
                    dimmedColor.g *= 0.3f;
                    dimmedColor.b *= 0.3f;
                    dimmedColor.a *= 0.6f;
                    renderers[i].material.color = dimmedColor;
                }
            }
        }
    }

    public void ResetFocusMode()
    {
        if (renderers == null || renderers.Length == 0)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null)
            {
                renderers[i].material.color = originalColors[i];
            }
        }
    }

    private void OnMouseOver()
    {
        if (manager.canInteract)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                bool isRightClick = Input.GetMouseButtonDown(1);
                Vector3 mousePos = Input.mousePosition;
                foreach (Camera cam in Camera.allCameras)
                {
                    Vector2 normalizedMouse = new Vector2(mousePos.x / Screen.width, mousePos.y / Screen.height);
                    if (cam.rect.Contains(normalizedMouse))
                    {
                        if ((cam.cullingMask & (1 << gameObject.layer)) != 0)
                        {
                            CameraBehaviour behaviour = cam.GetComponent<CameraBehaviour>();
                            if (behaviour != null && behaviour.CanMoveCamera())
                            {
                                if (isRightClick)
                                {
                                    behaviour.ChangeLookAt(transform);
                                }
                                else
                                {
                                    manager.ShowTreeInfo(this);
                                    var lastSelectedTree = manager.GetSelectedTree();
                                    if (lastSelectedTree != null)
                                        lastSelectedTree.transform.Find("OutlineMesh").gameObject.SetActive(false);
                                    manager.SelectTree(gameObject);
                                    gameObject.transform.Find("OutlineMesh").gameObject.SetActive(true);
                                }
                            }
                            return;
                        }
                    }
                }
            }
        }
    }
}