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
        public List<Monstrum> monstra = new List<Monstrum>();

        public KomponentaMonstra(Hra hra)
        {
            this.hra = hra;
        }

        public void Update(float deltaTime)
        {
            NaspawnujMonstra();

            //Regenerace hracu
            foreach (var hrac in hra.komponentaMultiplayer.hraci)
            {
                if (hrac.Value.zivoty < Hra.MAX_ZIVOTY)
                    hrac.Value.zivoty += deltaTime * Hra.RYCHLOST_REGENERACE;
                if (hrac.Value.zivoty > Hra.MAX_ZIVOTY)
                    hrac.Value.zivoty = Hra.MAX_ZIVOTY;
            }

            if (deltaTime != 0)
            {
                int pocet = Math.Max(1, (int)(360f * deltaTime));
                for (int i = 0; i < pocet; i++)
                    PohniMonstra(deltaTime / pocet);
            }
        }

        public void NaspawnujMonstra()
        {
            while (monstra.Count < MAX_POCET_MONSTER)
            {
                monstra.Add(new Monstrum(new Vector2(hra.rnd.Next(0, KomponentaMapa.VELIKOST_MAPY_X * KomponentaMapa.VELIKOST_BLOKU), hra.rnd.Next(0, KomponentaMapa.VELIKOST_MAPY_Y * KomponentaMapa.VELIKOST_BLOKU))));
            }
        }

        //Vrati pokud je hrac v kolizi a zaroven vrati odpudivou silu
        public (Vector2, bool) VypocitejOdpudivouSilu(Vector2 pozice1, Vector2 velikost1, Vector2 pozice2, Vector2 velikost2)
        {
            if (Hra.KolizeObdelniku(pozice1.X, pozice1.Y, velikost1.X, velikost1.Y, pozice2.X, pozice2.Y, velikost2.X, velikost2.Y))
            {
                Vector2 rozdilPozic = pozice1 + velikost1 / 2 - pozice2 - velikost2 / 2;
                if (rozdilPozic.X != 0 || rozdilPozic.Y != 0)
                {
                    Vector2 normalizace = Vector2.Normalize(rozdilPozic);
                    float sila = (float)Math.Sqrt((velikost2.X / 2f + velikost1.X / 2f - Math.Abs(rozdilPozic.X)) * (velikost2.Y / 2f + velikost1.Y / 2f - Math.Abs(rozdilPozic.Y)));
                    return (normalizace * sila, true);
                }
            }
            return (Vector2.Zero, false);
        }

        public void PohniMonstra(float deltaTime)
        {
            //Aplikovani monster na monstra
            for (int i = 0; i < monstra.Count; i++)
            {
                for (int j = 0; j < monstra.Count; j++)
                {
                    if (j == i)
                        continue;
                    (Vector2, bool) odpudivaSila = VypocitejOdpudivouSilu(monstra[j].pozice, monstra[j].velikost.ToVector2(), monstra[i].pozice, monstra[i].velikost.ToVector2());
                    monstra[j].pozice += odpudivaSila.Item1 * 0.15f * 60 * deltaTime;
                }
            }

            //Aplikovani hracu na monstra
            for (int i = 0; i < monstra.Count; i++)
            {
                foreach (var hrac in hra.komponentaMultiplayer.hraci)
                {
                    if (hrac.Value.jePripojen && Hra.KolizeObdelniku(hrac.Value.pozice.X, hrac.Value.pozice.Y, Hra.VELIKOST_HRACE_X, Hra.VELIKOST_HRACE_Y, monstra[i].pozice.X, monstra[i].pozice.Y, monstra[i].velikost.X, monstra[i].velikost.Y))
                    {
                        (Vector2, bool) odpudivaSila = VypocitejOdpudivouSilu(monstra[i].pozice, monstra[i].velikost.ToVector2(), hrac.Value.pozice, new Vector2(Hra.VELIKOST_HRACE_X, Hra.VELIKOST_HRACE_Y));
                        monstra[i].pozice += odpudivaSila.Item1 * 0.3f * 60 * deltaTime;

                        if (odpudivaSila.Item2)
                        {
                            hrac.Value.zivoty -= 180 * deltaTime;

                            if (hrac.Value.zivoty <= 0)
                            {
                                hrac.Value.pozice = new Vector2((hra.komponentaMapa.start.X + 0.5f) * KomponentaMapa.VELIKOST_BLOKU - Hra.VELIKOST_HRACE_X / 2f,
                                    (hra.komponentaMapa.start.Y + 0.5f) * KomponentaMapa.VELIKOST_BLOKU - Hra.VELIKOST_HRACE_Y / 2f);

                                hrac.Value.pozicePrebrana = false;

                                hrac.Value.zivoty = Hra.MAX_ZIVOTY;
                            }
                        }
                    }
                }
            }

            //Aplikovani bloku na monstra
            for (int i = 0; i < monstra.Count; i++)
                monstra[i].pozice = hra.komponentaMapa.VypocitejKolizeSBloky(monstra[i].pozice, monstra[i].velikost.ToVector2());

            //Pohyb
            for (int i = 0; i < monstra.Count; i++)
            {
                Vector2 cil = new Vector2(float.MaxValue);

                foreach (var hrac in hra.komponentaMultiplayer.hraci)
                {
                    if (hrac.Value.jePripojen && Vector2.Distance(cil, monstra[i].pozice) > Vector2.Distance(hrac.Value.pozice, monstra[i].pozice))
                        cil = hrac.Value.pozice;
                }


                Point startovniKostka = (monstra[i].pozice + monstra[i].velikost.ToVector2() / 2).ToPoint() / new Point(KomponentaMapa.VELIKOST_BLOKU);
                Point cilovaKostka = (cil + new Vector2(Hra.VELIKOST_HRACE_X, Hra.VELIKOST_HRACE_Y) / 2).ToPoint() / new Point(KomponentaMapa.VELIKOST_BLOKU);

                if (cil == new Vector2(float.MaxValue))
                    cilovaKostka = (monstra[i].pozice + monstra[i].velikost.ToVector2() / 2).ToPoint() / new Point(KomponentaMapa.VELIKOST_BLOKU);

                if (startovniKostka != cilovaKostka)
                {
                    Point dalsiKrok = hra.komponentaMapa.NajdiDalsiKrokNaCesteMeziBody(startovniKostka, cilovaKostka);
                    if (dalsiKrok == new Point(-1))
                    {
                        monstra.RemoveAt(i);
                        i--;
                        continue;
                    }
                    cil = (dalsiKrok.ToVector2() + new Vector2(0.5f)) * new Vector2(KomponentaMapa.VELIKOST_BLOKU) - monstra[i].velikost.ToVector2() / 2;
                }

                Vector2 pohyb = Vector2.Normalize(cil - monstra[i].pozice) * monstra[i].rychlost * deltaTime;

                if (Math.Abs(monstra[i].pozice.X - cil.X) > pohyb.X)
                    monstra[i].pozice.X += pohyb.X;
                else
                    monstra[i].pozice.X = cil.X;

                if (Math.Abs(monstra[i].pozice.Y - cil.Y) > pohyb.Y)
                    monstra[i].pozice.Y += pohyb.Y;
                else
                    monstra[i].pozice.Y = cil.Y;
            }
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
                monstrumVBytech.Add((byte)(monstra[i].zivoty / 255 / 255));
                monstrumVBytech.Add((byte)(monstra[i].zivoty % (255 * 255) / 255));
                monstrumVBytech.Add((byte)(monstra[i].zivoty % 255));
                monstrumVBytech.Add((byte)(monstra[i].maxZivoty / 255 / 255));
                monstrumVBytech.Add((byte)(monstra[i].maxZivoty % (255 * 255) / 255));
                monstrumVBytech.Add((byte)(monstra[i].maxZivoty % 255));
                monstrumVBytech.Add((byte)monstra[i].rychlost);
                monstrumVBytech.Add((byte)monstra[i].typMonstra);
            }

            return Encoding.UTF8.GetBytes(Convert.ToBase64String(monstrumVBytech.ToArray()));
        }

        public void PrevedBytyNaMonstra(byte[] prichoziBytyMonster)
        {
            byte[] bytyMonster = Convert.FromBase64String(Encoding.UTF8.GetString(prichoziBytyMonster));

            int pocetMonster = bytyMonster[0] * 255 + bytyMonster[1];
            List<Monstrum> monstraZBytu = new List<Monstrum>(pocetMonster);

            for (int i = 0; i < pocetMonster; i++)
            {
                Monstrum monstrum = new Monstrum();
                monstrum.velikost.X = bytyMonster[2 + i * 16];
                monstrum.velikost.Y = bytyMonster[2 + i * 16 + 1];
                monstrum.pozice.X = bytyMonster[2 + i * 16 + 2] * 255 + bytyMonster[2 + i * 16 + 3] + bytyMonster[2 + i * 16 + 4] / 255f;
                monstrum.pozice.Y = bytyMonster[2 + i * 16 + 5] * 255 + bytyMonster[2 + i * 16 + 6] + bytyMonster[2 + i * 16 + 7] / 255f;
                monstrum.zivoty = bytyMonster[2 + i * 16 + 8] * 255 * 255 + bytyMonster[2 + i * 16 + 9] * 255 + bytyMonster[2 + i * 16 + 10];
                monstrum.maxZivoty = bytyMonster[2 + i * 16 + 11] * 255 * 255 + bytyMonster[2 + i * 16 + 12] * 255 + bytyMonster[2 + i * 16 + 13];
                monstrum.rychlost = bytyMonster[2 + i * 16 + 14];
                monstrum.typMonstra = bytyMonster[2 + i * 16 + 15];
                monstraZBytu.Add(monstrum);
            }

            monstra = monstraZBytu;
        }
    }
}
