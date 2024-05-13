using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Labyrinth_of_Secrets
{
    public class KomponentaZbrane : DrawableGameComponent
    {
        //Zaklad
        private Hra hra;

        //Textury
        public static List<Texture2D> texturyProjektilu = new List<Texture2D>();
        public static List<Texture2D> texturyZbrani = new List<Texture2D>();

        //Promenne
        public List<Projektil> projektily = new List<Projektil>();
        private List<Projektil> noveProjektily = null;
        public List<Zbran> zbrane = new List<Zbran>();
        public int aktualniZbran = 0;

        public KomponentaZbrane(Hra hra) : base(hra)
        {
            this.hra = hra;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            string cesta = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            char lomeno = Path.DirectorySeparatorChar;
            for (int i = 1; File.Exists(cesta + @$"{lomeno}Content{lomeno}Images{lomeno}Bullet_" + i + ".xnb"); i++)
            {
                texturyProjektilu.Add(hra.Content.Load<Texture2D>(@$"Images{lomeno}Bullet_" + i));
            }
            for (int i = 1; File.Exists(cesta + @$"{lomeno}Content{lomeno}Images{lomeno}Weapon_" + i + ".xnb"); i++)
            {
                texturyZbrani.Add(hra.Content.Load<Texture2D>(@$"Images{lomeno}Weapon_" + i));
            }

            for (int i = 0; i < 4; i++)
                zbrane.Add(new Zbran((Zbran.TypZbrane)i));

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            //Vyber zbrani
            if (Keyboard.GetState().IsKeyDown(Keys.D1))
                aktualniZbran = 0;
            if (Keyboard.GetState().IsKeyDown(Keys.D2))
                aktualniZbran = 1;
            if (Keyboard.GetState().IsKeyDown(Keys.D3))
                aktualniZbran = 2;
            if (Keyboard.GetState().IsKeyDown(Keys.D4))
                aktualniZbran = 3;

            if (hra.komponentaMultiplayer.typZarizeni == KomponentaMultiplayer.TypZarizeni.Klient && noveProjektily != null)
            {
                projektily = noveProjektily;
                noveProjektily = null;
            }

            for (int i = 0; i < zbrane.Count; i++)
                if (zbrane[i].aktCas > 0)
                    zbrane[i].aktCas -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                else
                    zbrane[i].aktCas = 0;

            Kamera _kamera = hra.komponentaKamera._kamera;
            Vector2 opravdovaPoziceKamery = new Vector2(-_kamera.GetViewMatrix().Translation.X / _kamera.zoom, -_kamera.GetViewMatrix().Translation.Y / _kamera.zoom);
            Vector2 opravdovaVelikostOkna = new Vector2(hra.velikostOkna.X / _kamera.zoom, hra.velikostOkna.Y / _kamera.zoom);
            Vector2 opravdovaPoziceMysi = opravdovaPoziceKamery + Mouse.GetState().Position.ToVector2() * opravdovaVelikostOkna / hra.velikostOkna.ToVector2();

            if (zbrane[aktualniZbran].aktCas <= 0 && Mouse.GetState().LeftButton == ButtonState.Pressed && hra.IsActive && !hra.komponentaObchod.obchodJeOtevreny)
            {
                Vector2 poziceVystreleni;
                Vector2 poziceZacatkuHlavne;
                float rotaceVystreleni;
                if (opravdovaPoziceMysi.X >= hra.komponentaHrac.poziceHrace.X + KomponentaHrac.VELIKOST_HRACE_X / 2)
                {
                    Vector2 origin = hra.komponentaHrac.poziceHrace + new Vector2(KomponentaHrac.VELIKOST_HRACE_X, KomponentaHrac.VELIKOST_HRACE_Y / 2);
                    poziceVystreleni = hra.komponentaHrac.poziceHrace + new Vector2(KomponentaHrac.VELIKOST_HRACE_X, KomponentaHrac.VELIKOST_HRACE_Y / 2) - zbrane[aktualniZbran].origin * zbrane[aktualniZbran].meritkoVykresleni + zbrane[aktualniZbran].spawnProjektilu * zbrane[aktualniZbran].meritkoVykresleni;
                    rotaceVystreleni = Hra.VypocitejRotaci(origin, poziceVystreleni - new Vector2(5, 0), poziceVystreleni, opravdovaPoziceMysi);
                    poziceZacatkuHlavne = Hra.RotaceBodu(poziceVystreleni - new Vector2(zbrane[aktualniZbran].velikostZbrane.X * zbrane[aktualniZbran].meritkoVykresleni, 0), origin, rotaceVystreleni);
                    poziceVystreleni = Hra.RotaceBodu(poziceVystreleni, origin, rotaceVystreleni);
                }
                else
                {
                    Vector2 origin = hra.komponentaHrac.poziceHrace + new Vector2(0, KomponentaHrac.VELIKOST_HRACE_Y / 2);
                    Vector2 invertovanyOriginZbrane = new Vector2(zbrane[aktualniZbran].origin.X * 2, zbrane[aktualniZbran].velikostZbrane.Y) - zbrane[aktualniZbran].origin;
                    Vector2 invertovanySpawnProjektilu = new Vector2(zbrane[aktualniZbran].spawnProjektilu.X * 2, zbrane[aktualniZbran].velikostZbrane.Y) - zbrane[aktualniZbran].spawnProjektilu;
                    poziceVystreleni = hra.komponentaHrac.poziceHrace + new Vector2(0, KomponentaHrac.VELIKOST_HRACE_Y / 2) - invertovanyOriginZbrane * zbrane[aktualniZbran].meritkoVykresleni + invertovanySpawnProjektilu * zbrane[aktualniZbran].meritkoVykresleni;
                    rotaceVystreleni = Hra.VypocitejRotaci(origin, poziceVystreleni - new Vector2(5, 0), poziceVystreleni, opravdovaPoziceMysi);
                    poziceZacatkuHlavne = Hra.RotaceBodu(poziceVystreleni - new Vector2(zbrane[aktualniZbran].velikostZbrane.X * zbrane[aktualniZbran].meritkoVykresleni, 0), origin, rotaceVystreleni);
                    poziceVystreleni = Hra.RotaceBodu(poziceVystreleni, origin, rotaceVystreleni);
                }

                Point aktualniBlok = new Point((int)((hra.komponentaHrac.poziceHrace.X + KomponentaHrac.VELIKOST_HRACE_X / 2f) / KomponentaMapa.VELIKOST_BLOKU),
                   (int)((hra.komponentaHrac.poziceHrace.Y + KomponentaHrac.VELIKOST_HRACE_Y / 2f) / KomponentaMapa.VELIKOST_BLOKU));

                Point odkudKontrolovat = new Point(Math.Max(0, aktualniBlok.X - 2), Math.Max(0, aktualniBlok.Y - 2));
                Point kamKontrolovat = new Point(Math.Min(KomponentaMapa.VELIKOST_MAPY_X - 1, aktualniBlok.X + 2),
                    Math.Min(KomponentaMapa.VELIKOST_MAPY_Y - 1, aktualniBlok.Y + 2));

                List<Point> poziceNaKontrolu = new List<Point>();
                for (int x = odkudKontrolovat.X; x <= kamKontrolovat.X; x++)
                    for (int y = odkudKontrolovat.Y; y <= kamKontrolovat.Y; y++)
                        poziceNaKontrolu.Add(new Point(x, y));

                bool kolize = false;
                for (int j = 0; j < poziceNaKontrolu.Count && !kolize; j++)
                {
                    int x = poziceNaKontrolu[j].X;
                    int y = poziceNaKontrolu[j].Y;

                    if (hra.komponentaMapa.mapa[x, y].typPole != Pole.TypPole.Zed)
                        continue;

                    Rectangle obdelnikBloku = new Rectangle(x * KomponentaMapa.VELIKOST_BLOKU, y * KomponentaMapa.VELIKOST_BLOKU,
                        KomponentaMapa.VELIKOST_BLOKU, KomponentaMapa.VELIKOST_BLOKU);

                    kolize = Hra.KolizeCarySObdelnikem(poziceZacatkuHlavne, poziceVystreleni, obdelnikBloku);
                }

                Vector2 rotaceVystreleniJakoVector = new Vector2((float)Math.Cos(rotaceVystreleni), (float)Math.Sin(rotaceVystreleni));

                if (!kolize && hra.komponentaMultiplayer.typZarizeni == KomponentaMultiplayer.TypZarizeni.SinglePlayer)
                {
                    zbrane[aktualniZbran].aktCas = zbrane[aktualniZbran].rychlostZbrane;
                    zbrane[aktualniZbran].PouzijZbran(poziceVystreleni, rotaceVystreleniJakoVector, projektily);
                }
                else if (hra.komponentaMultiplayer.typZarizeni == KomponentaMultiplayer.TypZarizeni.Klient)
                {
                    hra.komponentaMultiplayer.PosliInfoONovemProjektilu(opravdovaPoziceMysi, zbrane[aktualniZbran].typZbrane);
                    zbrane[aktualniZbran].aktCas = zbrane[aktualniZbran].rychlostZbrane;
                }
            }

            for (int i = 0; i < projektily.Count; i++)
            {
                projektily[i].PohniSe((float)gameTime.ElapsedGameTime.TotalSeconds);
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
                        {
                            hra.komponentaHrac.penize += monstra[j].penize;
                            monstra.RemoveAt(j);
                        }
                        else
                            monstra[j].zivoty -= projektily[i].zraneni;
                        projektily.RemoveAt(i);
                        i--;
                        break;
                    }
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            hra._spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: hra.komponentaKamera._kamera.GetViewMatrix());

            if (!hra.komponentaMinimapa.jeOtevrena)
            {
                foreach (Projektil projektil in projektily)
                {
                    Vector2 velikostTextury = new Vector2(texturyProjektilu[(int)projektil.typProjektilu].Width, texturyProjektilu[(int)projektil.typProjektilu].Height);
                    hra._spriteBatch.Draw(texturyProjektilu[(int)projektil.typProjektilu], projektil.pozice, null, Color.White,
                        (float)(Math.PI / 2 + Math.Atan2(projektil.smer.Y, projektil.smer.X)), velikostTextury / 2f, projektil.velikost / velikostTextury, SpriteEffects.None, 0);
                }
            }

            hra._spriteBatch.End();

            base.Draw(gameTime);
        }

        public void VykresliZbranUHrace(Zbran.TypZbrane typZbrane, Vector2 pozicehrace, Vector2 poziceMysi)
        {
            Zbran zbran = new Zbran(typZbrane);
            if (poziceMysi.X >= pozicehrace.X + KomponentaHrac.VELIKOST_HRACE_X / 2)
            {
                Vector2 origin = pozicehrace + new Vector2(KomponentaHrac.VELIKOST_HRACE_X, KomponentaHrac.VELIKOST_HRACE_Y / 2);
                Vector2 poziceVystreleni = pozicehrace + new Vector2(KomponentaHrac.VELIKOST_HRACE_X, KomponentaHrac.VELIKOST_HRACE_Y / 2) - zbran.origin * zbran.meritkoVykresleni + zbran.spawnProjektilu * zbran.meritkoVykresleni;
                float rotace = Hra.VypocitejRotaci(origin, poziceVystreleni - new Vector2(5, 0), poziceVystreleni, poziceMysi);
                hra._spriteBatch.Draw(texturyZbrani[(int)zbran.typZbrane], pozicehrace + new Vector2(KomponentaHrac.VELIKOST_HRACE_X, KomponentaHrac.VELIKOST_HRACE_Y / 2),
                    null, Color.White, rotace, zbran.origin, zbran.meritkoVykresleni, SpriteEffects.None, 0);
            }
            else
            {
                Vector2 origin = pozicehrace + new Vector2(0, KomponentaHrac.VELIKOST_HRACE_Y / 2);
                Vector2 invertovanyOriginZbrane = new Vector2(zbran.origin.X * 2, zbran.velikostZbrane.Y) - zbran.origin;
                Vector2 invertovanySpawnProjektilu = new Vector2(zbran.spawnProjektilu.X * 2, zbran.velikostZbrane.Y) - zbran.spawnProjektilu;
                Vector2 poziceVystreleni = pozicehrace + new Vector2(0, KomponentaHrac.VELIKOST_HRACE_Y / 2) - invertovanyOriginZbrane * zbran.meritkoVykresleni + invertovanySpawnProjektilu * zbran.meritkoVykresleni;
                float rotace = Hra.VypocitejRotaci(origin, poziceVystreleni - new Vector2(5, 0), poziceVystreleni, poziceMysi);
                hra._spriteBatch.Draw(texturyZbrani[(int)zbran.typZbrane], pozicehrace + new Vector2(0, KomponentaHrac.VELIKOST_HRACE_Y / 2),
                    null, Color.White, rotace, invertovanyOriginZbrane, zbran.meritkoVykresleni, SpriteEffects.FlipVertically, 0);
            }
        }

        public void PrevedBytyNaProjektily(byte[] prichoziBytyProjektilu)
        {
            byte[] bytyProjektilu = Convert.FromBase64String(Encoding.UTF8.GetString(prichoziBytyProjektilu));

            int pocetMonster = bytyProjektilu[0] * 255 + bytyProjektilu[1];
            List<Projektil> projektilyZBytu = new List<Projektil>(pocetMonster);

            for (int i = 0; i < pocetMonster; i++)
            {
                Projektil projektil = new Projektil();
                projektil.velikost.X = bytyProjektilu[2 + i * 17];
                projektil.velikost.Y = bytyProjektilu[2 + i * 17 + 1];
                projektil.pozice.X = bytyProjektilu[2 + i * 17 + 2] * 255 + bytyProjektilu[2 + i * 17 + 3] + bytyProjektilu[2 + i * 17 + 4] / 255f;
                projektil.pozice.Y = bytyProjektilu[2 + i * 17 + 5] * 255 + bytyProjektilu[2 + i * 17 + 6] + bytyProjektilu[2 + i * 17 + 7] / 255f;
                projektil.smer.X = bytyProjektilu[2 + i * 17 + 8] / 255f;
                if (bytyProjektilu[2 + i * 17 + 9] == 1)
                    projektil.smer.X *= -1;
                projektil.smer.Y = bytyProjektilu[2 + i * 17 + 10] / 255f;
                if (bytyProjektilu[2 + i * 17 + 11] == 1)
                    projektil.smer.Y *= -1;
                projektil.rychlost = bytyProjektilu[2 + i * 17 + 12];
                projektil.zraneni = bytyProjektilu[2 + i * 17 + 13] * 255 * 255 + bytyProjektilu[2 + i * 17 + 14] * 255 + bytyProjektilu[2 + i * 17 + 15];
                projektil.typProjektilu = (Projektil.TypProjektilu)bytyProjektilu[2 + i * 17 + 16];
                projektilyZBytu.Add(projektil);
            }

            noveProjektily = projektilyZBytu;
        }
    }
}
