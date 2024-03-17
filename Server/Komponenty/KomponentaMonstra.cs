using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Labyrinth_of_Secrets
{
    public class KomponentaMonstra
    {
        //Zaklad
        private Hra hra;

        //Struktury

        //Konstanty
        public static int MAX_POCET_MONSTER = 100;

        //Promenne
        private List<Monstrum> monstra = new List<Monstrum>();

        public KomponentaMonstra(Hra hra)
        {
            this.hra = hra;
        }

        public void Update(float deltaTime)
        {
            NaspawnujMonstra();

            PohniMonstra(deltaTime);
        }

        public void NaspawnujMonstra()
        {
            while (monstra.Count < MAX_POCET_MONSTER)
            {
                monstra.Add(new Monstrum(new Vector2(hra.rnd.Next(0, KomponentaMapa.VELIKOST_MAPY_X * KomponentaMapa.VELIKOST_BLOKU), hra.rnd.Next(0, KomponentaMapa.VELIKOST_MAPY_Y * KomponentaMapa.VELIKOST_BLOKU))));
            }
        }

        public void PohniMonstra(float deltaTime)
        {
            List<Monstrum> monstra2 = new List<Monstrum>();
            foreach (Monstrum monstrum in monstra)
            {
                Vector2 cil = new Vector2(float.MaxValue);

                foreach (var hrac in hra.komponentaMultiplayer.hraci)
                {
                    if (Vector2.Distance(cil, monstrum.pozice) > Vector2.Distance(hrac.Value, monstrum.pozice))
                        cil = hrac.Value;
                }

                if (cil == new Vector2(float.MaxValue))
                    return;

                Point startovniKostka = (monstrum.pozice + monstrum.velikost.ToVector2() / 2).ToPoint() / new Point(KomponentaMapa.VELIKOST_BLOKU);
                Point cilovaKostka = (cil + monstrum.velikost.ToVector2() / 2).ToPoint() / new Point(KomponentaMapa.VELIKOST_BLOKU);

                if (startovniKostka != cilovaKostka)
                {
                    Point dalsiKrok = hra.komponentaMapa.NajdiDalsiKrokNaCesteMeziBody(startovniKostka, cilovaKostka);
                    if (dalsiKrok == new Point(-1))
                        continue;
                    cil = (dalsiKrok.ToVector2() + new Vector2(0.5f)) * new Vector2(KomponentaMapa.VELIKOST_BLOKU) - monstrum.velikost.ToVector2() / 2;
                }

                Vector2 pohyb = Vector2.Normalize(cil - monstrum.pozice) * monstrum.rychlost * deltaTime;

                if (Math.Abs(monstrum.pozice.X - cil.X) > pohyb.X)
                    monstrum.pozice.X += pohyb.X;
                else
                    monstrum.pozice.X = cil.X;

                if (Math.Abs(monstrum.pozice.Y - cil.Y) > pohyb.Y)
                    monstrum.pozice.Y += pohyb.Y;
                else
                    monstrum.pozice.Y = cil.Y;
                monstra2.Add(monstrum);
            }

            monstra = monstra2;
        }

        public byte[] PrevedMonstraNaByty()
        {
            List<byte> monstrumVBytech = new List<byte>
            {
                (byte)(monstra.Count / 255),
                (byte)(monstra.Count % 255)
            };

            for (int i = 0; i < monstra.Count; i++)
            {
                monstrumVBytech.Add((byte)monstra[i].velikost.X);
                monstrumVBytech.Add((byte)monstra[i].velikost.Y);
                monstrumVBytech.Add((byte)(monstra[i].pozice.X / 255));
                monstrumVBytech.Add((byte)(monstra[i].pozice.X % 255));
                monstrumVBytech.Add((byte)(monstra[i].pozice.X % 1 * 255));
                monstrumVBytech.Add((byte)(monstra[i].pozice.Y / 255));
                monstrumVBytech.Add((byte)(monstra[i].pozice.Y % 255));
                monstrumVBytech.Add((byte)(monstra[i].pozice.Y % 1 * 255));
                monstrumVBytech.Add((byte)monstra[i].rychlost);
                monstrumVBytech.Add((byte)monstra[i].typMonstra);
            }

            return Encoding.UTF8.GetBytes(Convert.ToBase64String(monstrumVBytech.ToArray()));
        }
    }
}
