using System;
using System.Collections.Generic;
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
    public class KomponentaObchod : DrawableGameComponent
    {
        //Zaklad
        private Hra hra;

        //Konstanty
        int VELIKOST_OBCHODU_X = 1400;
        int VELIKOST_OBCHODU_Y = 800;

        //Promenne
        public bool obchodJeOtevreny = false;
        List<Tlacitko> tlacitkaObchodu = new List<Tlacitko>();

        public KomponentaObchod(Hra hra) : base(hra)
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
            //Nacitani akt bloku
            Point aktualniKostka = (hra.komponentaHrac.poziceHrace + new Vector2(KomponentaHrac.VELIKOST_HRACE_X, KomponentaHrac.VELIKOST_HRACE_Y) / 2).ToPoint() / new Point(KomponentaMapa.VELIKOST_BLOKU);
            Pole.TypPole aktualniTypKostky = (Pole.TypPole)int.MaxValue;
            if (aktualniKostka.X < KomponentaMapa.VELIKOST_MAPY_X && aktualniKostka.X >= 0 && aktualniKostka.Y < KomponentaMapa.VELIKOST_MAPY_Y && aktualniKostka.Y >= 0)
                aktualniTypKostky = hra.komponentaMapa.mapa[aktualniKostka.X, aktualniKostka.Y].typPole;

            //Vypocet pozice obchodu
            float pomerRozliseni = Math.Min(hra.velikostOkna.X / 1920f, hra.velikostOkna.Y / 1080f);
            Vector2 poziceObchodu = hra.velikostOkna.ToVector2() / 2 - new Vector2(VELIKOST_OBCHODU_X, VELIKOST_OBCHODU_Y) / 2 * pomerRozliseni;
            Vector2 velikostObchodu = new Vector2(VELIKOST_OBCHODU_X, VELIKOST_OBCHODU_Y) * pomerRozliseni;

            //Otevirani obchodu
            if ((aktualniTypKostky == Pole.TypPole.Obchod || aktualniTypKostky == Pole.TypPole.Obchodnik) && hra.NoveZmacknutaKlavesa(Keys.E))
            {
                obchodJeOtevreny = !obchodJeOtevreny;
                if (obchodJeOtevreny)
                {
                    int i = 0;
                    for (int y = 0; y < 2; y++)
                        for (int x = 0; x < 2; x++)
                        {
                            tlacitkaObchodu.Add(new Tlacitko(new Vector2(310, 280) + new Vector2(x * VELIKOST_OBCHODU_X / 2, y * VELIKOST_OBCHODU_Y / 2), new Vector2(300, 70), "Koupit", Color.LimeGreen, i.ToString(), 6f));
                            i++;
                        }
                }
                else
                    tlacitkaObchodu.Clear();
            }

            //Update obchodua
            if (obchodJeOtevreny)
            {
                MouseState stavMysi = Mouse.GetState();
                foreach (Tlacitko tlacitko in tlacitkaObchodu)
                {
                    if (tlacitko.UpdatujTlacitko(stavMysi, poziceObchodu, pomerRozliseni))
                    {
                        int i = int.Parse(tlacitko.data);
                        Zbran zbran = hra.komponentaZbrane.zbrane[i];

                        if (zbran.levelZbrane < 10)
                        {
                            hra.komponentaHrac.penize -= zbran.cenaUpgradu;
                            zbran.levelZbrane++;
                            zbran.zraneniZbrane = zbran.zraneniZbrane * zbran.levelZbrane / (zbran.levelZbrane - 1);//dodelat
                            zbran.cenaUpgradu = (int)(zbran.cenaUpgradu * 1.75f);
                        }
                    }
                }
            }

            base.Update(gameTime);
        }


        public override void Draw(GameTime gameTime)
        {
            hra._spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            float pomerRozliseni = Math.Min(hra.velikostOkna.X / 1920f, hra.velikostOkna.Y / 1080f);

            //Nacitani akt bloku
            Point aktualniKostka = (hra.komponentaHrac.poziceHrace + new Vector2(KomponentaHrac.VELIKOST_HRACE_X, KomponentaHrac.VELIKOST_HRACE_Y) / 2).ToPoint() / new Point(KomponentaMapa.VELIKOST_BLOKU);
            Pole.TypPole aktualniTypKostky = (Pole.TypPole)int.MaxValue;
            if (aktualniKostka.X < KomponentaMapa.VELIKOST_MAPY_X && aktualniKostka.X >= 0 && aktualniKostka.Y < KomponentaMapa.VELIKOST_MAPY_Y && aktualniKostka.Y >= 0)
                aktualniTypKostky = hra.komponentaMapa.mapa[aktualniKostka.X, aktualniKostka.Y].typPole;

            //Jsem v obchode
            if (aktualniTypKostky == Pole.TypPole.Obchod || aktualniTypKostky == Pole.TypPole.Obchodnik)
            {
                if (!obchodJeOtevreny)//Navod jak otevrit obchod
                {
                    string textObchodu = "Zmáčkněte E pro otevření obchodu";
                    hra.VykresliTextSOkrajem(Hra.pixeloidSans, hra.velikostOkna.ToVector2() / 2 - Hra.pixeloidSans.MeasureString(textObchodu) * 0.1f / 2 * pomerRozliseni, textObchodu, 0.1f * pomerRozliseni, Color.White, Color.Black, 0.07f, 8, true);
                }
                else//Otevreny obchod
                {
                    Vector2 poziceObchodu = hra.velikostOkna.ToVector2() / 2 - new Vector2(VELIKOST_OBCHODU_X, VELIKOST_OBCHODU_Y) / 2 * pomerRozliseni;
                    hra._spriteBatch.Draw(Hra.pixel, poziceObchodu, null, new Color(40, 40, 40, 180), 0, Vector2.Zero, new Vector2(VELIKOST_OBCHODU_X, VELIKOST_OBCHODU_Y) * pomerRozliseni, SpriteEffects.None, 0);

                    foreach (Tlacitko tlacitko in tlacitkaObchodu)
                        tlacitko.VykresliTlacitko(hra, poziceObchodu, pomerRozliseni);

                    int i = 0;
                    for (int y = 0; y < 2; y++)
                    {
                        for (int x = 0; x < 2; x++)
                        {
                            Point relativniPozice = poziceObchodu.ToPoint() + (new Vector2(x * VELIKOST_OBCHODU_X / 2, y * VELIKOST_OBCHODU_Y / 2) * pomerRozliseni).ToPoint();
                            Zbran zbran = hra.komponentaZbrane.zbrane[i];

                            //Vykresleni ikony zbrane
                            Texture2D texturaZbrane = KomponentaZbrane.texturyZbrani[(int)zbran.typZbrane];
                            int vetsiRozmer = Math.Max(texturaZbrane.Width, texturaZbrane.Height);
                            float pomerVelikosti = 200f / vetsiRozmer;
                            Vector2 velikost = texturaZbrane.Bounds.Size.ToVector2() * pomerVelikosti;
                            hra._spriteBatch.Draw(KomponentaZbrane.texturyZbrani[(int)zbran.typZbrane],
                                new Rectangle(relativniPozice + ((new Vector2(100, 50) + new Vector2(100) - velikost / 2f) * pomerRozliseni).ToPoint(), (velikost * pomerRozliseni).ToPoint()), Color.White);

                            //Vykresleni levelu
                            for (int j = 0; j < 10; j++)
                            {
                                if (zbran.levelZbrane >= 10 - j)
                                    hra._spriteBatch.Draw(Hra.pixel, new Rectangle(relativniPozice + (new Vector2(370, 60 + j * 18) * pomerRozliseni).ToPoint(),
                                        (new Vector2(120, 15) * pomerRozliseni).ToPoint()), Color.LightGray);
                                else
                                    hra._spriteBatch.Draw(Hra.pixel, new Rectangle(relativniPozice + (new Vector2(370, 60 + j * 18) * pomerRozliseni).ToPoint(),
                                        (new Vector2(120, 15) * pomerRozliseni).ToPoint()), Color.Gray);
                            }

                            //Vykresleni ikony penez
                            hra._spriteBatch.Draw(KomponentaMenu.texturaPenez, new Rectangle(relativniPozice + (new Vector2(220, 280) * pomerRozliseni).ToPoint(),
                                (new Vector2(70) * pomerRozliseni).ToPoint()), Color.LightGray);

                            //Vykresleni ceny
                            string textCeny = zbran.levelZbrane == 10 ? "MAX" : zbran.cenaUpgradu.ToString();
                            float pomerVykresleniTextu = 70f / Hra.pixeloidSans.MeasureString(textCeny).Y;
                            float velikostTextuX = pomerVykresleniTextu * Hra.pixeloidSans.MeasureString(textCeny).X;
                            hra.VykresliTextSOkrajem(Hra.pixeloidSans, relativniPozice.ToVector2() + new Vector2(200 - velikostTextuX, 280) * pomerRozliseni, textCeny, pomerVykresleniTextu * pomerRozliseni, Color.White, Color.Black, 0.07f, 8, true);
                            i++;
                        }
                    }
                }
            }

            hra._spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
