using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Labyrinth_of_Secrets
{
    public class KomponentaMonstra : DrawableGameComponent
    {
        //Zaklad
        private Hra hra;

        //Textury
        public static List<Texture2D> texturyMonster = new List<Texture2D>();

        //Struktury

        //Konstanty
        private const int MAX_POCET_MONSTER = 100;
        private const int UKAZATEL_ZDRAVI_X_VEL = 6;
        private const int UKAZATEL_ZDRAVI_Y_VEL = 1;

        //Promenne
        public List<Monstrum> monstra = new List<Monstrum>();
        private List<Monstrum> novaMonstra = null;

        public KomponentaMonstra(Hra hra) : base(hra)
        {
            this.hra = hra;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            texturyMonster.Add(hra.Content.Load<Texture2D>("Images\\Player"));
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (hra.komponentaMultiplayer.typZarizeni == KomponentaMultiplayer.TypZarizeni.SinglePlayer)
            {
                NaspawnujMonstra();
                if ((float)gameTime.ElapsedGameTime.TotalSeconds != 0)
                {
                    int pocet = Math.Max(1, (int)(360f * (float)gameTime.ElapsedGameTime.TotalSeconds));
                    for (int i = 0; i < pocet; i++)
                        PohniMonstra((float)gameTime.ElapsedGameTime.TotalSeconds / pocet);
                }
            }
            else if (hra.komponentaMultiplayer.typZarizeni == KomponentaMultiplayer.TypZarizeni.Klient)
            {
                if ((float)gameTime.ElapsedGameTime.TotalSeconds != 0)
                {
                    int pocet = Math.Max(1, (int)(360f * (float)gameTime.ElapsedGameTime.TotalSeconds));
                    for (int i = 0; i < pocet; i++)
                        PohniMonstra((float)gameTime.ElapsedGameTime.TotalSeconds / pocet);
                }
                if (novaMonstra != null)
                {
                    monstra = novaMonstra;
                    novaMonstra = null;
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!hra.komponentaMinimapa.jeOtevrena)
            {
                foreach (Monstrum monstrum in monstra)
                {
                    hra._spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: hra.komponentaKamera._kamera.GetViewMatrix());

                    //zivoty
                    hra._spriteBatch.Draw(Hra.pixel, monstrum.pozice + new Vector2(monstrum.velikost.X / 2 - UKAZATEL_ZDRAVI_X_VEL / 2 - 0.25f, -0.5f - UKAZATEL_ZDRAVI_Y_VEL - 0.25f), null, Color.Black, 0, Vector2.Zero,
                        new Vector2(UKAZATEL_ZDRAVI_X_VEL + 0.5f, UKAZATEL_ZDRAVI_Y_VEL + 0.5f), SpriteEffects.None, 0);
                    hra._spriteBatch.Draw(Hra.pixel, monstrum.pozice + new Vector2(monstrum.velikost.X / 2 - UKAZATEL_ZDRAVI_X_VEL / 2, -0.5f - UKAZATEL_ZDRAVI_Y_VEL), null, Color.Red, 0, Vector2.Zero,
                        new Vector2(UKAZATEL_ZDRAVI_X_VEL, UKAZATEL_ZDRAVI_Y_VEL) * new Vector2(monstrum.zivoty / (float)monstrum.maxZivoty, 1), SpriteEffects.None, 0);

                    //monstrum
                    hra._spriteBatch.Draw(texturyMonster[monstrum.typMonstra], monstrum.pozice, null, Color.Red, 0, Vector2.Zero,
                        monstrum.velikost.ToVector2() / new Vector2(texturyMonster[monstrum.typMonstra].Width, texturyMonster[monstrum.typMonstra].Height), SpriteEffects.None, 0);

                    hra._spriteBatch.End();
                }
            }

            base.Draw(gameTime);
        }

        public void ResetujPromenne()
        {
            monstra = new List<Monstrum>();
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
                (Vector2, bool) odpudivaSila = VypocitejOdpudivouSilu(monstra[i].pozice, monstra[i].velikost.ToVector2(), hra.komponentaHrac.poziceHrace, new Vector2(KomponentaHrac.VELIKOST_HRACE_X, KomponentaHrac.VELIKOST_HRACE_Y));
                monstra[i].pozice += odpudivaSila.Item1 * 0.3f * 60 * deltaTime;
                if (odpudivaSila.Item2)
                    hra.komponentaHrac.zivoty -= 180 * deltaTime;

            }
            for (int i = 0; i < monstra.Count; i++)
            {
                foreach (var hrac in hra.komponentaMultiplayer.hraci)
                {
                    (Vector2, bool) odpudivaSila = VypocitejOdpudivouSilu(monstra[i].pozice, monstra[i].velikost.ToVector2(), hrac.Value.pozice, new Vector2(KomponentaHrac.VELIKOST_HRACE_X, KomponentaHrac.VELIKOST_HRACE_Y));
                    monstra[i].pozice += odpudivaSila.Item1 * 0.3f * 60 * deltaTime;
                }
            }

            //Aplikovani bloku na monstra
            for (int i = 0; i < monstra.Count; i++)
                monstra[i].pozice = hra.komponentaMapa.VypocitejKolizeSBloky(monstra[i].pozice, monstra[i].velikost.ToVector2());

            //Pohyb
            for (int i = 0; i < monstra.Count; i++)
            {
                Vector2 cil = hra.komponentaHrac.poziceHrace;

                if (hra.komponentaMultiplayer.typZarizeni == KomponentaMultiplayer.TypZarizeni.Klient)
                {
                    foreach (var hrac in hra.komponentaMultiplayer.hraci)
                    {
                        if (Vector2.Distance(cil, monstra[i].pozice) > Vector2.Distance(hrac.Value.pozice, monstra[i].pozice))
                            cil = hrac.Value.pozice;
                    }
                }
                Point startovniKostka = (monstra[i].pozice + monstra[i].velikost.ToVector2() / 2).ToPoint() / new Point(KomponentaMapa.VELIKOST_BLOKU);
                Point cilovaKostka = (cil + new Vector2(KomponentaHrac.VELIKOST_HRACE_X, KomponentaHrac.VELIKOST_HRACE_Y) / 2).ToPoint() / new Point(KomponentaMapa.VELIKOST_BLOKU);

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

            novaMonstra = monstraZBytu;
        }
    }
}
