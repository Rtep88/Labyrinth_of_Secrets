using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static Texture2D texturaSlotuInventare;
        public static Texture2D texturaPenez;

        //Konstanty
        private const int UKAZATEL_ZDRAVI_X_VEL = 450;
        private const int UKAZATEL_ZDRAVI_Y_VEL = 25;
        private const int UKAZATEL_ZDRAVI_OKRAJ_VEL = 6;
        private const int ODSAZENI = 10;
        private const int INVENTAR_SLOT_VEL = 64;
        private const int TEXTURA_PENEZ_VEL = 40;
        const float BEZNE_ROZLISENI_X = 1920;
        const float BEZNE_ROZLISENI_Y = 1080;
        const int VELIKOST_TLACITKA_X = 500;
        const int VELIKOST_TLACITKA_Y = 120;
        const int ODSAZENI_TLACITEK = VELIKOST_TLACITKA_Y + 30;

        //Promenne
        public bool pauza = false;
        private List<Tlacitko> tlacitkaPauzy = new List<Tlacitko>();

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
            texturaPenez = hra.Content.Load<Texture2D>("Images/Coin");

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (hra.NoveZmacknutaKlavesa(Keys.Escape) && !hra.komponentaMinimapa.jeOtevrena &&
                !hra.komponentaKonzole.jeOtevrena && !hra.komponentaObchod.obchodJeOtevreny)
                PrepniStavPauzy();

            float pomerRozliseni = Math.Min(hra.velikostOkna.X / BEZNE_ROZLISENI_X, hra.velikostOkna.Y / BEZNE_ROZLISENI_Y);
            float pomerX = hra.velikostOkna.X / BEZNE_ROZLISENI_X / pomerRozliseni;
            float pomerY = hra.velikostOkna.Y / BEZNE_ROZLISENI_Y / pomerRozliseni;

            Vector2 posunutiMenu = new Vector2((pomerX - 1) / 2 * BEZNE_ROZLISENI_X * pomerRozliseni,
                    (pomerY - 1) / 2 * BEZNE_ROZLISENI_Y * pomerRozliseni);


            for (int i = 0; i < tlacitkaPauzy.Count; i++)
            {
                if (tlacitkaPauzy[i].UpdatujTlacitko(Mouse.GetState(), posunutiMenu, pomerRozliseni, hra.IsActive))
                {
                    if (tlacitkaPauzy[i].data == "pokracovat")
                        PrepniStavPauzy();
                    else
                    {
                        if (hra.komponentaMultiplayer.typZarizeni == KomponentaMultiplayer.TypZarizeni.Klient)
                            hra.komponentaMultiplayer.OdpojSeOdServer();

                        if (hra.komponentaMapa.jmenoSveta != "")
                            hra.komponentaMapa.UlozMapuNaDisk(hra.komponentaMapa.jmenoSveta);

                        hra.komponentaHlavniMenu.PrepniStavMenu(KomponentaHlavniMenu.StavMenu.HlavniMenu);
                    }
                }
            }

            base.Update(gameTime);
        }


        public override void Draw(GameTime gameTime)
        {
            hra._spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            if (!hra.komponentaMinimapa.jeOtevrena)
            {
                float pomerRozliseniX = hra.velikostOkna.X / 1500f;

                for (int i = 0; i < hra.komponentaZbrane.zbrane.Count; i++)
                {
                    int velikostSlotuInventare = (int)(INVENTAR_SLOT_VEL * pomerRozliseniX);
                    Color barvaZbrane = i == hra.komponentaZbrane.aktualniZbran ? Color.White : new Color(190, 190, 190);
                    Zbran zbran = hra.komponentaZbrane.zbrane[i];
                    Rectangle obdelnikVykresleniSlotu = new Rectangle(new Point(ODSAZENI) + new Point(velikostSlotuInventare * i, 0), new Point(velikostSlotuInventare));
                    hra._spriteBatch.Draw(texturaSlotuInventare, obdelnikVykresleniSlotu, barvaZbrane);

                    Texture2D texturaZbrane = KomponentaZbrane.texturyZbrani[(int)zbran.typZbrane];
                    int vetsiRozmer = Math.Max(texturaZbrane.Width, texturaZbrane.Height);
                    float meritko = velikostSlotuInventare / (float)vetsiRozmer / 3 * 2;

                    hra._spriteBatch.Draw(texturaZbrane, new Vector2(ODSAZENI) + new Vector2(velikostSlotuInventare * i, 0) + new Vector2(velikostSlotuInventare) / 2f, null, barvaZbrane, 0,
                        new Vector2(texturaZbrane.Width, texturaZbrane.Height) / 2f, meritko, SpriteEffects.None, 0);

                    hra._spriteBatch.Draw(Hra.pixel, new Vector2(ODSAZENI) + new Vector2(velikostSlotuInventare * i + velikostSlotuInventare / 2f, velikostSlotuInventare / 6f * 5f), null, new Color((int)(100 * barvaZbrane.R / 255f), (int)(100 * barvaZbrane.R / 255f), (int)(100 * barvaZbrane.R / 255f)),
                        0, new Vector2(0.5f), new Vector2(velikostSlotuInventare * 2f / 3, velikostSlotuInventare / 10f), SpriteEffects.None, 0);

                    hra._spriteBatch.Draw(Hra.pixel, new Vector2(ODSAZENI) + new Vector2(velikostSlotuInventare * i + velikostSlotuInventare / 2f - velikostSlotuInventare * 2f / 3 * zbran.aktCas / zbran.rychlostZbrane / 2f,
                        velikostSlotuInventare / 6f * 5f), null, new Color((int)(180 * barvaZbrane.R / 255f), (int)(180 * barvaZbrane.R / 255f), (int)(180 * barvaZbrane.R / 255f)), 0, new Vector2(0.5f), new Vector2(velikostSlotuInventare * 2f / 3 * (zbran.rychlostZbrane - zbran.aktCas) / zbran.rychlostZbrane,
                        velikostSlotuInventare / 10f), SpriteEffects.None, 0);
                }

                //Moje zivoty
                int ukazatelZdraviVelikostX = (int)(UKAZATEL_ZDRAVI_X_VEL * pomerRozliseniX);
                int ukazatelZdraviVelikostY = (int)(UKAZATEL_ZDRAVI_Y_VEL * pomerRozliseniX);
                int ukazatelZdraviVelikostOkraj = (int)(UKAZATEL_ZDRAVI_OKRAJ_VEL * pomerRozliseniX);
                hra._spriteBatch.Draw(Hra.pixel, new Vector2(hra.velikostOkna.X - ukazatelZdraviVelikostX - ukazatelZdraviVelikostOkraj - ODSAZENI, ODSAZENI), null, new Color(40, 40, 40), 0, Vector2.Zero,
                    new Vector2(ukazatelZdraviVelikostX + ukazatelZdraviVelikostOkraj, ukazatelZdraviVelikostY + ukazatelZdraviVelikostOkraj), SpriteEffects.None, 0);
                hra._spriteBatch.Draw(Hra.pixel, new Vector2(hra.velikostOkna.X - ukazatelZdraviVelikostX - ukazatelZdraviVelikostOkraj / 2 - ODSAZENI, ukazatelZdraviVelikostOkraj / 2 + ODSAZENI), null, Color.Black, 0, Vector2.Zero,
                new Vector2(ukazatelZdraviVelikostX, ukazatelZdraviVelikostY), SpriteEffects.None, 0);
                hra._spriteBatch.Draw(Hra.pixel, new Vector2(hra.velikostOkna.X - ukazatelZdraviVelikostX - ukazatelZdraviVelikostOkraj / 2 - ODSAZENI, ukazatelZdraviVelikostOkraj / 2 + ODSAZENI), null, Color.Red, 0, Vector2.Zero,
                    new Vector2(ukazatelZdraviVelikostX, ukazatelZdraviVelikostY) * new Vector2(hra.komponentaHrac.zivoty / (float)KomponentaHrac.MAX_ZIVOTY, 1), SpriteEffects.None, 0);

                //Vykresleni penez
                int texturaPenezVelikost = (int)(TEXTURA_PENEZ_VEL * pomerRozliseniX);
                string pocetPenez = hra.komponentaHrac.penize.ToString();
                hra.VykresliTextSOkrajem(Hra.pixeloidSans, new Vector2(hra.velikostOkna.X - texturaPenezVelikost - ODSAZENI * 2 - Hra.pixeloidSans.MeasureString(pocetPenez).X * 0.1f * pomerRozliseniX,
                    ODSAZENI * 2 + ukazatelZdraviVelikostY + ukazatelZdraviVelikostOkraj), pocetPenez, 0.1f * pomerRozliseniX, Color.White, Color.Black, 0.08f, 16, true);
                hra._spriteBatch.Draw(texturaPenez, new Vector2(hra.velikostOkna.X - texturaPenezVelikost - ODSAZENI, ODSAZENI * 2 + ukazatelZdraviVelikostY + ukazatelZdraviVelikostOkraj),
                    null, Color.White, 0, Vector2.Zero, new Vector2(texturaPenezVelikost, texturaPenezVelikost) / texturaPenez.Bounds.Size.ToVector2(), SpriteEffects.None, 0);
            }

            if (pauza)
            {
                hra._spriteBatch.Draw(Hra.pixel, new Rectangle(Point.Zero, hra.velikostOkna), new Color(20, 20, 20, 140));

                float pomerRozliseni = Math.Min(hra.velikostOkna.X / BEZNE_ROZLISENI_X, hra.velikostOkna.Y / BEZNE_ROZLISENI_Y);
                float pomerX = hra.velikostOkna.X / BEZNE_ROZLISENI_X / pomerRozliseni;
                float pomerY = hra.velikostOkna.Y / BEZNE_ROZLISENI_Y / pomerRozliseni;

                Vector2 posunutiMenu = new Vector2((pomerX - 1) / 2 * BEZNE_ROZLISENI_X * pomerRozliseni,
                        (pomerY - 1) / 2 * BEZNE_ROZLISENI_Y * pomerRozliseni);

                //Vykresleni tlacitek
                foreach (Tlacitko tlacitko in tlacitkaPauzy)
                    tlacitko.VykresliTlacitko(hra, posunutiMenu, pomerRozliseni);
            }

            hra._spriteBatch.End();

            base.Draw(gameTime);
        }

        public void PrepniStavPauzy()
        {
            pauza = !pauza;
            tlacitkaPauzy.Clear();

            if (pauza)
            {
                tlacitkaPauzy.Add(new Tlacitko(new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) / 2 - new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y) / 2,
                    new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y), "Pokračovat", new Color(110, 110, 110), "pokracovat", 6f));
                tlacitkaPauzy.Add(new Tlacitko(new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) / 2 - new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y) / 2 + new Vector2(0, ODSAZENI_TLACITEK),
                    new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y),
                    hra.komponentaMultiplayer.typZarizeni == KomponentaMultiplayer.TypZarizeni.SinglePlayer ? "Odejít" : "Odpojit se",
                    new Color(110, 110, 110), "odejit", 6f));
            }
        }
    }
}
