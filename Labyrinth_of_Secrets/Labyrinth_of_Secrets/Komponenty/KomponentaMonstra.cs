﻿using Microsoft.Xna.Framework;
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

        //Promenne
        public List<Monstrum> monstra = new List<Monstrum>();
        public List<Monstrum> novaMonstra = null;

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
                PohniMonstra((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
            else if (hra.komponentaMultiplayer.typZarizeni == KomponentaMultiplayer.TypZarizeni.Klient)
            {
                PohniMonstra((float)gameTime.ElapsedGameTime.TotalSeconds);
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
            foreach (Monstrum monstrum in monstra)
            {
                hra._spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: hra.komponentaKamera._kamera.GetViewMatrix());

                hra._spriteBatch.Draw(texturyMonster[monstrum.typMonstra], monstrum.pozice, null, Color.Red, 0, Vector2.Zero,
                    monstrum.velikost.ToVector2() / new Vector2(texturyMonster[monstrum.typMonstra].Width, texturyMonster[monstrum.typMonstra].Height), SpriteEffects.None, 0);

                hra._spriteBatch.End();
            }

            base.Draw(gameTime);
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
            foreach (Monstrum monstrum in monstra)
            {
                int pocetUpdatu = (int)(monstrum.rychlost / 20) + 1;
                Vector2 cil = hra.komponentaHrac.poziceHrace;

                if (hra.komponentaMultiplayer.typZarizeni == KomponentaMultiplayer.TypZarizeni.Klient)
                {
                    foreach (var hrac in hra.komponentaMultiplayer.hraci)
                    {
                        if (Vector2.Distance(cil, monstrum.pozice) > Vector2.Distance(hrac.Value, monstrum.pozice))
                            cil = hrac.Value;
                    }
                }

                for (int i = 0; i < pocetUpdatu; i++)
                {
                    if (monstrum.pozice != cil)
                    {
                        Vector2 pohyb = Vector2.Normalize(cil - monstrum.pozice) * monstrum.rychlost * deltaTime / pocetUpdatu;

                        if (Math.Abs(monstrum.pozice.X - cil.X) > pohyb.X)
                            monstrum.pozice.X += pohyb.X;
                        else
                            monstrum.pozice.X = cil.X;

                        if (Math.Abs(monstrum.pozice.Y - cil.Y) > pohyb.Y)
                            monstrum.pozice.Y += pohyb.Y;
                        else
                            monstrum.pozice.Y = cil.Y;
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
                }
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
                monstrum.velikost.X = bytyMonster[2 + i * 10];
                monstrum.velikost.Y = bytyMonster[2 + i * 10 + 1];
                monstrum.pozice.X = bytyMonster[2 + i * 10 + 2] * 255 + bytyMonster[2 + i * 10 + 3] + bytyMonster[2 + i * 10 + 4] / 255f;
                monstrum.pozice.Y = bytyMonster[2 + i * 10 + 5] * 255 + bytyMonster[2 + i * 10 + 6] + bytyMonster[2 + i * 10 + 7] / 255f;
                monstrum.rychlost = bytyMonster[2 + i * 10 + 8];
                monstrum.typMonstra = bytyMonster[2 + i * 10 + 9];
                monstraZBytu.Add(monstrum);
            }

            novaMonstra = monstraZBytu;
        }
    }
}
