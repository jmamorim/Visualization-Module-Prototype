using TMPro;
using UnityEngine;

public class Tree : MonoBehaviour
{
    public GameObject trunk, leafs;
    public int id_presc, ciclo, Year, id_arv, estado;
    public float t, Xarv, Yarv, d, h, cw, rotation;

    private Manager manager;

    private void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<Manager>();
    }

    public Tree(int id_presc, int ciclo, int Year, float t, int id_arv, float Xarv, float Yarv, float d, float h, float cw, int estado, float rotation)
    {
        this.id_presc = id_presc;
        this.ciclo = ciclo;
        this.Year = Year;
        this.t = t;
        this.id_arv = id_arv;
        this.Xarv = Xarv;
        this.Yarv = Yarv;
        this.d = d;
        this.h = h;
        this.cw = cw;
        this.estado = estado;
        this.rotation = rotation;
    }

    public void initTree(Tree tree)
    {
        this.id_presc = tree.id_presc;
        this.ciclo = tree.ciclo;
        this.Year = tree.Year;
        this.t = tree.t;
        this.id_arv = tree.id_arv;
        this.Xarv = tree.Xarv;
        this.Yarv = tree.Yarv;
        this.d = tree.d;
        this.h = tree.h;
        this.cw = tree.cw;
        this.estado = tree.estado;
        this.rotation = tree.rotation;
    }

    public void OnMouseDown()
    {
       manager.ShowTreeInfo(this);
    }


}
