using System;
using System.Collections.Generic;

namespace Labyrinth_of_Secrets
{
    public class Hra
    {
        //Promenne
        public Random rnd = new Random();
        public KomponentaMapa komponentaMapa;
        public KomponentaZbrane komponentaZbrane;
        public KomponentaSvetlo komponentaSvetlo;
        public KomponentaMonstra komponentaMonstra;
        public KomponentaMultiplayer komponentaMultiplayer;

        //Konstanty
        public const int VELIKOST_HRACE_X = 6; //Velikost vykresleni hrace
        public const int VELIKOST_HRACE_Y = 12; //Velikost vykresleni hrace

        public Hra()
        {
            komponentaMapa = new KomponentaMapa(this);
            komponentaZbrane = new KomponentaZbrane(this);
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

        public static float NaRadiany(float angle)
        {
            return (float)(Math.PI * angle / 180.0);
        }

        public static Vector2 OtocVector2(Vector2 vector, float angle)
        {
            float sin = (float)Math.Sin(angle);
            float cos = (float)Math.Cos(angle);

            float tx = vector.X;
            float ty = vector.Y;

            vector.X = cos * tx - sin * ty;
            vector.Y = sin * tx + cos * ty;

            return vector;
        }

    }
}