using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
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

        //Struktury

        //Konstanty
        const float rychlostUtoceni = 0.2f;

        //Promenne
        public List<Projektil> projektily = new List<Projektil>();
        private List<Projektil> noveProjektily = null;
        private float casZautoceni = 0;

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
            Texture2D pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new Color[] { Color.White });
            texturyProjektilu.Add(pixel);

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (hra.komponentaMultiplayer.typZarizeni == KomponentaMultiplayer.TypZarizeni.Klient && noveProjektily != null)
            {
                projektily = noveProjektily;
                noveProjektily = null;
            }

            if (casZautoceni > 0)
                casZautoceni -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            Kamera _kamera = hra.komponentaKamera._kamera;
            Vector2 opravdovaPoziceKamery = new Vector2(-_kamera.GetViewMatrix().Translation.X / _kamera.zoom, -_kamera.GetViewMatrix().Translation.Y / _kamera.zoom);
            Vector2 opravdovaVelikostOkna = new Vector2(hra.velikostOkna.X / _kamera.zoom, hra.velikostOkna.Y / _kamera.zoom);
            Vector2 opravdovaPoziceMysi = opravdovaPoziceKamery + Mouse.GetState().Position.ToVector2() * opravdovaVelikostOkna / hra.velikostOkna.ToVector2();

            if (casZautoceni <= 0 && Mouse.GetState().LeftButton == ButtonState.Pressed && hra.IsActive)
            {
                Vector2 stredHrace = hra.komponentaHrac.poziceHrace + new Vector2(KomponentaHrac.VELIKOST_HRACE_X, KomponentaHrac.VELIKOST_HRACE_Y) / 2f;
                casZautoceni = rychlostUtoceni;

                if (hra.komponentaMultiplayer.typZarizeni == KomponentaMultiplayer.TypZarizeni.SinglePlayer)
                    projektily.Add(new Projektil(stredHrace, Vector2.Normalize(opravdovaPoziceMysi - stredHrace)));
                else
                    hra.komponentaMultiplayer.PosliInfoONovemProjektilu(Vector2.Normalize(opravdovaPoziceMysi - stredHrace));
            }

            for (int i = 0; i < projektily.Count; i++)
            {
                projektily[i].pozice += projektily[i].smer * projektily[i].rychlost * (float)gameTime.ElapsedGameTime.TotalSeconds;
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
                        monstra.RemoveAt(j);
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
            foreach (Projektil projektil in projektily)
            {
                Vector2 velikostTextury = new Vector2(texturyProjektilu[(int)projektil.typProjektilu].Width, texturyProjektilu[(int)projektil.typProjektilu].Height);
                hra._spriteBatch.Draw(texturyProjektilu[(int)projektil.typProjektilu], projektil.pozice, null, Color.Red,
                (float)(Math.PI / 2 + Math.Atan2(projektil.smer.Y, projektil.smer.X)), velikostTextury / 2f, projektil.velikost / velikostTextury, SpriteEffects.None, 0);
            }
            hra._spriteBatch.End();

            base.Draw(gameTime);
        }

        public void PrevedBytyNaProjektily(byte[] prichoziBytyProjektilu)
        {
            byte[] bytyProjektilu = Convert.FromBase64String(Encoding.UTF8.GetString(prichoziBytyProjektilu));

            int pocetMonster = bytyProjektilu[0] * 255 + bytyProjektilu[1];
            List<Projektil> projektilyZBytu = new List<Projektil>(pocetMonster);

            for (int i = 0; i < pocetMonster; i++)
            {
                Projektil projektil = new Projektil();
                projektil.velikost.X = bytyProjektilu[2 + i * 14];
                projektil.velikost.Y = bytyProjektilu[2 + i * 14 + 1];
                projektil.pozice.X = bytyProjektilu[2 + i * 14 + 2] * 255 + bytyProjektilu[2 + i * 14 + 3] + bytyProjektilu[2 + i * 14 + 4] / 255f;
                projektil.pozice.Y = bytyProjektilu[2 + i * 14 + 5] * 255 + bytyProjektilu[2 + i * 14 + 6] + bytyProjektilu[2 + i * 14 + 7] / 255f;
                projektil.smer.X = bytyProjektilu[2 + i * 14 + 8] / 255f;
                if (bytyProjektilu[2 + i * 14 + 9] == 1)
                    projektil.smer.X *= -1;
                projektil.smer.Y = bytyProjektilu[2 + i * 14 + 10] / 255f;
                if (bytyProjektilu[2 + i * 14 + 11] == 1)
                    projektil.smer.Y *= -1;
                projektil.rychlost = bytyProjektilu[2 + i * 14 + 12];
                projektil.typProjektilu = (Projektil.TypProjektilu)bytyProjektilu[2 + i * 14 + 13];
                projektilyZBytu.Add(projektil);
            }

            noveProjektily = projektilyZBytu;
        }
    }
}
