using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    public GameObject trunk, leafs;
    public int id_presc, ciclo, Year, id_arv, estado;
    public float t, Xarv, Yarv, d, h, cw, rotation;

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

    public void applyDataToTree()
    {
        gameObject.transform.localScale = new Vector3(1, h * 0.25f,1);
        trunk.transform.localScale = new Vector3(d * 0.1f, 1, d * 0.1f);
        leafs.transform.localScale = new Vector3(cw , 1, cw);
    }

}
