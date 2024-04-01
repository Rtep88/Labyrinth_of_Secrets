using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Labyrinth_of_Secrets
{
    public class KomponentaZbrane
    {
        //Zaklad
        private Hra hra;

        //Promenne
        private List<Projektil> projektily = new List<Projektil>();
        public List<Projektil> noveProjektily = new List<Projektil>();

        public bool zmenaProjektilu = false;

        public KomponentaZbrane(Hra hra)
        {
            this.hra = hra;
        }

        public void Update(float deltaTime)
        {
            while (noveProjektily.Count > 0)
            {
                projektily.Add(noveProjektily.Last());
                noveProjektily.RemoveAt(noveProjektily.Count - 1);
                zmenaProjektilu = true;
            }

            for (int i = 0; i < projektily.Count; i++)
            {
                projektily[i].PohniSe(deltaTime);
                RotatedRectangle obdelnikProjektilu = new RotatedRectangle
                {
                    Center = projektily[i].pozice,
                    Width = projektily[i].velikost.X,
                    Height = projektily[i].velikost.Y,
                    Rotation = (float)(Math.PI / 2 + Math.Atan2(projektily[i].smer.Y, projektily[i].smer.X))
                };

                Point aktualniKostka = (projektily[i].pozice / new Vector2(KomponentaMapa.VELIKOST_BLOKU)).ToPoint();
                bool kolize = false;
                for (int x = Math.Max(0, aktualniKostka.X - 1); x <= Math.Min(KomponentaMapa.VELIKOST_MAPY_X - 1, aktualniKostka.X + 1); x++)
                {
                    for (int y = Math.Max(0, aktualniKostka.Y - 1); y <= Math.Min(KomponentaMapa.VELIKOST_MAPY_Y - 1, aktualniKostka.Y + 1); y++)
                    {
                        if (hra.komponentaMapa.mapa[x, y].typPole != Pole.TypPole.Zed)
                            continue;

                        RotatedRectangle obdelnikBloku = new RotatedRectangle
                        {
                            Center = new Vector2(x + 0.5f, y + 0.5f) * KomponentaMapa.VELIKOST_BLOKU,
                            Width = KomponentaMapa.VELIKOST_BLOKU,
                            Height = KomponentaMapa.VELIKOST_BLOKU,
                            Rotation = 0
                        };
                        if (obdelnikProjektilu.Intersects(obdelnikBloku))
                        {
                            projektily.RemoveAt(i);
                            i--;
                            kolize = true;
                            break;
                        }
                    }
                    if (kolize)
                        break;
                }
                if (kolize)
                    continue;

                List<Monstrum> monstra = hra.komponentaMonstra.monstra;
                for (int j = 0; j < monstra.Count; j++)
                {
                    if (Vector2.Distance(projektily[i].pozice, monstra[j].pozice) > 100)
                        continue;

                    RotatedRectangle obdelnikMonstra = new RotatedRectangle
                    {
                        Center = new Vector2(monstra[j].pozice.X, monstra[j].pozice.Y) + new Vector2(0.5f) * monstra[j].velikost.ToVector2(),
                        Width = monstra[j].velikost.X,
                        Height = monstra[j].velikost.Y,
                        Rotation = 0
                    };
                    if (obdelnikProjektilu.Intersects(obdelnikMonstra))
                    {
                        if (monstra[j].zivoty - projektily[i].zraneni <= 0)
                            monstra.RemoveAt(j);
                        else
                            monstra[j].zivoty -= projektily[i].zraneni;
                        projektily.RemoveAt(i);
                        i--;
                        break;
                    }
                }
            }
        }

        public byte[] PrevedProjektilyNaByty()
        {
            List<byte> projektilyVBytech = new List<byte>
            {
                (byte)(projektily.Count / 255),
                (byte)(projektily.Count % 255)
            };

            for (int i = 0; i < projektily.Count; i++)
            {
                projektilyVBytech.Add((byte)projektily[i].velikost.X);
                projektilyVBytech.Add((byte)projektily[i].velikost.Y);
                projektilyVBytech.Add((byte)(projektily[i].pozice.X / 255));
                projektilyVBytech.Add((byte)(projektily[i].pozice.X % 255));
                projektilyVBytech.Add((byte)(projektily[i].pozice.X % 1 * 255));
                projektilyVBytech.Add((byte)(projektily[i].pozice.Y / 255));
                projektilyVBytech.Add((byte)(projektily[i].pozice.Y % 255));
                projektilyVBytech.Add((byte)(projektily[i].pozice.Y % 1 * 255));
                projektilyVBytech.Add((byte)(Math.Abs(projektily[i].smer.X) * 255));
                projektilyVBytech.Add((byte)(projektily[i].smer.X < 0 ? 1 : 0));
                projektilyVBytech.Add((byte)(Math.Abs(projektily[i].smer.Y) * 255));
                projektilyVBytech.Add((byte)(projektily[i].smer.Y < 0 ? 1 : 0));
                projektilyVBytech.Add((byte)projektily[i].rychlost);
                projektilyVBytech.Add((byte)(projektily[i].zraneni / 255 / 255));
                projektilyVBytech.Add((byte)(projektily[i].zraneni % (255 * 255) / 255));
                projektilyVBytech.Add((byte)(projektily[i].zraneni % 255));
                projektilyVBytech.Add((byte)projektily[i].typProjektilu);
            }

            return Encoding.UTF8.GetBytes(Convert.ToBase64String(projektilyVBytech.ToArray()));
        }
    }
}
