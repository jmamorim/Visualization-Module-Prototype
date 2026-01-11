public class TreeData
{
    public int ciclo, Year, id_arv, estado;
    public float t, Xarv, Yarv, d, h, cw, rotation, hbc;
    public string id_stand, id_presc, specie;
    public bool wasAlive;

    public TreeData(string id_stand, string id_presc, int ciclo, int Year, float t, int id_arv, float Xarv, float Yarv, string specie, float d, float h, float cw, float hbc, int estado, float rotation, bool wasAlive)
    {
        this.id_stand = id_stand;
        this.id_presc = id_presc;
        this.ciclo = ciclo;
        this.Year = Year;
        this.t = t;
        this.id_arv = id_arv;
        this.Xarv = Xarv;
        this.Yarv = Yarv;
        this.specie = specie;
        this.d = d;
        this.h = h;
        this.cw = cw;
        this.hbc = hbc;
        this.estado = estado;
        this.rotation = rotation;
        this.wasAlive = wasAlive;
    }
}
