using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace Labyrinth_of_Secrets
{
    public class KomponentaMinimapa : DrawableGameComponent
    {
        //Zaklad
        private Hra hra;
        public Kamera kameraMapy;

        //Konstanty
        const int RYCHLOST_KAMERY = 10;

        //Promenne
        public RenderTarget2D minimapa;
        public int[,] svetloMinimapy;
        public Color dataMinimapy;
        public bool jeOtevrena = false;

        public KomponentaMinimapa(Hra hra) : base(hra)
        {
            this.hra = hra;
        }

        public override void Initialize()
        {
            VygenerujMinimapu();
            kameraMapy = new Kamera(GraphicsDevice.Viewport, 1);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (hra.NoveZmacknutaKlavesa(Keys.Q))
            {
                jeOtevrena = !jeOtevrena;
            }

            Vector2 velikostMapy = new Vector2(KomponentaMapa.VELIKOST_MAPY_X * KomponentaMapa.VELIKOST_BLOKU, KomponentaMapa.VELIKOST_MAPY_Y * KomponentaMapa.VELIKOST_BLOKU);

            if (jeOtevrena && !hra.komponentaKonzole.jeOtevrena)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.P))
                    kameraMapy.zoom *= 1.02f;
                else if (Keyboard.GetState().IsKeyDown(Keys.M) && velikostMapy.X * kameraMapy.zoom / 1.02 > hra.velikostOkna.X &&
                    velikostMapy.Y * kameraMapy.zoom / 1.02 > hra.velikostOkna.Y)
                    kameraMapy.zoom /= 1.02f;
                else if (Keyboard.GetState().IsKeyDown(Keys.M))
                    kameraMapy.zoom = Math.Max(hra.velikostOkna.X / velikostMapy.X, hra.velikostOkna.Y / velikostMapy.Y);
                if (Keyboard.GetState().IsKeyDown(Keys.Up))
                    kameraMapy.pozice -= new Vector2(0, RYCHLOST_KAMERY / kameraMapy.zoom);
                if (Keyboard.GetState().IsKeyDown(Keys.Right))
                    kameraMapy.pozice += new Vector2(RYCHLOST_KAMERY / kameraMapy.zoom, 0);
                if (Keyboard.GetState().IsKeyDown(Keys.Down))
                    kameraMapy.pozice += new Vector2(0, RYCHLOST_KAMERY / kameraMapy.zoom);
                if (Keyboard.GetState().IsKeyDown(Keys.Left))
                    kameraMapy.pozice -= new Vector2(RYCHLOST_KAMERY / kameraMapy.zoom, 0);
            }

            //Nastaveni zoomu tak aby kamera nebyla mimo vykreslovaci oblast
            if (kameraMapy.zoom < Math.Max(hra.velikostOkna.X / velikostMapy.X, hra.velikostOkna.Y / velikostMapy.Y))
                kameraMapy.zoom = Math.Max(hra.velikostOkna.X / velikostMapy.X, hra.velikostOkna.Y / velikostMapy.Y);

            //Vypocet potrebnych hodnot pro praci s kamerou
            Vector2 nula = new Vector2(kameraMapy.pozice.X + kameraMapy.GetViewMatrix().Translation.X / kameraMapy.zoom, kameraMapy.pozice.Y + kameraMapy.GetViewMatrix().Translation.Y / kameraMapy.zoom);
            Vector2 opravdovaPoziceKamery = new Vector2(-kameraMapy.GetViewMatrix().Translation.X / kameraMapy.zoom, -kameraMapy.GetViewMatrix().Translation.Y / kameraMapy.zoom);
            Vector2 opravdovaVelikostOkna = new Vector2(hra.velikostOkna.X / kameraMapy.zoom, hra.velikostOkna.Y / kameraMapy.zoom);

            //Posunuti kamery tak aby nebyla mimo vykreslovaci oblast
            if (opravdovaPoziceKamery.X < 0)
                kameraMapy.pozice = new Vector2(nula.X, kameraMapy.pozice.Y);
            if (opravdovaPoziceKamery.Y < 0)
                kameraMapy.pozice = new Vector2(kameraMapy.pozice.X, nula.Y);
            if (opravdovaPoziceKamery.X + opravdovaVelikostOkna.X > velikostMapy.X)
                kameraMapy.pozice = new Vector2(nula.X + velikostMapy.X - opravdovaVelikostOkna.X, kameraMapy.pozice.Y);
            if (opravdovaPoziceKamery.Y + opravdovaVelikostOkna.Y > velikostMapy.Y)
                kameraMapy.pozice = new Vector2(kameraMapy.pozice.X, nula.Y + velikostMapy.X - opravdovaVelikostOkna.Y);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (jeOtevrena)
            {
                hra._spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: kameraMapy.GetViewMatrix());
                hra._spriteBatch.Draw(minimapa, Vector2.Zero, Color.White);
                hra._spriteBatch.Draw(Hra.pixel, new Rectangle(hra.komponentaHrac.poziceHrace.ToPoint() - new Point(2) +
                    new Point(KomponentaHrac.VELIKOST_HRACE_X / 2, KomponentaHrac.VELIKOST_HRACE_Y / 2), new Point(5)), Color.Red);
                hra._spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        public void VygenerujMinimapu()
        {
            Point pocetPixelu = new Point(KomponentaMapa.VELIKOST_BLOKU * KomponentaMapa.VELIKOST_MAPY_X,
                KomponentaMapa.VELIKOST_BLOKU * KomponentaMapa.VELIKOST_MAPY_Y);

            minimapa = new RenderTarget2D(GraphicsDevice, pocetPixelu.X, pocetPixelu.Y, false, SurfaceFormat.Vector4,
                DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
            svetloMinimapy = new int[KomponentaMapa.VELIKOST_MAPY_X, KomponentaMapa.VELIKOST_MAPY_Y];

            GraphicsDevice.SetRenderTarget(minimapa);
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.SetRenderTarget(null);
        }

        public void AktualizujMinimapu()
        {
            GraphicsDevice.SetRenderTarget(minimapa);
            hra._spriteBatch.Begin();

            for (int x = 0; x < KomponentaMapa.VELIKOST_MAPY_X; x++)
            {
                for (int y = 0; y < KomponentaMapa.VELIKOST_MAPY_Y; y++)
                {
                    Color barvaNaPolicku = Color.Black;
                    for (int x2 = 0; x2 < KomponentaSvetlo.POCET_SVETLA_NA_BLOK; x2++)
                    {
                        for (int y2 = 0; y2 < KomponentaSvetlo.POCET_SVETLA_NA_BLOK; y2++)
                        {
                            Color aktualniBarva = hra.komponentaSvetlo.dataSvetla[1][x * KomponentaSvetlo.POCET_SVETLA_NA_BLOK + x2 +
                                ((y * KomponentaSvetlo.POCET_SVETLA_NA_BLOK + y2) * KomponentaMapa.VELIKOST_MAPY_X * KomponentaSvetlo.POCET_SVETLA_NA_BLOK)];
                            if (barvaNaPolicku.R + barvaNaPolicku.G + barvaNaPolicku.B < aktualniBarva.R + aktualniBarva.G + aktualniBarva.B)
                                barvaNaPolicku = aktualniBarva;
                        }
                    }

                    if (barvaNaPolicku.R + barvaNaPolicku.G + barvaNaPolicku.B > svetloMinimapy[x, y])
                    {
                        svetloMinimapy[x, y] = barvaNaPolicku.R + barvaNaPolicku.G + barvaNaPolicku.B;

                        if (hra.komponentaMapa.mapa[x, y].typPole == Pole.TypPole.Zed)
                            hra._spriteBatch.Draw(KomponentaMapa.brick, new Rectangle(x * KomponentaMapa.VELIKOST_BLOKU, y * KomponentaMapa.VELIKOST_BLOKU,
                                KomponentaMapa.VELIKOST_BLOKU, KomponentaMapa.VELIKOST_BLOKU), barvaNaPolicku);
                        else if (hra.komponentaMapa.mapa[x, y].typPole == Pole.TypPole.Obchodnik)
                            hra._spriteBatch.Draw(KomponentaMapa.brick, new Rectangle(x * KomponentaMapa.VELIKOST_BLOKU, y * KomponentaMapa.VELIKOST_BLOKU,
                                KomponentaMapa.VELIKOST_BLOKU, KomponentaMapa.VELIKOST_BLOKU),
                                new Color(barvaNaPolicku.R * 128 / 255, barvaNaPolicku.G * 128 / 255, barvaNaPolicku.B * 128 / 255));
                        else
                            hra._spriteBatch.Draw(Hra.pixel, new Rectangle(x * KomponentaMapa.VELIKOST_BLOKU, y * KomponentaMapa.VELIKOST_BLOKU,
                                KomponentaMapa.VELIKOST_BLOKU, KomponentaMapa.VELIKOST_BLOKU), barvaNaPolicku);
                    }
                }
            }

            hra._spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
        }
    }
}