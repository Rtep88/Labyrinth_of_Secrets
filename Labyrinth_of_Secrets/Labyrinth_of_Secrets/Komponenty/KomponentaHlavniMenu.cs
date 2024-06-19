using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
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
        const int ODSAZENI = 30;
        const int VELIKOST_TEXTBOXU_X = 800;
        const int VELIKOST_TEXTBOXU_Y = 80;
        const float BEZNE_ROZLISENI_X = 1920;
        const float BEZNE_ROZLISENI_Y = 1080;
        const int VELIKOST_TLACITKA_SVETA_Y = 80;
        const int POCET_SVETU_NA_STRANCE = 5;

        //Enumy
        public enum StavMenu
        {
            HlavniMenu,
            VyberSveta,
            VytvareniSveta,
            Nastaveni,
            Multiplayer,
            Hra
        }

        //Promenne
        private List<Tlacitko> tlacitkaMenu = new List<Tlacitko>();
        private List<Textbox> textboxyMenu = new List<Textbox>();
        private List<Label> labelyMenu = new List<Label>();
        public StavMenu stavMenu = StavMenu.HlavniMenu;
        private Texture2D pozadiMenu;
        private int strankaSvetu = 0;

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
            pozadiMenu = hra.Content.Load<Texture2D>("Images/Menu_background");

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            float pomerRozliseni = Math.Min(hra.velikostOkna.X / BEZNE_ROZLISENI_X, hra.velikostOkna.Y / BEZNE_ROZLISENI_Y);
            float pomerX = hra.velikostOkna.X / BEZNE_ROZLISENI_X / pomerRozliseni;
            float pomerY = hra.velikostOkna.Y / BEZNE_ROZLISENI_Y / pomerRozliseni;

            Vector2 posunutiMenu = new Vector2((pomerX - 1) / 2 * BEZNE_ROZLISENI_X * pomerRozliseni,
                    (pomerY - 1) / 2 * BEZNE_ROZLISENI_Y * pomerRozliseni);

            Point velikostSvetu = new Point((int)(hra.velikostOkna.X * 2f / 3f / pomerX), (int)(hra.velikostOkna.Y * 2f / 3f / pomerY));
            Point poziceSvetu = hra.velikostOkna / new Point(2) - velikostSvetu / new Point(2);

            Vector2 posunuti = posunutiMenu;

            if (stavMenu == StavMenu.VyberSveta)
                posunuti = poziceSvetu.ToVector2();

            //Aktualizace tlacitek
            for (int i = 0; i < tlacitkaMenu.Count; i++)
            {
                if (tlacitkaMenu[i].UpdatujTlacitko(Mouse.GetState(), posunuti, pomerRozliseni, hra.IsActive))
                {
                    switch (stavMenu)
                    {
                        case StavMenu.HlavniMenu:
                            if (tlacitkaMenu[i].data == "singleplayer")
                            {
                                strankaSvetu = 0;
                                PrepniStavMenu(StavMenu.VyberSveta);
                            }
                            else if (tlacitkaMenu[i].data == "multiplayer")
                                PrepniStavMenu(StavMenu.Multiplayer);
                            else if (tlacitkaMenu[i].data == "nastaveni")
                                PrepniStavMenu(StavMenu.Nastaveni);
                            else if (tlacitkaMenu[i].data == "odejit")
                                hra.VypniHru();
                            break;
                        case StavMenu.VyberSveta:
                            if (tlacitkaMenu[i].data == "zpatky")
                            {
                                strankaSvetu--;
                                PrepniStavMenu(StavMenu.HlavniMenu);
                            }
                            else if (tlacitkaMenu[i].data == "dozadu")
                            {
                                strankaSvetu--;
                                PrepniStavMenu(StavMenu.VyberSveta);
                            }
                            else if (tlacitkaMenu[i].data == "dopredu")
                            {
                                strankaSvetu++;
                                PrepniStavMenu(StavMenu.VyberSveta);
                            }
                            else if (tlacitkaMenu[i].data == "vytvorit")
                            {
                                PrepniStavMenu(StavMenu.VytvareniSveta);
                            }
                            else if (tlacitkaMenu[i].data == "svet")
                            {
                                try
                                {
                                    string jmenoSveta = tlacitkaMenu[i].text;
                                    PrepniStavMenu(StavMenu.Hra);
                                    hra.komponentaMapa.NactiMapuZeSouboru(jmenoSveta);
                                    hra.komponentaMapa.jmenoSveta = jmenoSveta;
                                }
                                catch
                                {
                                    PrepniStavMenu(StavMenu.VyberSveta);
                                }
                            }
                            break;
                        case StavMenu.VytvareniSveta:
                            if (tlacitkaMenu[i].data == "vytvorit")
                                try
                                {
                                    string jmenoSveta = Textbox.VratTextPodleDat(textboxyMenu, "jmenoSveta");
                                    string cestaKDokumentum = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                                    string cestaKSavu = Path.Combine(new string[] { cestaKDokumentum, ".Labyrinth_of_Secrets", "Saves", jmenoSveta });

                                    if (!Regex.Match(jmenoSveta, @"^[a-zA-Z0-9_]+$").Success || Directory.Exists(cestaKSavu))
                                        break;

                                    PrepniStavMenu(StavMenu.Hra);
                                    hra.komponentaMapa.UlozMapuNaDisk(jmenoSveta);
                                    PrepniStavMenu(StavMenu.VyberSveta);
                                }
                                catch
                                {
                                    PrepniStavMenu(StavMenu.VytvareniSveta);
                                }
                            else if (tlacitkaMenu[i].data == "zpatky")
                            {
                                strankaSvetu = 0;
                                PrepniStavMenu(StavMenu.VyberSveta);
                            }
                            break;
                        case StavMenu.Nastaveni:
                            if (tlacitkaMenu[i].data == "fullscreen")
                            {
                                hra.fullscreen = !hra.fullscreen;
                                hra.NastavRozliseni();
                            }
                            else if (tlacitkaMenu[i].data == "zpatky")
                            {
                                string noveJmeno = Textbox.VratTextPodleDat(textboxyMenu, "jmenoHrace");

                                if (noveJmeno != null && noveJmeno != "" && noveJmeno.Length <= 20 && Regex.Match(noveJmeno, @"^[a-zA-Z0-9_]+$").Success)
                                {
                                    string cestaKDokumentum = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                                    string cestaKeHernimDatum = Path.Combine(new string[] { cestaKDokumentum, ".Labyrinth_of_Secrets" });
                                    string cestaKeKonfigu = Path.Combine(cestaKeHernimDatum, "config.ini");

                                    Hra.ZapisHodnotuDoIni(cestaKeKonfigu, "playerName", noveJmeno);
                                    hra.jmeno = noveJmeno;

                                    PrepniStavMenu(StavMenu.HlavniMenu);
                                }
                            }
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
                textboxyMenu[i].UpdatujTextbox(hra, gameTime, Mouse.GetState(), posunutiMenu, pomerRozliseni, hra.IsActive);
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

            if (stavMenu != StavMenu.Hra)
            {
                Point velikostPozadi = hra.velikostOkna;
                if (pomerX > pomerY)
                    velikostPozadi.Y = (int)(velikostPozadi.X * BEZNE_ROZLISENI_Y / BEZNE_ROZLISENI_X);
                else
                    velikostPozadi.X = (int)(velikostPozadi.Y * BEZNE_ROZLISENI_X / BEZNE_ROZLISENI_Y);

                Point pozicePozadi = (hra.velikostOkna - velikostPozadi) / new Point(2);

                //Vykresleni pozadi
                hra._spriteBatch.Draw(pozadiMenu, new Rectangle(pozicePozadi, velikostPozadi), Color.White);
            }

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

            //Vykresleni vyberu sveta
            Point velikostSvetu = new Point((int)(hra.velikostOkna.X * 2f / 3f / pomerX), (int)(hra.velikostOkna.Y * 2f / 3f / pomerY));
            Point poziceSvetu = hra.velikostOkna / new Point(2) - velikostSvetu / new Point(2);
            if (stavMenu == StavMenu.VyberSveta)
            {
                hra._spriteBatch.Draw(Hra.pixel, new Rectangle(poziceSvetu, velikostSvetu), new Color(50, 50, 50, 210));
            }

            if (stavMenu != StavMenu.VyberSveta)
            {
                //Vykresleni tlacitek
                foreach (Tlacitko tlacitko in tlacitkaMenu)
                    tlacitko.VykresliTlacitko(hra, posunutiMenu, pomerRozliseni);

                //Vykresleni textboxu
                foreach (Textbox textbox in textboxyMenu)
                    textbox.VykresliTextbox(hra, posunutiMenu, pomerRozliseni);

                //Vykresleni labelu
                foreach (Label label in labelyMenu)
                    label.VykresliLabel(hra, posunutiMenu, pomerRozliseni);
            }
            else
            {
                //Vykresleni tlacitek
                foreach (Tlacitko tlacitko in tlacitkaMenu)
                    tlacitko.VykresliTlacitko(hra, poziceSvetu.ToVector2(), pomerRozliseni);

                //Vykresleni labelu
                foreach (Label label in labelyMenu)
                    label.VykresliLabel(hra, poziceSvetu.ToVector2(), pomerRozliseni);
            }

            hra._spriteBatch.End();

            base.Draw(gameTime);
        }

        public void PrepniStavMenu(StavMenu stavMenu)
        {
            this.stavMenu = stavMenu;
            tlacitkaMenu.Clear();
            textboxyMenu.Clear();
            labelyMenu.Clear();
            hra.VypniKomponentyHry();

            switch (stavMenu)
            {
                case StavMenu.HlavniMenu:
                    tlacitkaMenu.Add(new Tlacitko(new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) / 2 - new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y) / 2,
                        new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y), "Singleplayer", new Color(110, 110, 110), "singleplayer", 6f));
                    tlacitkaMenu.Add(new Tlacitko(new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) / 2 - new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y) / 2 + new Vector2(0, ODSAZENI + VELIKOST_TLACITKA_Y),
                        new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y), "Multiplayer", new Color(110, 110, 110), "multiplayer", 6f));
                    tlacitkaMenu.Add(new Tlacitko(new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) / 2 - new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y) / 2 + new Vector2(0, (ODSAZENI + VELIKOST_TLACITKA_Y) * 2),
                        new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y), "Nastavení", new Color(110, 110, 110), "nastaveni", 6f));
                    tlacitkaMenu.Add(new Tlacitko(new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) / 2 - new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y) / 2 + new Vector2(0, (ODSAZENI + VELIKOST_TLACITKA_Y) * 3),
                        new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y), "Odejít", new Color(110, 110, 110), "odejit", 6f));
                    break;
                case StavMenu.VyberSveta:
                    Vector2 velikostSeznamu = new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) * 2 / 3;
                    List<string> svety = new List<string>();
                    string cestaKDokumentum = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string cestaKSavum = Path.Combine(new string[] { cestaKDokumentum, ".Labyrinth_of_Secrets", "Saves" });

                    if (Directory.Exists(cestaKSavum))
                        svety = Directory.GetDirectories(cestaKSavum).ToList();

                    svety = svety.Where(x => File.Exists(Path.Combine(x, "world.bin"))).ToList();

                    int maxStranka = Math.Max(0, (svety.Count - 1) / POCET_SVETU_NA_STRANCE);
                    strankaSvetu = Math.Max(0, Math.Min(maxStranka, strankaSvetu));

                    for (int i = 0; i < POCET_SVETU_NA_STRANCE && (i + POCET_SVETU_NA_STRANCE * strankaSvetu) < svety.Count; i++)
                    {
                        tlacitkaMenu.Add(new Tlacitko(new Vector2(ODSAZENI) + new Vector2(0, (VELIKOST_TLACITKA_SVETA_Y + ODSAZENI) * i),
                            new Vector2(velikostSeznamu.X - ODSAZENI * 2, VELIKOST_TLACITKA_SVETA_Y), Path.GetFileNameWithoutExtension(svety[i + POCET_SVETU_NA_STRANCE * strankaSvetu]),
                            new Color(110, 110, 110), "svet", 6f));
                    }
                    float velikostTlacitka = (velikostSeznamu.X - ODSAZENI * 6) / 5;

                    tlacitkaMenu.Add(new Tlacitko(new Vector2(ODSAZENI) + new Vector2(0, (VELIKOST_TLACITKA_SVETA_Y + ODSAZENI) * 5),
                        new Vector2(velikostTlacitka, VELIKOST_TLACITKA_SVETA_Y), "Zpátky", new Color(110, 110, 110), "zpatky", 6f));

                    if (strankaSvetu > 0)
                        tlacitkaMenu.Add(new Tlacitko(new Vector2(ODSAZENI) + new Vector2(velikostTlacitka + ODSAZENI, (VELIKOST_TLACITKA_SVETA_Y + ODSAZENI) * 5),
                            new Vector2(velikostTlacitka, VELIKOST_TLACITKA_SVETA_Y), "<--", new Color(110, 110, 110), "dozadu", 6f));
                    if (0 != maxStranka)
                        labelyMenu.Add(new Label(new Vector2(ODSAZENI) + new Vector2((velikostTlacitka + ODSAZENI) * 2, (VELIKOST_TLACITKA_SVETA_Y + ODSAZENI) * 5),
                            new Vector2(velikostTlacitka, VELIKOST_TLACITKA_SVETA_Y), strankaSvetu + 1 + " / " + (maxStranka + 1), new Color(110, 110, 110), 6f));
                    if (strankaSvetu < maxStranka)
                        tlacitkaMenu.Add(new Tlacitko(new Vector2(ODSAZENI) + new Vector2((velikostTlacitka + ODSAZENI) * 3, (VELIKOST_TLACITKA_SVETA_Y + ODSAZENI) * 5),
                            new Vector2(velikostTlacitka, VELIKOST_TLACITKA_SVETA_Y), "-->", new Color(110, 110, 110), "dopredu", 6f));
                    tlacitkaMenu.Add(new Tlacitko(new Vector2(ODSAZENI) + new Vector2((velikostTlacitka + ODSAZENI) * 4, (VELIKOST_TLACITKA_SVETA_Y + ODSAZENI) * 5),
                        new Vector2(velikostTlacitka, VELIKOST_TLACITKA_SVETA_Y), "Vytvořit", Color.LimeGreen, "vytvorit", 6f));
                    break;
                case StavMenu.VytvareniSveta:
                    labelyMenu.Add(new Label(new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) / 2 + new Vector2(-VELIKOST_TEXTBOXU_X / 2, -ODSAZENI - VELIKOST_TEXTBOXU_Y),
                            new Vector2(VELIKOST_TEXTBOXU_X, VELIKOST_TEXTBOXU_Y), "Zadejte jméno světa:", new Color(110, 110, 110), 6f));
                    textboxyMenu.Add(new Textbox(new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) / 2 + new Vector2(-VELIKOST_TEXTBOXU_X / 2, 0),
                        new Vector2(VELIKOST_TEXTBOXU_X, VELIKOST_TEXTBOXU_Y), new Color(110, 110, 110), "jmenoSveta", 6f));
                    tlacitkaMenu.Add(new Tlacitko(new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) / 2 + new Vector2(-VELIKOST_TLACITKA_X / 2, ODSAZENI + VELIKOST_TEXTBOXU_Y),
                        new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y), "Vytvořit", new Color(110, 110, 110), "vytvorit", 6f));
                    tlacitkaMenu.Add(new Tlacitko(new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) / 2 + new Vector2(-VELIKOST_TLACITKA_X / 2, ODSAZENI + VELIKOST_TEXTBOXU_Y + (ODSAZENI + VELIKOST_TLACITKA_Y)),
                        new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y), "Zpátky", new Color(110, 110, 110), "zpatky", 6f));
                    break;
                case StavMenu.Nastaveni:
                    labelyMenu.Add(new Label(new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) / 2 - new Vector2(VELIKOST_TEXTBOXU_X / 2, (VELIKOST_TEXTBOXU_Y + ODSAZENI) * 2),
                                new Vector2(VELIKOST_TEXTBOXU_X, VELIKOST_TEXTBOXU_Y), "Jméno hráče:", new Color(110, 110, 110), 6f));
                    textboxyMenu.Add(new Textbox(new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) / 2 - new Vector2(VELIKOST_TEXTBOXU_X / 2, VELIKOST_TEXTBOXU_Y + ODSAZENI),
                        new Vector2(VELIKOST_TEXTBOXU_X, VELIKOST_TEXTBOXU_Y), hra.jmeno, new Color(110, 110, 110), "jmenoHrace", 6f));
                    tlacitkaMenu.Add(new Tlacitko(new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) / 2 - new Vector2(VELIKOST_TLACITKA_X, 0) / 2,
                        new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y), "Přepni fullscreen", new Color(110, 110, 110), "fullscreen", 6f));
                    tlacitkaMenu.Add(new Tlacitko(new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) / 2 - new Vector2(VELIKOST_TLACITKA_X, 0) / 2 + new Vector2(0, ODSAZENI + VELIKOST_TLACITKA_Y),
                        new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y), "Uložit", new Color(110, 110, 110), "zpatky", 6f));
                    break;
                case StavMenu.Multiplayer:
                    labelyMenu.Add(new Label(new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) / 2 + new Vector2(-VELIKOST_TEXTBOXU_X / 2, -ODSAZENI - VELIKOST_TEXTBOXU_Y),
                        new Vector2(VELIKOST_TEXTBOXU_X, VELIKOST_TEXTBOXU_Y), "Zadejte IP adresu serveru:", new Color(110, 110, 110), 6f));
                    textboxyMenu.Add(new Textbox(new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) / 2 + new Vector2(-VELIKOST_TEXTBOXU_X / 2, 0),
                        new Vector2(VELIKOST_TEXTBOXU_X, VELIKOST_TEXTBOXU_Y), new Color(110, 110, 110), "ipadresa", 6f));
                    tlacitkaMenu.Add(new Tlacitko(new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) / 2 + new Vector2(-VELIKOST_TLACITKA_X / 2, ODSAZENI + VELIKOST_TEXTBOXU_Y),
                        new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y), "Připojit", new Color(110, 110, 110), "pripojit", 6f));
                    tlacitkaMenu.Add(new Tlacitko(new Vector2(BEZNE_ROZLISENI_X, BEZNE_ROZLISENI_Y) / 2 + new Vector2(-VELIKOST_TLACITKA_X / 2, ODSAZENI + VELIKOST_TEXTBOXU_Y + (ODSAZENI + VELIKOST_TLACITKA_Y)),
                        new Vector2(VELIKOST_TLACITKA_X, VELIKOST_TLACITKA_Y), "Zpátky", new Color(110, 110, 110), "zpatky", 6f));
                    break;
                case StavMenu.Hra:
                    hra.SpustKomponentyHry();
                    break;
            }
        }
    }
}
