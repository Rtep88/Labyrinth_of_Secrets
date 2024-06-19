using System;
using System.Collections.Generic;
using System.Text;

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
        public const int RYCHLOST_HRACE = 50;
        public const int MAX_ZIVOTY = 10000;
        public const int RYCHLOST_REGENERACE = 50;

        public Hra()
        {
            komponentaMapa = new KomponentaMapa(this);
            komponentaZbrane = new KomponentaZbrane(this);
            komponentaSvetlo = new KomponentaSvetlo(this);
            komponentaMonstra = new KomponentaMonstra(this);
            komponentaMultiplayer = new KomponentaMultiplayer(this);
        }

        //Kolize obdelniku
        public static bool KolizeObdelniku(float x1, float y1, float width1, float height1, float x2, float y2, float width2, float height2)
        {
            return x1 < x2 + width2 &&
                   x1 + width1 > x2 &&
                   y1 < y2 + height2 &&
                   y1 + height1 > y2;
        }

        public static bool KolizeCarySObdelnikem(Vector2 bod1, Vector2 bod2, Rectangle obdelnik)
        {
            return KolizeCarySCarou(bod1, bod2, new Vector2(obdelnik.X, obdelnik.Y), new Vector2(obdelnik.X + obdelnik.Width, obdelnik.Y)) ||
                   KolizeCarySCarou(bod1, bod2, new Vector2(obdelnik.X + obdelnik.Width, obdelnik.Y), new Vector2(obdelnik.X + obdelnik.Width, obdelnik.Y + obdelnik.Height)) ||
                   KolizeCarySCarou(bod1, bod2, new Vector2(obdelnik.X + obdelnik.Width, obdelnik.Y + obdelnik.Height), new Vector2(obdelnik.X, obdelnik.Y + obdelnik.Height)) ||
                   KolizeCarySCarou(bod1, bod2, new Vector2(obdelnik.X, obdelnik.Y + obdelnik.Height), new Vector2(obdelnik.X, obdelnik.Y)) ||
                   (obdelnik.Contains(bod1) && obdelnik.Contains(bod2));
        }

        private static bool KolizeCarySCarou(Vector2 cara1bod1, Vector2 cara1bod2, Vector2 cara2bod1, Vector2 cara2bod2)
        {
            float q = (cara1bod1.Y - cara2bod1.Y) * (cara2bod2.X - cara2bod1.X) - (cara1bod1.X - cara2bod1.X) * (cara2bod2.Y - cara2bod1.Y);
            float d = (cara1bod2.X - cara1bod1.X) * (cara2bod2.Y - cara2bod1.Y) - (cara1bod2.Y - cara1bod1.Y) * (cara2bod2.X - cara2bod1.X);

            if (d == 0)
            {
                return false;
            }

            float r = q / d;

            q = (cara1bod1.Y - cara2bod1.Y) * (cara1bod2.X - cara1bod1.X) - (cara1bod1.X - cara2bod1.X) * (cara1bod2.Y - cara1bod1.Y);
            float s = q / d;

            if (r < 0 || r > 1 || s < 0 || s > 1)
            {
                return false;
            }

            return true;
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

        //Vrátí podpole podle indexu a délky
        public static T[] SubArray<T>(T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public byte[] PrevedHraceNaBytovePole(Hrac hrac, bool pripsatPozici)
        {
            List<byte> hracVBytech = new List<byte>();

            //Zapsani penez
            hracVBytech.AddRange(BitConverter.GetBytes(hrac.penize));

            byte[] zivotyByty = Encoding.UTF8.GetBytes(komponentaMultiplayer.PrevedFloatNaString(hrac.zivoty));
            byte[] xByty = Encoding.UTF8.GetBytes(komponentaMultiplayer.PrevedFloatNaString(pripsatPozici ? hrac.pozice.X : (-1)));
            byte[] yByty = Encoding.UTF8.GetBytes(komponentaMultiplayer.PrevedFloatNaString(pripsatPozici ? hrac.pozice.Y : (-1)));

            //Zapsani zivotu
            hracVBytech.AddRange(BitConverter.GetBytes(zivotyByty.Length));
            hracVBytech.AddRange(zivotyByty);

            //Zapsani pozice X
            hracVBytech.AddRange(BitConverter.GetBytes(xByty.Length));
            hracVBytech.AddRange(xByty);

            //Zapsani pozice Y
            hracVBytech.AddRange(BitConverter.GetBytes(yByty.Length));
            hracVBytech.AddRange(yByty);

            //Zapsani zbrani
            hracVBytech.AddRange(BitConverter.GetBytes(hrac.zbrane.Count));
            foreach (Zbran zbran in hrac.zbrane)
            {
                hracVBytech.AddRange(BitConverter.GetBytes((int)zbran.typZbrane));
                hracVBytech.Add((byte)zbran.levelZbrane);
            }

            return Encoding.UTF8.GetBytes(Convert.ToBase64String(hracVBytech.ToArray()));
        }

        public void PrevedBytovePoleNaHrace(byte[] bytovePole, Hrac hrac)
        {
            bytovePole = Convert.FromBase64String(Encoding.UTF8.GetString(bytovePole));

            //Nacteni penez
            int i = 0;
            hrac.penize = Math.Max(0, BitConverter.ToInt32(Hra.SubArray(bytovePole, i, 4)));
            i += 4;

            //Nacteni zivotu
            int pocetBytuNaZivoty = BitConverter.ToInt32(Hra.SubArray(bytovePole, i, 4));
            i += 4;
            hrac.zivoty = komponentaMultiplayer.PrevedStringNaFloat(Encoding.UTF8.GetString(Hra.SubArray(bytovePole, i, pocetBytuNaZivoty)));
            i += pocetBytuNaZivoty;

            //Nacteni pozice X
            int pocetBytuNaXPozici = BitConverter.ToInt32(Hra.SubArray(bytovePole, i, 4));
            i += 4;
            hrac.pozice.X = komponentaMultiplayer.PrevedStringNaFloat(Encoding.UTF8.GetString(Hra.SubArray(bytovePole, i, pocetBytuNaXPozici)));
            i += pocetBytuNaXPozici;

            //Nacteni pozice Y
            int pocetBytuNaYPozici = BitConverter.ToInt32(Hra.SubArray(bytovePole, i, 4));
            i += 4;
            hrac.pozice.Y = komponentaMultiplayer.PrevedStringNaFloat(Encoding.UTF8.GetString(Hra.SubArray(bytovePole, i, pocetBytuNaYPozici)));
            i += pocetBytuNaXPozici;

            //Nacteni zbrani
            hrac.zbrane.Clear();
            int pocetZbrani = BitConverter.ToInt32(Hra.SubArray(bytovePole, i, 4));
            i += 4;
            for (int j = 0; j < pocetZbrani; j++)
            {
                Zbran zbran = new Zbran((Zbran.TypZbrane)BitConverter.ToInt32(Hra.SubArray(bytovePole, i, 4)));
                i += 4;
                zbran.levelZbrane = bytovePole[i++];
                hrac.zbrane.Add(zbran);
            }
        }
    }
}