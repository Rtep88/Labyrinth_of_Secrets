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

        //Vypocita potrebnou rotaci primnky AB okolo originu v radianech tak, aby se ji bod C co nejvice priblizoval
        public static float VypocitejRotaci(Vector2 origin, Vector2 bodA, Vector2 bodB, Vector2 bodC)
        {
            float idealniRotace = (float)Math.Atan2(bodC.Y - origin.Y, bodC.X - origin.X);
            float krok = (float)Math.PI / 4f;

            bodA = RotaceBodu(bodA, origin, idealniRotace);
            bodB = RotaceBodu(bodB, origin, idealniRotace);

            //Binarni vyhledavani rotace
            while (krok > float.Epsilon)
            {
                float vzdalenost1 = VypocitejVzdalenostBoduOdPrimky(RotaceBodu(bodA, origin, -krok), RotaceBodu(bodB, origin, -krok), bodC);
                float vzdalenost2 = VypocitejVzdalenostBoduOdPrimky(RotaceBodu(bodA, origin, krok), RotaceBodu(bodB, origin, krok), bodC);
                float vzdalenost3 = VypocitejVzdalenostBoduOdPrimky(RotaceBodu(bodA, origin, 0), RotaceBodu(bodB, origin, 0), bodC);

                if (Math.Min(vzdalenost1, Math.Min(vzdalenost2, vzdalenost3)) == vzdalenost1)
                {
                    idealniRotace -= krok;
                    bodA = RotaceBodu(bodA, origin, -krok);
                    bodB = RotaceBodu(bodB, origin, -krok);
                }
                else if (Math.Min(vzdalenost1, Math.Min(vzdalenost2, vzdalenost3)) == vzdalenost2)
                {
                    idealniRotace += krok;
                    bodA = RotaceBodu(bodA, origin, krok);
                    bodB = RotaceBodu(bodB, origin, krok);
                }
                krok /= 2f;
            }

            return idealniRotace;
        }

        //Vrati vzdalenost bodu C od primky AB
        public static float VypocitejVzdalenostBoduOdPrimky(Vector2 bodA, Vector2 bodB, Vector2 bodC)
        {
            double A = bodB.Y - bodA.Y;
            double B = bodA.X - bodB.X;
            double C = bodB.X * bodA.Y - bodA.X * bodB.Y;

            return (float)(Math.Abs(A * bodC.X + B * bodC.Y + C) / Math.Sqrt(A * A + B * B));
        }

        //Otoci bod kolem originu o urcity pocet radianu
        public static Vector2 RotaceBodu(Vector2 bodA, Vector2 origin, float rotace)
        {
            Vector2 posunutyBod = bodA - origin;
            Vector2 otocenyBod = new Vector2(
                posunutyBod.X * (float)Math.Cos(rotace) - posunutyBod.Y * (float)Math.Sin(rotace),
                posunutyBod.X * (float)Math.Sin(rotace) + posunutyBod.Y * (float)Math.Cos(rotace)
            );

            return otocenyBod + origin;
        }
    }
}