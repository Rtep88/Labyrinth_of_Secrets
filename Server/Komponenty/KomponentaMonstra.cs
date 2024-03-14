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

        public void Update()
        {
            /*while (monstra.Count < MAX_POCET_MONSTER)
            {
                monstra.Add(new Monstrum(new Vector2(hra.rnd.Next(0, KomponentaMapa.VELIKOST_MAPY_X * KomponentaMapa.VELIKOST_BLOKU), hra.rnd.Next(0, KomponentaMapa.VELIKOST_MAPY_Y * KomponentaMapa.VELIKOST_BLOKU))));
            }

            foreach (Monstrum monstrum in monstra)
            {
                int pocetUpdatu = (int)(monstrum.rychlost / 20) + 1;
                Vector2 cil = hra.komponentaHrac.poziceHrace;

                for (int i = 0; i < pocetUpdatu; i++)
                {
                    if (monstrum.pozice != cil)
                        monstrum.pozice += Vector2.Normalize(cil - monstrum.pozice) * monstrum.rychlost * (float)gameTime.ElapsedGameTime.TotalSeconds / pocetUpdatu;
                }

                //Kolize
                Point aktualniBlok = new Point((int)((monstrum.pozice.X + monstrum.velikost.X / 2f) / KomponentaMapa.VELIKOST_BLOKU),
                    (int)((monstrum.pozice.Y + monstrum.velikost.Y / 2f) / KomponentaMapa.VELIKOST_BLOKU));

                Point odkudKontrolovat = new Point(Math.Max(0, aktualniBlok.X - 2), Math.Max(0, aktualniBlok.Y - 2));
                Point kamKontrolovat = new Point(Math.Min(KomponentaMapa.VELIKOST_MAPY_X - 1, aktualniBlok.X + 2),
                    Math.Min(KomponentaMapa.VELIKOST_MAPY_Y - 1, aktualniBlok.Y + 2));

                List<Point> poziceNaKontrolu = new List<Point>();
                for (int x = odkudKontrolovat.X; x <= kamKontrolovat.X; x++)
                    for (int y = odkudKontrolovat.Y; y <= kamKontrolovat.Y; y++)
                        poziceNaKontrolu.Add(new Point(x, y));

                poziceNaKontrolu = poziceNaKontrolu.OrderBy(x => Vector2.Distance(new Vector2((x.X + 0.5f) * KomponentaMapa.VELIKOST_BLOKU,
                    (x.Y + 0.5f) * KomponentaMapa.VELIKOST_BLOKU), monstrum.pozice + new Vector2(monstrum.velikost.X / 2f, monstrum.velikost.Y / 2f))).ToList();

                for (int j = 0; j < poziceNaKontrolu.Count; j++)
                {
                    int x = poziceNaKontrolu[j].X;
                    int y = poziceNaKontrolu[j].Y;

                    if (hra.komponentaMapa.mapa[x, y].typPole != Pole.TypPole.Zed)
                        continue;

                    Rectangle obdelnikBloku = new Rectangle(x * KomponentaMapa.VELIKOST_BLOKU, y * KomponentaMapa.VELIKOST_BLOKU,
                        KomponentaMapa.VELIKOST_BLOKU, KomponentaMapa.VELIKOST_BLOKU);

                    if (Hra.KolizeObdelniku(monstrum.pozice.X, monstrum.pozice.Y, monstrum.velikost.X, monstrum.velikost.Y,
                        obdelnikBloku.X, obdelnikBloku.Y, obdelnikBloku.Width, obdelnikBloku.Height))
                    {
                        float x1 = Math.Max(monstrum.pozice.X, obdelnikBloku.Left);
                        float y1 = Math.Max(monstrum.pozice.Y, obdelnikBloku.Top);
                        float x2 = Math.Min(monstrum.pozice.X + monstrum.velikost.X, obdelnikBloku.Right);
                        float y2 = Math.Min(monstrum.pozice.Y + monstrum.velikost.Y, obdelnikBloku.Bottom);

                        Vector2 bodDotyku = new Vector2((x1 + x2) / 2, (y1 + y2) / 2);

                        float vzdalenostKeStene = Math.Min(Math.Min(Vector2.Distance(bodDotyku, new Vector2(obdelnikBloku.Left, obdelnikBloku.Center.Y)),
                        Vector2.Distance(bodDotyku, new Vector2(obdelnikBloku.Right, obdelnikBloku.Center.Y))), Math.Min(
                            Vector2.Distance(bodDotyku, new Vector2(obdelnikBloku.Center.X, obdelnikBloku.Top)),
                            Vector2.Distance(bodDotyku, new Vector2(obdelnikBloku.Center.X, obdelnikBloku.Bottom))
                        ));

                        if (vzdalenostKeStene == Vector2.Distance(bodDotyku, new Vector2(obdelnikBloku.Left, obdelnikBloku.Center.Y))) //Leva
                            monstrum.pozice.X = obdelnikBloku.Left - monstrum.velikost.X;
                        if (vzdalenostKeStene == Vector2.Distance(bodDotyku, new Vector2(obdelnikBloku.Right, obdelnikBloku.Center.Y))) //Prava
                            monstrum.pozice.X = obdelnikBloku.Right;
                        if (vzdalenostKeStene == Vector2.Distance(bodDotyku, new Vector2(obdelnikBloku.Center.X, obdelnikBloku.Bottom))) //Dolni
                            monstrum.pozice.Y = obdelnikBloku.Bottom;
                        if (vzdalenostKeStene == Vector2.Distance(bodDotyku, new Vector2(obdelnikBloku.Center.X, obdelnikBloku.Top))) //Horni
                            monstrum.pozice.Y = obdelnikBloku.Top - monstrum.velikost.Y;
                    }
                }
            }*/
        }
    }
}
