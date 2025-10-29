using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YieldTableEntry
{
    public int year, Nst, N, Ndead;
    public float hdom, G, dg, Vu_st, V, Vu_as1, Vu_as2, Vu_as3, Vu_as4, Vu_as5, maiV, iV, Ww, Wb, Wbr, Wl, Wa, Wr, NPVsum, EEA;
    
    public YieldTableEntry(
        int year,
        int Nst,
        int N,
        int Ndead,
        float hdom,
        float G,
        float dg,
        float Vu_st,
        float V,
        float Vu_as1,
        float Vu_as2,
        float Vu_as3,
        float Vu_as4,
        float Vu_as5,
        float maiV,
        float iV,
        float Ww,
        float Wb,
        float Wbr,
        float Wl,
        float Wa,
        float Wr,
        float NPVsum,
        float EEA)
    {
        this.year = year;
        this.Nst = Nst;
        this.N = N;
        this.Ndead = Ndead;
        this.hdom = hdom;
        this.G = G;
        this.dg = dg;
        this.Vu_st = Vu_st;
        this.V = V;
        this.Vu_as1 = Vu_as1;
        this.Vu_as2 = Vu_as2;
        this.Vu_as3 = Vu_as3;
        this.Vu_as4 = Vu_as4;
        this.Vu_as5 = Vu_as5;
        this.maiV = maiV;
        this.iV = iV;
        this.Ww = Ww;
        this.Wb = Wb;
        this.Wbr = Wbr;
        this.Wl = Wl;
        this.Wa = Wa;
        this.Wr = Wr;
        this.NPVsum = NPVsum;
        this.EEA = EEA;
    }
}
