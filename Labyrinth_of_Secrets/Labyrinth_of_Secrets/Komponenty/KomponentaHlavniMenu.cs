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
    public class KomponentaHlavniMenu : DrawableGameComponent
    {
        //Zaklad
        private Hra hra;

        //Textury

        //Konstanty
        const int VELIKOST_TLACITKA_X = 500;
        const int VELIKOST_TLACITKA_Y = 120;
        const int ODSAZENI_TLACITEK = VELIKOST_TLACITKA_Y + 30;
        const int VELIKOST_TEXTBOXU_X = 800;
        const int VELIKOST_TEXTBOXU_Y = 80;
        const int ODSAZENI_TEXTBOXU = VELIKOST_TEXTBOXU_Y + 30;
        const float BEZNE_ROZLISENI_X = 1920;
        const float BEZNE_ROZLISENI_Y = 1080;

        //Enumy
        public enum StavMenu
        {
            HlavniMenu,
            VyberSveta,
            Nastaveni,
            Multiplayer,
            Hra
        }

        //Promenne
        private List<Tlacitko> tlacitkaMenu = new List<Tlacitko>();
        private List<Textbox> textboxyMenu = new List<Textbox>();
        private StavMenu stavMenu = StavMenu.HlavniMenu;

        public KomponentaHlavniMenu(Hra hra) : base(hra)
        {
            this.hra = hra;
        }

        public override void Initialize()
        {
            PrepniStavMenu(StavMenu.HlavniMenu);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            float pomerRozliseni = Math.Min(hra.velikostOkna.X / BEZNE_ROZLISENI_X, hra.velikostOkna.Y / BEZNE_ROZLISENI_Y);
            float pomerX = hra.velikostOkna.X / BEZNE_ROZLISENI_X / pomerRozliseni;
            float pomerY = hra.velikostOkna.Y / BEZNE_ROZLISENI_Y / pomerRozliseni;

            Vector2 posunutiMenu = new Vector2((pomerX - 1) / 2 * BEZNE_ROZLISENI_X * pomerRozliseni,
                    (pomerY - 1) / 2 * BEZNE_ROZLISENI_Y * pomerRozliseni);

            //Aktualizace tlacitek
            for (int i = 0; i < tlacitkaMenu.Count; i++)
            {
                if (tlacitkaMenu[i].UpdatujTlacitko(Mouse.GetState(), posunutiMenu, pomerRozliseni))
                {
                    switch (stavMenu)
                    {
                        case StavMenu.HlavniMenu:
                            if (tlacitkaMenu[i].data == "singleplayer")
                                PrepniStavMenu(StavMenu.Hra);
                            else if (tlacitkaMenu[i].data == "multiplayer")
                                PrepniStavMenu(StavMenu.Multiplayer);
                            else if (tlacitkaMenu[i].data == "nastaveni")
                                PrepniStavMenu(StavMenu.Nastaveni);
                            else if (tlacitkaMenu[i].data == "odejit")
                                hra.VypniHru();
                            break;
                        case StavMenu.Nastaveni:
                            if (tlacitkaMenu[i].data == "fullscreen")
                            {
                                hra.fullscreen = !hra.fullscreen;
                                hra.NastavRozliseni();
                            }
                            else if (tlacitkaMenu[i].data == "zpatky")
                                PrepniStavMenu(StavMenu.HlavniMenu);
                            break;
                        case StavMenu.Multiplayer:
                            if (tlacitkaMenu[i].data == "pripojit")
                                try
                                {
                                    IPAddress iPAddresa = IPAddress.Parse(Textbox.VratTextPodleDat(textboxyMenu, "ipadresa"));

                                    PrepniStavMenu(StavMenu.Hra);
                                    hra.komponentaMultiplayer.PripojSeNaServer(iPAddresa, true);
                                }
                                catch
                                {
                                    PrepniStavMenu(StavMenu.Multiplayer);
                                }
                            else if (tlacitkaMenu[i].data == "zpatky")
                                PrepniStavMenu(StavMenu.HlavniMenu);
                            break;
                    }
                }
            }

            //Aktualizace textboxu
            for (int i = 0; i < textboxyMenu.Count; i++)
            {
                textboxyMenu[i].UpdatujTextbox(hra, gameTime, Mouse.GetState(), posunutiMenu, pomerRozliseni);
            }

            base.Update(gameTime);
        }


        public override void Draw(GameTime gameTime)
        {
            hra._spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            float pomerRozliseni = Math.Min(hra.velikostOkna.X / BEZNE_ROZLISENI_X, hra.velikostOkna.Y / BEZNE_ROZLISENI_Y);
            float pomerX = hra.velikostOkna.X / BEZNE_ROZLISENI_X / pomerRozliseni;
            float pomerY = hra.velikostOkna.Y / BEZNE_ROZLISENI_Y / pomerRozliseni;

            Vector2 posunutiMenu = new Vector2((pomerX - 1) / 2 * BEZNE_ROZLISENI_X * pomerRozliseni,
                    (pomerY - 1) / 2 * BEZNE_ROZLISENI_Y * pomerRozliseni);

            //Vykresleni tlacitek
            foreach (Tlacitko tlacitko in tlacitkaMenu)
                tlacitko.VykresliTlacitko(hra, posunutiMenu, pomerRozliseni);

            //Vykresleni textboxu
            foreach (Textbox textbox in textboxyMenu)
                textbox.VykresliTextbox(hra, posunutiMenu, pomerRozliseni);

            if (stavMenu == StavMenu.HlavniMenu)
            {
                //Vykresleni nadpisu
                float velikostTextu = 0.35f * pomerRozliseni;
                string text1 = "Labyrinth";
                Vector2 velikostTextu1 = Hra.pixeloidSans.MeasureString(text1) * velikostTextu;
                hra.VykresliTextSOkrajem(Hra.pixeloidSans, new Vector2(posunutiMenu.X + BEZNE_ROZLISENI_X / 2 * pomerRozliseni - velikostTextu1.X / 2, velikostTextu1.Y / 2), text1, velikostTextu, Color.White, Color.Black, 0.07f, 8, true);
                string text2 = "of";
                Vector2 velikostTextu2 = Hra.pixeloidSans.MeasureString(text2) * velikostTextu;
                hra.VykresliTextSOkrajem(Hra.pixeloidSans, new Vector2(posunutiMenu.X + BEZNE_ROZLISENI_X / 2 * pomerRozliseni - velikostTextu2.X / 2, velikostTextu1.Y * 1.5f), text2, velikostTextu, Color.White, Color.Black, 0.07f, 8, true);
                string text3 = "Secrets";
                Vector2 velikostTextu3 = Hra.pixeloidSans.MeasureString(text3) * velikostTextu;
                hra.VykresliTextSOkrajem(Hra.pixeloidSans, new Vector2(posunutiMenu.X + BEZNE_ROZLISENI_X / 2 * pomerRozliseni - velikostTextu3.X / 2, velikostTextu1.Y * 1.5f + velikostTextu2.Y), text3, velikostTextu, Color.White, Color.Black, 0.07f, 8, true);

                //Vykresleni verze
                velikostTextu = 0.15f * pomerRozliseni;
                string textVerze = "Vývojová verze";
                Vector2 velikostTextuVerze = Hra.pixeloidSans.MeasureString(text3) * velikostTextu;
                hra.VykresliTextSOkrajem(Hra.pixeloidSans, new Vector2(0, hra.velikostOkna.Y - velikostTextuVerze.Y), textVerze, velikostTextu, Color.White, Color.Black, 0.07f, 8, true);
            }

            hra._spriteBatch.End();

            base.Draw(gameTime);
        }

        public void PrepniStavMenu(StavMenu stavMenu)
        {
            this.stavMenu = stavMenu;
            tlacitkaMenu.Clear();
            textboxyMenu.Clear();
            hra.VypniKomponentyHry();

            switch (stavMenu)
            {
                case StavMenu.HlavniMenu:
                    tlacitkaMenu.Add(new Tlacitko(new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) / 2 - new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y) / 2,
                        new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y), "Singleplayer", Color.Gray, "singleplayer", 6f));
                    tlacitkaMenu.Add(new Tlacitko(new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) / 2 - new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y) / 2 + new Vector2(0, ODSAZENI_TLACITEK),
                        new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y), "Multiplayer", Color.Gray, "multiplayer", 6f));
                    tlacitkaMenu.Add(new Tlacitko(new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) / 2 - new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y) / 2 + new Vector2(0, ODSAZENI_TLACITEK * 2),
                        new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y), "Nastavení", Color.Gray, "nastaveni", 6f));
                    tlacitkaMenu.Add(new Tlacitko(new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) / 2 - new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y) / 2 + new Vector2(0, ODSAZENI_TLACITEK * 3),
                        new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y), "Odejít", Color.Gray, "odejit", 6f));
                    break;
                case StavMenu.Nastaveni:
                    tlacitkaMenu.Add(new Tlacitko(new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) / 2 - new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y) / 2,
                        new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y), "Přepni fullscreen", Color.Gray, "fullscreen", 6f));
                    tlacitkaMenu.Add(new Tlacitko(new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) / 2 - new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y) / 2 + new Vector2(0, ODSAZENI_TLACITEK),
                        new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y), "Zpátky", Color.Gray, "zpatky", 6f));
                    break;
                case StavMenu.Multiplayer:
                    textboxyMenu.Add(new Textbox(new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) / 2 + new Vector2(-VELIKOST_TEXTBOXU_X / 2, 0),
                        new Vector2(VELIKOST_TEXTBOXU_X, VELIKOST_TEXTBOXU_Y), Color.Gray, "ipadresa", 6f));
                    tlacitkaMenu.Add(new Tlacitko(new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) / 2 + new Vector2(-VELIKOST_TLACITKA_X / 2, ODSAZENI_TEXTBOXU),
                        new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y), "Připojit", Color.Gray, "pripojit", 6f));
                    tlacitkaMenu.Add(new Tlacitko(new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) / 2 + new Vector2(-VELIKOST_TLACITKA_X / 2, ODSAZENI_TEXTBOXU + ODSAZENI_TLACITEK),
                        new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y), "Zpátky", Color.Gray, "zpatky", 6f));
                    break;
                case StavMenu.Hra:
                    hra.SpustKomponentyHry();
                    break;
            }
        }
    }
}
