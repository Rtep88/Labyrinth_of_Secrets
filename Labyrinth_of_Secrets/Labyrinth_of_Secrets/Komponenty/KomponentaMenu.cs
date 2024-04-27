using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Labyrinth_of_Secrets
{
    public class KomponentaMenu : DrawableGameComponent
    {
        //Zaklad
        private Hra hra;

        //Textury
        Texture2D texturaSlotuInventare;
        Texture2D texturePenez;

        //Konstanty
        private const int UKAZATEL_ZDRAVI_X_VEL = 350;
        private const int UKAZATEL_ZDRAVI_Y_VEL = 25;
        private const int UKAZATEL_ZDRAVI_OKRAJ_VEL = 6;
        private const int ODSAZENI = 10;
        private const int INVENTAR_SLOT_VEL = 64;
        private const int TEXTURA_PENEZ_VEL = 40;

        //Promenne
        public bool obchodJeOtevreny = false;
        List<Tlacitko> tlacitkaObchodu = new List<Tlacitko>();

        public KomponentaMenu(Hra hra) : base(hra)
        {
            this.hra = hra;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            texturaSlotuInventare = hra.Content.Load<Texture2D>("Images/Inventory_slot");
            texturePenez = hra.Content.Load<Texture2D>("Images/Coin");

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            //Nacitani akt bloku
            Point aktualniKostka = (hra.komponentaHrac.poziceHrace + new Vector2(KomponentaHrac.VELIKOST_HRACE_X, KomponentaHrac.VELIKOST_HRACE_Y) / 2).ToPoint() / new Point(KomponentaMapa.VELIKOST_BLOKU);
            Pole.TypPole aktualniTypKostky = (Pole.TypPole)int.MaxValue;
            if (aktualniKostka.X < KomponentaMapa.VELIKOST_MAPY_X && aktualniKostka.X >= 0 && aktualniKostka.Y < KomponentaMapa.VELIKOST_MAPY_Y && aktualniKostka.Y >= 0)
                aktualniTypKostky = hra.komponentaMapa.mapa[aktualniKostka.X, aktualniKostka.Y].typPole;

            //Otevirani obchodu
            if ((aktualniTypKostky == Pole.TypPole.Obchod || aktualniTypKostky == Pole.TypPole.Obchodnik) && hra.NoveZmacknutaKlavesa(Keys.E))
            {
                obchodJeOtevreny = !obchodJeOtevreny;
                if (obchodJeOtevreny)
                {
                    tlacitkaObchodu.Add(new Tlacitko(new Vector2(50, 50), new Vector2(300, 200), "Koupit", Color.LimeGreen, "1"));
                    tlacitkaObchodu.Add(new Tlacitko(new Vector2(450, 50), new Vector2(300, 200), "Koupit", Color.LimeGreen, "1"));
                    tlacitkaObchodu.Add(new Tlacitko(new Vector2(50, 350), new Vector2(300, 200), "Koupit", Color.LimeGreen, "1"));
                    tlacitkaObchodu.Add(new Tlacitko(new Vector2(450, 350), new Vector2(300, 200), "Koupit", Color.LimeGreen, "1"));
                }
                else
                    tlacitkaObchodu.Clear();
            }

            //Update obchodu
            if (obchodJeOtevreny)
            {
                Vector2 poziceObchodu = hra.velikostOkna.ToVector2() / 2 - new Vector2(400, 300);

                MouseState stavMysi = Mouse.GetState();
                foreach (Tlacitko tlacitko in tlacitkaObchodu)
                    tlacitko.UpdatujTlacitko(stavMysi, poziceObchodu);
            }

            base.Update(gameTime);
        }


        public override void Draw(GameTime gameTime)
        {
            hra._spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            for (int i = 0; i < hra.komponentaZbrane.zbrane.Count; i++)
            {
                Color barvaZbrane = i == hra.komponentaZbrane.aktualniZbran ? Color.White : new Color(190, 190, 190);
                Zbran zbran = hra.komponentaZbrane.zbrane[i];
                Rectangle obdelnikVykresleniSlotu = new Rectangle(new Point(ODSAZENI) + new Point(INVENTAR_SLOT_VEL * i, 0), new Point(INVENTAR_SLOT_VEL));
                hra._spriteBatch.Draw(texturaSlotuInventare, obdelnikVykresleniSlotu, barvaZbrane);

                Texture2D texturaZbrane = KomponentaZbrane.texturyZbrani[(int)zbran.typZbrane];
                int vetsiRozmer = Math.Max(texturaZbrane.Width, texturaZbrane.Height);
                float meritko = INVENTAR_SLOT_VEL / (float)vetsiRozmer / 3 * 2;

                hra._spriteBatch.Draw(texturaZbrane, new Vector2(ODSAZENI) + new Vector2(INVENTAR_SLOT_VEL * i, 0) + new Vector2(INVENTAR_SLOT_VEL) / 2f, null, barvaZbrane, 0,
                    new Vector2(texturaZbrane.Width, texturaZbrane.Height) / 2f, meritko, SpriteEffects.None, 0);

                hra._spriteBatch.Draw(Hra.pixel, new Vector2(ODSAZENI) + new Vector2(INVENTAR_SLOT_VEL * i + INVENTAR_SLOT_VEL / 2f, INVENTAR_SLOT_VEL / 6f * 5f), null, new Color((int)(100 * barvaZbrane.R / 255f), (int)(100 * barvaZbrane.R / 255f), (int)(100 * barvaZbrane.R / 255f)),
                    0, new Vector2(0.5f), new Vector2(INVENTAR_SLOT_VEL * 2f / 3, INVENTAR_SLOT_VEL / 10f), SpriteEffects.None, 0);

                hra._spriteBatch.Draw(Hra.pixel, new Vector2(ODSAZENI) + new Vector2(INVENTAR_SLOT_VEL * i + INVENTAR_SLOT_VEL / 2f - INVENTAR_SLOT_VEL * 2f / 3 * zbran.aktCas / zbran.rychlostZbrane / 2f,
                    INVENTAR_SLOT_VEL / 6f * 5f), null, new Color((int)(180 * barvaZbrane.R / 255f), (int)(180 * barvaZbrane.R / 255f), (int)(180 * barvaZbrane.R / 255f)), 0, new Vector2(0.5f), new Vector2(INVENTAR_SLOT_VEL * 2f / 3 * (zbran.rychlostZbrane - zbran.aktCas) / zbran.rychlostZbrane,
                    INVENTAR_SLOT_VEL / 10f), SpriteEffects.None, 0);
            }

            //Moje zivoty
            hra._spriteBatch.Draw(Hra.pixel, new Vector2(hra.velikostOkna.X - UKAZATEL_ZDRAVI_X_VEL - UKAZATEL_ZDRAVI_OKRAJ_VEL - ODSAZENI, ODSAZENI), null, new Color(40, 40, 40), 0, Vector2.Zero,
                new Vector2(UKAZATEL_ZDRAVI_X_VEL + UKAZATEL_ZDRAVI_OKRAJ_VEL, UKAZATEL_ZDRAVI_Y_VEL + UKAZATEL_ZDRAVI_OKRAJ_VEL), SpriteEffects.None, 0);
            hra._spriteBatch.Draw(Hra.pixel, new Vector2(hra.velikostOkna.X - UKAZATEL_ZDRAVI_X_VEL - UKAZATEL_ZDRAVI_OKRAJ_VEL / 2 - ODSAZENI, UKAZATEL_ZDRAVI_OKRAJ_VEL / 2 + ODSAZENI), null, Color.Black, 0, Vector2.Zero,
            new Vector2(UKAZATEL_ZDRAVI_X_VEL, UKAZATEL_ZDRAVI_Y_VEL), SpriteEffects.None, 0);
            hra._spriteBatch.Draw(Hra.pixel, new Vector2(hra.velikostOkna.X - UKAZATEL_ZDRAVI_X_VEL - UKAZATEL_ZDRAVI_OKRAJ_VEL / 2 - ODSAZENI, UKAZATEL_ZDRAVI_OKRAJ_VEL / 2 + ODSAZENI), null, Color.Red, 0, Vector2.Zero,
                new Vector2(UKAZATEL_ZDRAVI_X_VEL, UKAZATEL_ZDRAVI_Y_VEL) * new Vector2(hra.komponentaHrac.zivoty / (float)KomponentaHrac.MAX_ZIVOTY, 1), SpriteEffects.None, 0);

            //Vykresleni penez
            string pocetPenez = hra.komponentaHrac.penize.ToString();
            hra.VykresliTextSOkrajem(Hra.pixeloidSans, new Vector2(hra.velikostOkna.X - TEXTURA_PENEZ_VEL - ODSAZENI * 2 - Hra.pixeloidSans.MeasureString(pocetPenez).X * 0.1f,
                ODSAZENI * 2 + UKAZATEL_ZDRAVI_Y_VEL + UKAZATEL_ZDRAVI_OKRAJ_VEL), pocetPenez, 0.1f, Color.White, Color.Black, 0.08f, 16, true);
            hra._spriteBatch.Draw(texturePenez, new Vector2(hra.velikostOkna.X - TEXTURA_PENEZ_VEL - ODSAZENI, ODSAZENI * 2 + UKAZATEL_ZDRAVI_Y_VEL + UKAZATEL_ZDRAVI_OKRAJ_VEL),
                null, Color.White, 0, Vector2.Zero, new Vector2(TEXTURA_PENEZ_VEL, TEXTURA_PENEZ_VEL) / texturePenez.Bounds.Size.ToVector2(), SpriteEffects.None, 0);

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
                    hra.VykresliTextSOkrajem(Hra.pixeloidSans, hra.velikostOkna.ToVector2() / 2 - Hra.pixeloidSans.MeasureString(textObchodu) * 0.1f / 2, textObchodu, 0.1f, Color.White, Color.Black, 0.07f, 8, true);
                }
                else//Otevreny obchod
                {
                    Vector2 poziceObchodu = hra.velikostOkna.ToVector2() / 2 - new Vector2(400, 300);
                    hra._spriteBatch.Draw(Hra.pixel, poziceObchodu, null, new Color(40, 40, 40, 180), 0, Vector2.Zero, new Vector2(800, 600), SpriteEffects.None, 0);

                    foreach (Tlacitko tlacitko in tlacitkaObchodu)
                        tlacitko.VykresliTlacitko(hra, poziceObchodu);
                }
            }

            hra._spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
