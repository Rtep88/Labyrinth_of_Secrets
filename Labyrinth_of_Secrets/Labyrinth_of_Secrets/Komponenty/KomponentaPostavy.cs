using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Reflection.Metadata;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Labyrinth_of_Secrets
{
    public class KomponentaPostavy : DrawableGameComponent
    {
        //Zaklad
        private Hra hra;

        //konstanty
        readonly string cestaKSouboruKDialogum = Path.Combine("Resources", "dialog.cz");

        //Promenne
        private string jmenoProdavace = "";
        public List<Postava> postavy = new List<Postava>();

        public KomponentaPostavy(Hra hra) : base(hra)
        {
            this.hra = hra;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }


        public override void Draw(GameTime gameTime)
        {
            hra._spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: hra.komponentaKamera._kamera.GetViewMatrix());

            Texture2D texturaHrace = KomponentaHrac.texturaHrace;

            for (int i = 0; i < postavy.Count; i++)
            {
                hra._spriteBatch.Draw(texturaHrace, postavy[i].pozice, null, Color.Blue, 0, Vector2.Zero,
                    new Vector2((float)postavy[i].velikost.X / texturaHrace.Width, (float)postavy[i].velikost.Y / texturaHrace.Height), SpriteEffects.None, 0);

                hra.komponentaHrac.VykresliJmenovku(postavy[i].pozice, postavy[i].velikost.ToVector2(), postavy[i].jmeno);
            }

            hra._spriteBatch.End();

            base.Draw(gameTime);
        }

        public void NactiPostavy()
        {
            postavy.Clear();
            try
            {
                string[] argumenty = File.ReadAllText(cestaKSouboruKDialogum).Replace("\r", "").Replace("\n", "").Split('^');
                jmenoProdavace = argumenty[0];
                for (int i = 1; i + 2 < argumenty.Length; i += 3)
                {
                    string jmenoNpccka = argumenty[i];

                    float vzdalenostOdStartu = float.Parse(argumenty[i + 1]) % 1;
                    Vector2 pozice = hra.komponentaMapa.cestaZeStartuDoCile[(int)((hra.komponentaMapa.cestaZeStartuDoCile.Count - 1) * vzdalenostOdStartu)].ToVector2();
                    List<Vector2> mozneRelativniPozice = new List<Vector2>();

                    if (pozice.X > 0 && hra.komponentaMapa.mapa[(int)pozice.X - 1, (int)pozice.Y].typPole == Pole.TypPole.Zed)
                        mozneRelativniPozice.Add(new Vector2(1, KomponentaMapa.VELIKOST_BLOKU / 2f - KomponentaHrac.VELIKOST_HRACE_Y / 2f));
                    if (pozice.X < KomponentaMapa.VELIKOST_MAPY_X - 1 && hra.komponentaMapa.mapa[(int)pozice.X + 1, (int)pozice.Y].typPole == Pole.TypPole.Zed)
                        mozneRelativniPozice.Add(new Vector2(KomponentaMapa.VELIKOST_BLOKU - 1 - KomponentaHrac.VELIKOST_HRACE_X, KomponentaMapa.VELIKOST_BLOKU / 2f - KomponentaHrac.VELIKOST_HRACE_Y / 2f));
                    if (pozice.Y > 0 && hra.komponentaMapa.mapa[(int)pozice.X, (int)pozice.Y - 1].typPole == Pole.TypPole.Zed)
                        mozneRelativniPozice.Add(new Vector2(KomponentaMapa.VELIKOST_BLOKU / 2f - KomponentaHrac.VELIKOST_HRACE_X / 2f, 1));
                    if (pozice.Y < KomponentaMapa.VELIKOST_MAPY_Y - 1 && hra.komponentaMapa.mapa[(int)pozice.X, (int)pozice.Y + 1].typPole == Pole.TypPole.Zed)
                        mozneRelativniPozice.Add(new Vector2(KomponentaMapa.VELIKOST_BLOKU / 2f - KomponentaHrac.VELIKOST_HRACE_X / 2f, KomponentaMapa.VELIKOST_BLOKU - 1 - KomponentaHrac.VELIKOST_HRACE_Y));

                    if (mozneRelativniPozice.Count == 0)
                    {
                        mozneRelativniPozice.Add(new Vector2(1, 1));
                        mozneRelativniPozice.Add(new Vector2(1, KomponentaMapa.VELIKOST_BLOKU - 1 - KomponentaHrac.VELIKOST_HRACE_Y));
                        mozneRelativniPozice.Add(new Vector2(KomponentaMapa.VELIKOST_BLOKU - 1 - KomponentaHrac.VELIKOST_HRACE_Y));
                        mozneRelativniPozice.Add(new Vector2(KomponentaMapa.VELIKOST_BLOKU - 1 - KomponentaHrac.VELIKOST_HRACE_Y,
                            KomponentaMapa.VELIKOST_BLOKU - 1 - KomponentaHrac.VELIKOST_HRACE_Y));
                    }
                    pozice = pozice * KomponentaMapa.VELIKOST_BLOKU + mozneRelativniPozice[hra.rnd.Next(0, mozneRelativniPozice.Count)];

                    List<List<string>> dialogy = new List<List<string>>();
                    string[] obrazovky = argumenty[i + 2].Split(';');

                    foreach (string obrazovka in obrazovky)
                    {
                        dialogy.Add(obrazovka.Split('&').ToList());
                    }

                    postavy.Add(new Postava(jmenoNpccka, pozice, dialogy));
                }
            }
            catch
            {
                throw new Exception("Dialogs are in bad format!");
            }

            NactiPostavyObchodu();
        }

        private void NactiPostavyObchodu()
        {
            for (int y = 0; y < KomponentaMapa.VELIKOST_MAPY_Y; y++)
            {
                for (int x = 0; x < KomponentaMapa.VELIKOST_MAPY_X; x++)
                {
                    if (hra.komponentaMapa.mapa[x, y].typPole == Pole.TypPole.Obchodnik)
                        postavy.Add(new Postava(jmenoProdavace, new Vector2(x + 0.5f, y + 0.5f) * KomponentaMapa.VELIKOST_BLOKU
                            - new Vector2(KomponentaHrac.VELIKOST_HRACE_X, KomponentaHrac.VELIKOST_HRACE_Y) / 2, true));
                }
            }
        }
    }
}
