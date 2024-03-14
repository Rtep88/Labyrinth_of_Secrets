using System;
using System.Collections.Generic;

namespace Labyrinth_of_Secrets
{
    public class Hra
    {
        //Promenne
        public Random rnd = new Random();
        public KomponentaMapa komponentaMapa;
        public KomponentaSvetlo komponentaSvetlo;
        public KomponentaMonstra komponentaMonstra;
        public KomponentaMultiplayer komponentaMultiplayer;

        public Hra()
        {
            komponentaMapa = new KomponentaMapa(this);
            komponentaSvetlo = new KomponentaSvetlo(this);
            komponentaMonstra = new KomponentaMonstra(this);
            komponentaMultiplayer = new KomponentaMultiplayer(this);
        }

        //Kolize obdelniku
        public bool KolizeObdelniku(float x1, float y1, float width1, float height1, float x2, float y2, float width2, float height2)
        {
            return x1 < x2 + width2 &&
                   x1 + width1 > x2 &&
                   y1 < y2 + height2 &&
                   y1 + height1 > y2;
        }

        //Nahodne prohazi polozky v listu
        public void PromichejList<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}