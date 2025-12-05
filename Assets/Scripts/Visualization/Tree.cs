using TMPro;
using UnityEngine;

public class Tree : MonoBehaviour
{
    public int ciclo, Year, id_arv, estado;
    public float t, Xarv, Yarv, d, h, cw, rotation, hbc;
    public string id_stand, id_presc, specie;
    public bool wasAlive;

    private Manager manager;

    private void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<Manager>();
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

    private void OnMouseDown()
    {
        Vector3 mousePos = Input.mousePosition;

        foreach (Camera cam in Camera.allCameras)
        {
            Vector2 normalizedMouse = new Vector2(mousePos.x / Screen.width, mousePos.y / Screen.height);

            if (cam.rect.Contains(normalizedMouse))
            {
                if ((cam.cullingMask & (1 << gameObject.layer)) != 0)
                {
                    CameraBehaviour behaviour = cam.GetComponent<CameraBehaviour>();
                    if (behaviour != null && behaviour.CanRotate())
                    {
                        manager.ShowTreeInfo(this);
                        behaviour.ChangeLookAt(transform);

                        var lastSelectedTree = manager.GetSelectedTree();
                        if (lastSelectedTree != null)
                            lastSelectedTree.transform.Find("OutlineMesh").gameObject.SetActive(false);

                        manager.SelectTree(gameObject);
                        gameObject.transform.Find("OutlineMesh").gameObject.SetActive(true);
                    }
                    return;
                }
            }
        }
    }
}
