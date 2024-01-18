using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private Keys[] naposledyZmackleKlavesy;
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

            if (hra.NoveZmacknutaKlavesa(Keys.OemTilde))
            {
                naposledyZmackleKlavesy = Keyboard.GetState().GetPressedKeys();
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
                        hra.komponentaMapa.VygenerujMapu();
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
                        char znak = KeyToChar(klavesa, (Keyboard.GetState().CapsLock ^ Keyboard.GetState().IsKeyDown(Keys.LeftShift)));
                        if (!Contains(naposledyZmackleKlavesy, klavesa) && ((znak >= 32 && 12 + hra.comicSans.MeasureString("> " + zprava + znak + " ").X * 0.05f < 550) || klavesa == Keys.Left || klavesa == Keys.Right || klavesa == Keys.Back || klavesa == Keys.Delete))
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
                naposledyZmackleKlavesy = zmackleKlavesy;
            }
            while (radky.Count * hra.comicSans.MeasureString("abcde").Y * 0.05f > 595 - 18 && radky.Count > 0)
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
                    hra.VykresliTextSOkrajem(hra.comicSans, new Vector2(8, (int)(hra.velikostOkna.Y - 18)), "> " + zprava.Substring(0, poziceKurzoru) + "l" + zprava.Substring(poziceKurzoru), 0.05f, Color.White, Color.Black, 0.08f, 8);
                else
                    hra.VykresliTextSOkrajem(hra.comicSans, new Vector2(8, (int)(hra.velikostOkna.Y - 18)), "> " + zprava.Substring(0, poziceKurzoru) + " " + zprava.Substring(poziceKurzoru), 0.05f, Color.White, Color.Black, 0.08f, 8);
            }
            int i = 1;
            foreach (Radek radek in radky)
            {
                if (jeOtevrena)
                    hra.VykresliTextSOkrajem(hra.comicSans, new Vector2(2, (int)(hra.velikostOkna.Y - 18 - (i * hra.comicSans.MeasureString("abcde").Y * 0.05f))), radek.text, 0.05f, radek.barva, Color.Black, 0.08f, 8);
                i++;
            }
            hra._spriteBatch.End();
            base.Draw(gameTime);
        }

        public bool Contains(Keys[] klavesy, Keys klavesa)
        {
            foreach (Keys klavesa2 in klavesy)
            {
                if (klavesa == klavesa2)
                    return true;
            }
            return false;
        }

        public char KeyToChar(Keys Key, bool Shift = false)
        {
            if (Key == Keys.Space)
            {
                return ' ';
            }
            else
            {
                string String = Key.ToString();

                if (String.Length == 1)
                {
                    Char Character = Char.Parse(String);
                    byte Byte = Convert.ToByte(Character);

                    if (
                        (Byte >= 65 && Byte <= 90) ||
                        (Byte >= 97 && Byte <= 122)
                        )
                    {
                        return (!Shift ? Character.ToString().ToLower() : Character.ToString())[0];
                    }
                }

                switch (Key)
                {
                    case Keys.D0:
                        if (Shift) { return ')'; } else { return '0'; }
                    case Keys.D1:
                        if (Shift) { return '!'; } else { return '1'; }
                    case Keys.D2:
                        if (Shift) { return '@'; } else { return '2'; }
                    case Keys.D3:
                        if (Shift) { return '#'; } else { return '3'; }
                    case Keys.D4:
                        if (Shift) { return '$'; } else { return '4'; }
                    case Keys.D5:
                        if (Shift) { return '%'; } else { return '5'; }
                    case Keys.D6:
                        if (Shift) { return '^'; } else { return '6'; }
                    case Keys.D7:
                        if (Shift) { return '&'; } else { return '7'; }
                    case Keys.D8:
                        if (Shift) { return '*'; } else { return '8'; }
                    case Keys.D9:
                        if (Shift) { return '('; } else { return '9'; }

                    case Keys.NumPad0: return '0';
                    case Keys.NumPad1: return '1';
                    case Keys.NumPad2: return '2';
                    case Keys.NumPad3: return '3';
                    case Keys.NumPad4: return '4';
                    case Keys.NumPad5: return '5';
                    case Keys.NumPad6: return '6';
                    case Keys.NumPad7: return '7'; ;
                    case Keys.NumPad8: return '8';
                    case Keys.NumPad9: return '9';

                    case Keys.OemTilde:
                        if (Shift) { return '~'; } else { return '`'; }
                    case Keys.OemSemicolon:
                        if (Shift) { return ':'; } else { return ';'; }
                    case Keys.OemQuotes:
                        if (Shift) { return '"'; } else { return '\''; }
                    case Keys.OemQuestion:
                        if (Shift) { return '?'; } else { return '/'; }
                    case Keys.OemPlus:
                        if (Shift) { return '+'; } else { return '='; }
                    case Keys.OemPipe:
                        if (Shift) { return '|'; } else { return '\\'; }
                    case Keys.OemPeriod:
                        if (Shift) { return '>'; } else { return '.'; }
                    case Keys.OemOpenBrackets:
                        if (Shift) { return '{'; } else { return '['; }
                    case Keys.OemCloseBrackets:
                        if (Shift) { return '}'; } else { return ']'; }
                    case Keys.OemMinus:
                        if (Shift) { return '_'; } else { return '-'; }
                    case Keys.OemComma:
                        if (Shift) { return '<'; } else { return ','; }
                }

                return (Char)0;

            }
        }
    }
}
