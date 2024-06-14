using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Labyrinth_of_Secrets
{
    public class KomponentaKonzole : DrawableGameComponent
    {
        Texture2D texturaKonzole1, texturaKonzole2;

        private Hra hra;
        public bool jeOtevrena = false;
        private string zprava = "";
        public List<Radek> radky = new List<Radek>();
        public List<string> historie = new List<string>();
        public int pozice = -1;
        private int poziceKurzoru = 0;
        private float casKurzoru = 0;

        public KomponentaKonzole(Hra hra) : base(hra)
        {
            this.hra = hra;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            texturaKonzole1 = hra.VytvorTexturuObdelnikuSOkrejem(550, 578, 0, new Color(28, 28, 28, 188));
            texturaKonzole2 = hra.VytvorTexturuObdelnikuSOkrejem(550, 17, 0, new Color(34, 34, 34, 236));

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            casKurzoru += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (casKurzoru >= 1f)
            {
                casKurzoru = 0;
            }

            if (hra.NoveZmacknutaKlavesa(Keys.OemTilde) || hra.NoveZmacknutaKlavesa(Keys.OemSemicolon))
            {
                if (jeOtevrena)
                    jeOtevrena = false;
                else
                    jeOtevrena = true;
                zprava = "";
                poziceKurzoru = 0;
                pozice = -1;
            }
            else if (hra.NoveZmacknutaKlavesa(Keys.Enter))
            {
                if (zprava == "")
                    return;
                if (historie.Count == 0 || historie[0] != zprava)
                    historie.Insert(0, zprava);
                pozice = -1;
                try
                {
                    radky.Insert(0, new Radek("> " + zprava, Color.White));
                    string[] rozebranaMessage = zprava.Split(' ');
                    if (rozebranaMessage[0] == "help" || rozebranaMessage[0] == "hp")
                    {
                        radky.Insert(0, new Radek("help/hp - Zobrazí všechny příkazy", Color.White));
                        radky.Insert(0, new Radek("clear/cl - Vymaže chat", Color.White));
                        radky.Insert(0, new Radek("setresolution/sr [velikost X] [velikost Y] - Nastavuje rozlišení", Color.White));
                        radky.Insert(0, new Radek("fullscreen/fs [\"true\"/\"false\"] - Aktivuje/Deaktivuje fullscreen", Color.White));
                        radky.Insert(0, new Radek("generatemap/gm - Znova vygeneruje celou mapu", Color.White));
                        radky.Insert(0, new Radek("lightupdate/lu [\"true\"/\"false\"] - Aktivuje/Deaktivuje aktualizaci svetla", Color.White));
                        radky.Insert(0, new Radek("fullbright/fb [\"true\"/\"false\"] - Aktivuje/Deaktivuje fullbright", Color.White));
                        radky.Insert(0, new Radek("showpath/sp [\"true\"/\"false\"] - Aktivuje/Deaktivuje viditelnost spravne cesty", Color.White));
                        radky.Insert(0, new Radek("freecam/fc [\"true\"/\"false\"] - Aktivuje/Deaktivuje volnou kameru", Color.White));
                        radky.Insert(0, new Radek("hostserver/hs - Spustí hostování serveru", Color.White));
                        radky.Insert(0, new Radek("joinserver/js [IP adresa] - Připojí se na server", Color.White));
                        radky.Insert(0, new Radek("echoname/en - Vypíše jméno hráče", Color.White));
                        radky.Insert(0, new Radek("setname/sn [jméno] - Nastaví jméno hráče", Color.White));
                        radky.Insert(0, new Radek("disconnect/dc - Odpojí se od serveru", Color.White));
                        radky.Insert(0, new Radek("server/sv [příkaz] - Pokud hostuji server, předám mu příkaz", Color.White));
                        radky.Insert(0, new Radek("save/sa [název souboru] - Uloží mapu na disk", Color.White));
                        radky.Insert(0, new Radek("load/lo [název souboru] - Načte mapy z disku", Color.White));
                        radky.Insert(0, new Radek("lsmaps/lm - Vypíše mapy na disku", Color.White));
                    }
                    else if (rozebranaMessage[0] == "clear" || rozebranaMessage[0] == "cl")
                    {
                        radky = new List<Radek>();
                        radky.Insert(0, new Radek("Chat úspěšně vymazán", Color.LimeGreen));
                    }
                    else if (rozebranaMessage[0] == "setresolution" || rozebranaMessage[0] == "sr")
                    {
                        hra.velikostOkna = new Point(int.Parse(rozebranaMessage[1]), int.Parse(rozebranaMessage[2]));
                        radky.Insert(0, new Radek("Nové rozlišení úspěšně nastaveno.", Color.LimeGreen));
                        hra.NastavRozliseni();
                    }
                    else if (rozebranaMessage[0] == "fullscreen" || rozebranaMessage[0] == "fs")
                    {
                        if (rozebranaMessage[1] == "true")
                        {
                            hra.fullscreen = true;
                            hra.NastavRozliseni();
                            radky.Insert(0, new Radek("Fullscreen byl aktivován.", Color.LimeGreen));
                        }
                        else if (rozebranaMessage[1] == "false")
                        {
                            hra.fullscreen = false;
                            hra.NastavRozliseni();
                            radky.Insert(0, new Radek("Fullscreen byl deaktivován.", Color.LimeGreen));
                        }
                        else
                        {
                            radky.Insert(0, new Radek("Chyba! - Zkuste příkaz help", Color.Red));
                        }
                        hra.NastavRozliseni();
                    }
                    else if (rozebranaMessage[0] == "generatemap" || rozebranaMessage[0] == "gm")
                    {
                        if (hra.komponentaMultiplayer.typZarizeni == KomponentaMultiplayer.TypZarizeni.SinglePlayer)
                        {
                            hra.komponentaMapa.VygenerujMapu();
                            radky.Insert(0, new Radek("Mapa přegenerována.", Color.LimeGreen));
                        }
                        else
                            radky.Insert(0, new Radek("Nelze měnit mapu v průběhu multiplayeru.", Color.Red));
                    }
                    else if (rozebranaMessage[0] == "lightupdate" || rozebranaMessage[0] == "lu")
                    {
                        if (rozebranaMessage[1] == "true")
                        {
                            hra.komponentaSvetlo.SpustPocitaniSvetla();
                            radky.Insert(0, new Radek("Počítání světla zapnuto.", Color.LimeGreen));
                        }
                        else if (rozebranaMessage[1] == "false")
                        {
                            hra.komponentaSvetlo.ZastavPocitaniSvetla();
                            radky.Insert(0, new Radek("Počítání světla vypnuto.", Color.LimeGreen));
                        }
                        else
                        {
                            radky.Insert(0, new Radek("Chyba! - Zkuste příkaz help", Color.Red));
                        }
                    }
                    else if (rozebranaMessage[0] == "fullbright" || rozebranaMessage[0] == "fb")
                    {
                        if (rozebranaMessage[1] == "true")
                        {
                            hra.komponentaSvetlo.fullBright = true;
                            hra.komponentaSvetlo.ZastavPocitaniSvetla();
                            radky.Insert(0, new Radek("Fullbright zapnut.", Color.LimeGreen));
                        }
                        else if (rozebranaMessage[1] == "false")
                        {
                            hra.komponentaSvetlo.fullBright = false;
                            hra.komponentaSvetlo.SpustPocitaniSvetla();
                            radky.Insert(0, new Radek("Fullbright vypnut.", Color.LimeGreen));
                        }
                        else
                        {
                            radky.Insert(0, new Radek("Chyba! - Zkuste příkaz help", Color.Red));
                        }
                    }
                    else if (rozebranaMessage[0] == "showpath" || rozebranaMessage[0] == "sp")
                    {
                        if (rozebranaMessage[1] == "true")
                        {
                            hra.komponentaMapa.ukazCestu = true;
                            radky.Insert(0, new Radek("Zobrazení cestu zapnuto.", Color.LimeGreen));
                        }
                        else if (rozebranaMessage[1] == "false")
                        {
                            hra.komponentaMapa.ukazCestu = false;
                            radky.Insert(0, new Radek("Zobrazení cestu vypnuto.", Color.LimeGreen));
                        }
                        else
                        {
                            radky.Insert(0, new Radek("Chyba! - Zkuste příkaz help", Color.Red));
                        }
                    }
                    else if (rozebranaMessage[0] == "freecam" || rozebranaMessage[0] == "fc")
                    {
                        if (rozebranaMessage[1] == "true")
                        {
                            hra.komponentaKamera.volnaKamera = true;
                            radky.Insert(0, new Radek("Volná kamera zapnuta.", Color.LimeGreen));
                        }
                        else if (rozebranaMessage[1] == "false")
                        {
                            hra.komponentaKamera.volnaKamera = false;
                            radky.Insert(0, new Radek("Volná kamera vypnuta.", Color.LimeGreen));
                        }
                        else
                        {
                            radky.Insert(0, new Radek("Chyba! - Zkuste příkaz help", Color.Red));
                        }
                    }
                    else if (rozebranaMessage[0] == "hostserver" || rozebranaMessage[0] == "hs")
                    {
                        hra.komponentaMultiplayer.SpustServer();
                    }
                    else if (rozebranaMessage[0] == "joinserver" || rozebranaMessage[0] == "js")
                    {
                        hra.komponentaMultiplayer.PripojSeNaServer(IPAddress.Parse(rozebranaMessage[1]), false);
                    }
                    else if (rozebranaMessage[0] == "echoname" || rozebranaMessage[0] == "en")
                    {
                        radky.Insert(0, new Radek("Jméno hráče je " + hra.jmeno, Color.White));
                    }
                    else if (rozebranaMessage[0] == "setname" || rozebranaMessage[0] == "sn")
                    {
                        if (hra.komponentaMultiplayer.typZarizeni == KomponentaMultiplayer.TypZarizeni.SinglePlayer)
                        {
                            hra.jmeno = rozebranaMessage[1];
                            radky.Insert(0, new Radek("Jméno úspěšně změněno.", Color.LimeGreen));
                        }
                        else
                            radky.Insert(0, new Radek("Nelze měnit jméno v průběhu multiplayeru.", Color.Red));
                    }
                    else if (rozebranaMessage[0] == "disconnect" || rozebranaMessage[0] == "dc")
                    {
                        if (hra.komponentaMultiplayer.typZarizeni == KomponentaMultiplayer.TypZarizeni.Klient)
                        {
                            for (int j = 0; j < 10; j++)
                                hra.komponentaMultiplayer.PosliData(Encoding.UTF8.GetBytes($"{(short)KomponentaMultiplayer.TypPacketu.OdpojilSeKlient};{hra.jmeno}"));

                            radky.Insert(0, new Radek("Odpojeno od serveru"));
                            hra.komponentaMultiplayer.OdpojSeOdServer();

                        }
                        else
                            radky.Insert(0, new Radek("Nelze se odpojit od serveru když na něm nejsem připojený!", Color.Yellow));
                    }
                    else if (rozebranaMessage[0] == "server" || rozebranaMessage[0] == "sv")
                    {
                        if (hra.komponentaMultiplayer.procesServeru != null && !hra.komponentaMultiplayer.procesServeru.HasExited)
                        {
                            hra.komponentaMultiplayer.procesServeru.StandardInput.WriteLine(zprava.Substring(rozebranaMessage[0].Length + 1).TrimEnd());

                            if (zprava.Substring(rozebranaMessage[0].Length + 1).TrimEnd() == "exit")
                            {
                                radky.Insert(0, new Radek("Odpojeno od serveru"));
                                hra.komponentaMultiplayer.OdpojSeOdServer();
                            }
                        }
                        else
                            radky.Insert(0, new Radek("Nelze předat příkaz serveru, když ho nehostuji!", Color.Yellow));
                    }
                    else if (rozebranaMessage[0] == "save" || rozebranaMessage[0] == "sa")
                    {
                        if (Regex.Match(rozebranaMessage[1], @"^[a-zA-Z0-9_]+$").Success)
                        {
                            hra.komponentaMapa.UlozMapuNaDisk(rozebranaMessage[1]);
                            radky.Insert(0, new Radek("Mapa úspěšně uložena.", Color.LimeGreen));
                        }
                        else
                        {
                            radky.Insert(0, new Radek("Jméno nesmí obsahovat speciální znaky!", Color.Yellow));
                        }
                    }
                    else if (rozebranaMessage[0] == "load" || rozebranaMessage[0] == "lo")
                    {
                        if (Regex.Match(rozebranaMessage[1], @"^[a-zA-Z0-9_]+$").Success)
                        {
                            string cestaKDokumentum = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                            string cestaKSavum = Path.Combine(new string[] { cestaKDokumentum, ".Labyrinth_of_Secrets", "Saves" });

                            if (Directory.Exists(cestaKSavum))
                            {
                                hra.komponentaMapa.NactiMapuZeSouboru(rozebranaMessage[1]);
                                radky.Insert(0, new Radek("Mapa úspěšně načtena.", Color.LimeGreen));
                            }
                            else
                                radky.Insert(0, new Radek("Tento soubor neexistuje!", Color.Yellow));
                        }
                        else
                        {
                            radky.Insert(0, new Radek("Jméno nesmí obsahovat speciální znaky!", Color.Yellow));
                        }
                    }
                    else if (rozebranaMessage[0] == "lsmaps" || rozebranaMessage[0] == "lm")
                    {
                        string cestaKDokumentum = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        string cestaKSavum = Path.Combine(new string[] { cestaKDokumentum, ".Labyrinth_of_Secrets", "Saves" });

                        if (Directory.Exists(cestaKSavum))
                        {
                            string[] savy = Directory.GetDirectories(cestaKSavum);
                            if (savy.Length == 0)
                                radky.Insert(0, new Radek("Žádne mapy zatím nejsou uloženy.", Color.Yellow));
                            else
                            {
                                radky.Insert(0, new Radek("Uložené mapy:", Color.White));
                                foreach (string save in savy)
                                {
                                    radky.Insert(0, new Radek(save, Color.White));
                                }
                            }
                        }
                        else
                            radky.Insert(0, new Radek("Žádne mapy zatím nejsou uloženy.", Color.Yellow));
                    }
                    else
                    {
                        radky.Insert(0, new Radek("Příkaz neexistuje! - Zkuste příkaz help", Color.Yellow));
                    }
                }
                catch
                {
                    radky.Insert(0, new Radek("Chyba - Zkuste help", Color.Red));
                }
                poziceKurzoru = 0;
                zprava = "";
            }
            else
            {
                if (hra.NoveZmacknutaKlavesa(Keys.Up) && pozice + 1 < historie.Count)
                {
                    pozice++;
                    zprava = historie[pozice];
                    poziceKurzoru = zprava.Length;
                }
                else if (hra.NoveZmacknutaKlavesa(Keys.Down) && pozice - 1 >= 0)
                {
                    pozice--;
                    zprava = historie[pozice];
                    poziceKurzoru = zprava.Length;
                }
                Keys[] zmackleKlavesy = Keyboard.GetState().GetPressedKeys();
                if (jeOtevrena)
                {
                    foreach (Keys klavesa in zmackleKlavesy)
                    {
                        char znak = Hra.KeyToChar(klavesa, (Keyboard.GetState().CapsLock ^ Keyboard.GetState().IsKeyDown(Keys.LeftShift)));
                        if (!Hra.Contains(hra.naposledyZmackleKlavesy, klavesa) && ((znak >= 32 && 12 + Hra.comicSans.MeasureString("> " + zprava + znak + " ").X * 0.05f < 550) || klavesa == Keys.Left || klavesa == Keys.Right || klavesa == Keys.Back || klavesa == Keys.Delete))
                        {
                            if (klavesa != Keys.Left && klavesa != Keys.Right && klavesa != Keys.Back && klavesa != Keys.Delete)
                            {
                                zprava = zprava.Substring(0, poziceKurzoru) + znak + zprava.Substring(poziceKurzoru);
                                poziceKurzoru++;
                            }
                            else if (klavesa == Keys.Back && zprava.Length > 0 && poziceKurzoru != 0)
                            {
                                zprava = zprava.Substring(0, poziceKurzoru - 1) + zprava.Substring(poziceKurzoru);
                                poziceKurzoru--;
                            }
                            else if (klavesa == Keys.Delete && zprava.Length > 0 && poziceKurzoru != zprava.Length)
                            {
                                zprava = zprava.Substring(0, poziceKurzoru) + zprava.Substring(poziceKurzoru + 1);
                            }
                            else if (klavesa == Keys.Left && poziceKurzoru != 0)
                                poziceKurzoru--;
                            else if (klavesa == Keys.Right && poziceKurzoru != zprava.Length)
                                poziceKurzoru++;
                        }
                    }

                }
                hra.naposledyZmackleKlavesy = zmackleKlavesy;
            }
            while (radky.Count * Hra.comicSans.MeasureString("abcde").Y * 0.05f > 595 - 18 && radky.Count > 0)
            {
                radky.RemoveAt(radky.Count - 1);
            }
            base.Update(gameTime);
        }


        public override void Draw(GameTime gameTime)
        {
            hra._spriteBatch.Begin();
            if (jeOtevrena)
            {
                hra._spriteBatch.Draw(texturaKonzole1, new Rectangle(0, (int)(hra.velikostOkna.Y - 595), 550, 578), Color.White);
                hra._spriteBatch.Draw(texturaKonzole2, new Rectangle(0, (int)(hra.velikostOkna.Y - 17), 550, 17), Color.White);
                if (casKurzoru < 0.5f)
                    hra.VykresliTextSOkrajem(Hra.comicSans, new Vector2(8, (int)(hra.velikostOkna.Y - 18)), "> " + zprava.Substring(0, poziceKurzoru) +
                        "l" + zprava.Substring(poziceKurzoru), 0.05f, Color.White, Color.Black, 0.08f, 8, false);
                else
                    hra.VykresliTextSOkrajem(Hra.comicSans, new Vector2(8, (int)(hra.velikostOkna.Y - 18)), "> " + zprava.Substring(0, poziceKurzoru) +
                        " " + zprava.Substring(poziceKurzoru), 0.05f, Color.White, Color.Black, 0.08f, 8, false);
                for (int i = 0; i < radky.Count; i++)
                    hra.VykresliTextSOkrajem(Hra.comicSans, new Vector2(2, (int)(hra.velikostOkna.Y - 18 - ((i + 1) * Hra.comicSans.MeasureString("abcde").Y * 0.05f))),
                        radky[i].text, 0.05f, radky[i].barva, Color.Black, 0.08f, 8, false);
            }
            hra._spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
