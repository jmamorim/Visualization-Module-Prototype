using UnityEngine;

public class TreeData
{
    public int id_presc, ciclo, Year, id_arv, estado;
    public float t, Xarv, Yarv, d, h, cw, rotation;
    public bool wasAlive;

    public TreeData(int id_presc, int ciclo, int Year, float t, int id_arv, float Xarv, float Yarv, float d, float h, float cw, int estado, float rotation, bool wasAlive)
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
        this.wasAlive = wasAlive;
    }
}
