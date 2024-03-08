﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Labyrinth_of_Secrets
{
    public class KomponentaHrac : DrawableGameComponent
    {
        //Zaklad
        private Hra hra;

        //Textury
        private Texture2D hrac;

        //Struktury

        //Konstanty
        public const int VELIKOST_HRACE_X = 6; //Velikost vykresleni hrace
        public const int VELIKOST_HRACE_Y = 12; //Velikost vykresleni hrace
        public const int RYCHLOST_HRACE = 50;

        //Promene
        public Vector2 poziceHrace = new Vector2();

        public KomponentaHrac(Hra hra) : base(hra)
        {
            this.hra = hra;
        }

        public override void Initialize()
        {
            poziceHrace = new Vector2((hra.komponentaMapa.start.X + 0.5f) * KomponentaMapa.VELIKOST_BLOKU - VELIKOST_HRACE_X / 2f,
             (hra.komponentaMapa.start.Y + 0.5f) * KomponentaMapa.VELIKOST_BLOKU - VELIKOST_HRACE_Y / 2f);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            hrac = hra.Content.Load<Texture2D>("Images\\Player");
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            int pocetUpdatu = RYCHLOST_HRACE / 20;

            for (int i = 0; i < pocetUpdatu; i++)
            {
                //Pohyb hrace
                if (!hra.komponentaKonzole.jeOtevrena)
                {
                    if (Keyboard.GetState().IsKeyDown(Keys.W))
                        poziceHrace -= new Vector2(0, RYCHLOST_HRACE * (float)gameTime.ElapsedGameTime.TotalSeconds / pocetUpdatu);
                    if (Keyboard.GetState().IsKeyDown(Keys.D))
                        poziceHrace += new Vector2(RYCHLOST_HRACE * (float)gameTime.ElapsedGameTime.TotalSeconds / pocetUpdatu, 0);
                    if (Keyboard.GetState().IsKeyDown(Keys.S))
                        poziceHrace += new Vector2(0, RYCHLOST_HRACE * (float)gameTime.ElapsedGameTime.TotalSeconds / pocetUpdatu);
                    if (Keyboard.GetState().IsKeyDown(Keys.A))
                        poziceHrace -= new Vector2(RYCHLOST_HRACE * (float)gameTime.ElapsedGameTime.TotalSeconds / pocetUpdatu, 0);
                }

                //Kolize
                Point aktualniBlok = new Point((int)((poziceHrace.X + VELIKOST_HRACE_X / 2f) / KomponentaMapa.VELIKOST_BLOKU),
                    (int)((poziceHrace.Y + VELIKOST_HRACE_Y / 2f) / KomponentaMapa.VELIKOST_BLOKU));

                Point odkudKontrolovat = new Point(Math.Max(0, aktualniBlok.X - 2), Math.Max(0, aktualniBlok.Y - 2));
                Point kamKontrolovat = new Point(Math.Min(KomponentaMapa.VELIKOST_MAPY_X - 1, aktualniBlok.X + 2),
                    Math.Min(KomponentaMapa.VELIKOST_MAPY_Y - 1, aktualniBlok.Y + 2));

                List<Point> poziceNaKontrolu = new List<Point>();
                for (int x = odkudKontrolovat.X; x <= kamKontrolovat.X; x++)
                    for (int y = odkudKontrolovat.Y; y <= kamKontrolovat.Y; y++)
                        poziceNaKontrolu.Add(new Point(x, y));

                poziceNaKontrolu = poziceNaKontrolu.OrderBy(x => Vector2.Distance(new Vector2((x.X + 0.5f) * KomponentaMapa.VELIKOST_BLOKU,
                    (x.Y + 0.5f) * KomponentaMapa.VELIKOST_BLOKU), poziceHrace + new Vector2(VELIKOST_HRACE_X / 2f, VELIKOST_HRACE_Y / 2f))).ToList();

                for (int j = 0; j < poziceNaKontrolu.Count; j++)
                {
                    int x = poziceNaKontrolu[j].X;
                    int y = poziceNaKontrolu[j].Y;

                    if (hra.komponentaMapa.mapa[x, y].typPole != Pole.TypPole.Zed)
                        continue;

                    Rectangle obdelnikBloku = new Rectangle(x * KomponentaMapa.VELIKOST_BLOKU, y * KomponentaMapa.VELIKOST_BLOKU,
                        KomponentaMapa.VELIKOST_BLOKU, KomponentaMapa.VELIKOST_BLOKU);

                    if (Hra.KolizeObdelniku(poziceHrace.X, poziceHrace.Y, VELIKOST_HRACE_X, VELIKOST_HRACE_Y,
                        obdelnikBloku.X, obdelnikBloku.Y, obdelnikBloku.Width, obdelnikBloku.Height))
                    {
                        float x1 = Math.Max(poziceHrace.X, obdelnikBloku.Left);
                        float y1 = Math.Max(poziceHrace.Y, obdelnikBloku.Top);
                        float x2 = Math.Min(poziceHrace.X + VELIKOST_HRACE_X, obdelnikBloku.Right);
                        float y2 = Math.Min(poziceHrace.Y + VELIKOST_HRACE_Y, obdelnikBloku.Bottom);

                        Vector2 bodDotyku = new Vector2((x1 + x2) / 2, (y1 + y2) / 2);

                        float vzdalenostKeStene = Math.Min(Math.Min(Vector2.Distance(bodDotyku, new Vector2(obdelnikBloku.Left, obdelnikBloku.Center.Y)),
                        Vector2.Distance(bodDotyku, new Vector2(obdelnikBloku.Right, obdelnikBloku.Center.Y))), Math.Min(
                            Vector2.Distance(bodDotyku, new Vector2(obdelnikBloku.Center.X, obdelnikBloku.Top)),
                            Vector2.Distance(bodDotyku, new Vector2(obdelnikBloku.Center.X, obdelnikBloku.Bottom))
                        ));

                        if (vzdalenostKeStene == Vector2.Distance(bodDotyku, new Vector2(obdelnikBloku.Left, obdelnikBloku.Center.Y))) //Leva
                            poziceHrace.X = obdelnikBloku.Left - VELIKOST_HRACE_X;
                        if (vzdalenostKeStene == Vector2.Distance(bodDotyku, new Vector2(obdelnikBloku.Right, obdelnikBloku.Center.Y))) //Prava
                            poziceHrace.X = obdelnikBloku.Right;
                        if (vzdalenostKeStene == Vector2.Distance(bodDotyku, new Vector2(obdelnikBloku.Center.X, obdelnikBloku.Bottom))) //Dolni
                            poziceHrace.Y = obdelnikBloku.Bottom;
                        if (vzdalenostKeStene == Vector2.Distance(bodDotyku, new Vector2(obdelnikBloku.Center.X, obdelnikBloku.Top))) //Horni
                            poziceHrace.Y = obdelnikBloku.Top - VELIKOST_HRACE_Y;
                    }
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            hra._spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: hra.komponentaKamera._kamera.GetViewMatrix());
            hra._spriteBatch.Draw(hrac, poziceHrace, null, Color.White, 0, Vector2.Zero,
                new Vector2((float)VELIKOST_HRACE_X / hrac.Width, (float)VELIKOST_HRACE_Y / hrac.Height), SpriteEffects.None, 0);
            hra._spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
