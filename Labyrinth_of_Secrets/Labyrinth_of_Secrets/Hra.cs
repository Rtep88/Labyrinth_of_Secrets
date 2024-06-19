using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace Labyrinth_of_Secrets
{
    public class Hra : Game
    {
        //Zaklad
        private GraphicsDeviceManager _graphics;
        public SpriteBatch _spriteBatch;

        //Textury a fonty
        public static SpriteFont comicSans, pixeloidSans;
        public static Texture2D pixel;

        //Konstanty

        //Enumy

        //Promenne
        public Point velikostOkna = new Point(1280, 720);
        public Point minulaVelikostOkna = new Point(1280, 720);
        public bool fullscreen = false;
        public Random rnd = new Random();
        public Keys[] aktualneZmackleKlavesy;
        public Keys[] naposledyZmackleKlavesy;
        public KeyboardState klavesy, klavesyMinule;
        public MouseState tlacitkaMysi, tlacitkaMysiMinule;
        public bool ukonceno = false;
        public KomponentaMapa komponentaMapa;
        public KomponentaZbrane komponentaZbrane;
        public KomponentaMonstra komponentaMonstra;
        public KomponentaPostavy komponentaPostavy;
        public KomponentaHrac komponentaHrac;
        public KomponentaKamera komponentaKamera;
        public KomponentaSvetlo komponentaSvetlo;
        public KomponentaKonzole komponentaKonzole;
        public KomponentaMultiplayer komponentaMultiplayer;
        public KomponentaMinimapa komponentaMinimapa;
        public KomponentaDialogy komponentaDialogy;
        public KomponentaObchod komponentaObchod;
        public KomponentaMenu komponentaMenu;
        public KomponentaHlavniMenu komponentaHlavniMenu;
        public string jmeno;

        public Hra()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            InactiveSleepTime = TimeSpan.Zero;
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += ZmenaRozliseni;
        }

        protected override void Initialize()
        {
            komponentaHlavniMenu = new KomponentaHlavniMenu(this);
            Components.Add(komponentaHlavniMenu);
            _graphics.HardwareModeSwitch = false;
            NastavRozliseni();

            string cestaKDokumentum = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string cestaKeHernimDatum = Path.Combine(new string[] { cestaKDokumentum, ".Labyrinth_of_Secrets" });

            if (!Directory.Exists(cestaKeHernimDatum))
                Directory.CreateDirectory(cestaKeHernimDatum);

            string cestaKeKonfigu = Path.Combine(cestaKeHernimDatum, "config.ini");

            jmeno = ZiskejHodnotuZIni(cestaKeKonfigu, "playerName");

            if (jmeno == null || jmeno == " " || jmeno.Length > 20 || !Regex.Match(jmeno, @"^[a-zA-Z0-9_]+$").Success)
            {
                ZapisHodnotuDoIni(cestaKeKonfigu, "playerName", "DefaultUser_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString().Substring(6));
                jmeno = ZiskejHodnotuZIni(cestaKeKonfigu, "playerName");
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            comicSans = Content.Load<SpriteFont>("Fonts/ComicSansMS");
            pixeloidSans = Content.Load<SpriteFont>("Fonts/PixeloidSans");
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new Color[] { Color.White });

            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            naposledyZmackleKlavesy = aktualneZmackleKlavesy;
            aktualneZmackleKlavesy = Keyboard.GetState().GetPressedKeys();
            klavesyMinule = klavesy;
            tlacitkaMysiMinule = tlacitkaMysi;
            klavesy = Keyboard.GetState();
            tlacitkaMysi = Mouse.GetState();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            base.Draw(gameTime);
        }

        //Kolize obdelniku
        public static bool KolizeObdelniku(float x1, float y1, float width1, float height1, float x2, float y2, float width2, float height2)
        {
            return x1 < x2 + width2 &&
                   x1 + width1 > x2 &&
                   y1 < y2 + height2 &&
                   y1 + height1 > y2;
        }

        public static bool KolizeCarySObdelnikem(Vector2 bod1, Vector2 bod2, Rectangle obdelnik)
        {
            return KolizeCarySCarou(bod1, bod2, new Vector2(obdelnik.X, obdelnik.Y), new Vector2(obdelnik.X + obdelnik.Width, obdelnik.Y)) ||
                   KolizeCarySCarou(bod1, bod2, new Vector2(obdelnik.X + obdelnik.Width, obdelnik.Y), new Vector2(obdelnik.X + obdelnik.Width, obdelnik.Y + obdelnik.Height)) ||
                   KolizeCarySCarou(bod1, bod2, new Vector2(obdelnik.X + obdelnik.Width, obdelnik.Y + obdelnik.Height), new Vector2(obdelnik.X, obdelnik.Y + obdelnik.Height)) ||
                   KolizeCarySCarou(bod1, bod2, new Vector2(obdelnik.X, obdelnik.Y + obdelnik.Height), new Vector2(obdelnik.X, obdelnik.Y)) ||
                   (obdelnik.Contains(bod1) && obdelnik.Contains(bod2));
        }

        private static bool KolizeCarySCarou(Vector2 cara1bod1, Vector2 cara1bod2, Vector2 cara2bod1, Vector2 cara2bod2)
        {
            float q = (cara1bod1.Y - cara2bod1.Y) * (cara2bod2.X - cara2bod1.X) - (cara1bod1.X - cara2bod1.X) * (cara2bod2.Y - cara2bod1.Y);
            float d = (cara1bod2.X - cara1bod1.X) * (cara2bod2.Y - cara2bod1.Y) - (cara1bod2.Y - cara1bod1.Y) * (cara2bod2.X - cara2bod1.X);

            if (d == 0)
            {
                return false;
            }

            float r = q / d;

            q = (cara1bod1.Y - cara2bod1.Y) * (cara1bod2.X - cara1bod1.X) - (cara1bod1.X - cara2bod1.X) * (cara1bod2.Y - cara1bod1.Y);
            float s = q / d;

            if (r < 0 || r > 1 || s < 0 || s > 1)
            {
                return false;
            }

            return true;
        }

        //Ukonci bezici vlakna pri vypnuti hry
        protected override void OnExiting(object sender, EventArgs args)
        {
            ukonceno = true;

            if (komponentaHlavniMenu.stavMenu == KomponentaHlavniMenu.StavMenu.Hra && komponentaMapa.jmenoSveta != null && komponentaMapa.jmenoSveta != "")
            {
                komponentaMapa.UlozMapuNaDisk(komponentaMapa.jmenoSveta);
            }

            base.OnExiting(sender, args);
        }

        //Nastavi rozliseni okna
        public void NastavRozliseni()
        {
            if (!fullscreen)
            {
                if (_graphics.IsFullScreen)
                    velikostOkna = minulaVelikostOkna;
                _graphics.PreferredBackBufferWidth = (int)velikostOkna.X;
                _graphics.PreferredBackBufferHeight = (int)velikostOkna.Y;
            }
            else
            {
                _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                if (!_graphics.IsFullScreen)
                    minulaVelikostOkna = velikostOkna;
                velikostOkna.X = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                velikostOkna.Y = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }
            _graphics.IsFullScreen = fullscreen;
            _graphics.ApplyChanges();

            NastavKameru();
        }

        private void NastavKameru()
        {
            if (komponentaKamera != null)
            {
                if (komponentaKamera._kamera == null)
                    komponentaKamera._kamera = new Kamera(GraphicsDevice.Viewport, 1);
                else
                    komponentaKamera._kamera = new Kamera(GraphicsDevice.Viewport, komponentaKamera._kamera.zoom);
            }
        }

        //Vrati zda je funkce zmacknuta poprve
        public bool NoveZmacknutaKlavesa(Keys klavesa)
        {
            return klavesy.IsKeyDown(klavesa) && klavesyMinule.IsKeyUp(klavesa);
        }

        //Vrati zda je funkce zmacknuta poprve
        public bool NoveZmacknuteLeveTlacitko()
        {
            return tlacitkaMysi.LeftButton == ButtonState.Pressed && tlacitkaMysiMinule.LeftButton == ButtonState.Released;
        }

        //Nahodne prohazi polozky v listu
        public void PromichejList<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        //Vrátí podpole podle indexu a délky
        public static T[] SubArray<T>(T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        //Vrati novou texturu obdelniku s okrajem se zadanymi parametry
        public Texture2D VytvorTexturuObdelnikuSOkrejem(int sirka, int vyska, int tloustkaOkraje, Color barvaObdelniku)
        {
            Texture2D obdelnik = new Texture2D(GraphicsDevice, sirka, vyska);
            Color[] barvyObdelniku = new Color[sirka * vyska];
            for (int y = 0; y < vyska; y++)
            {
                for (int x = 0; x < sirka; x++)
                {
                    if (x < tloustkaOkraje || x >= sirka - tloustkaOkraje ||
                        y < tloustkaOkraje || y >= vyska - tloustkaOkraje)
                    {
                        barvyObdelniku[y * sirka + x] = new Color(0, 0, 0, 255);
                    }
                    else
                    {
                        barvyObdelniku[y * sirka + x] = barvaObdelniku;
                    }
                }
            }
            obdelnik.SetData(barvyObdelniku);
            return obdelnik;
        }

        //Vykresli text s okrajem
        public void VykresliTextSOkrajem(SpriteFont font, Vector2 pozice, string text, float scale, Color barva, Color barvaObrysu, float tloustkaOkraje, int presnost, bool pixelovyFont)
        {
            Vector2 pozice2 = pozice;

            for (int j = 0; j < text.Length; j++)
            {
                //Okraj
                for (int i = 0; i < presnost; i++)
                {
                    Vector2 posun = new Vector2((float)Math.Sin(i * Math.PI * 2 / presnost), (float)Math.Cos(i * Math.PI * 2 / presnost));
                    if (pixelovyFont)
                    {
                        float delitel = Math.Max(Math.Abs(posun.X), Math.Abs(posun.Y));
                        posun.X /= delitel;
                        posun.Y /= delitel;
                    }
                    _spriteBatch.DrawString(font, text[j].ToString(), pozice2 + new Vector2(font.MeasureString(text[j].ToString()).Y) * scale * tloustkaOkraje * posun, barvaObrysu, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                }
                pozice2 += new Vector2(font.MeasureString(text[j].ToString()).X * scale, 0);
            }
            pozice2 = pozice;
            for (int j = 0; j < text.Length; j++)
            {
                //Text
                _spriteBatch.DrawString(font, text[j].ToString(), pozice2, barva, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                pozice2 += new Vector2(font.MeasureString(text[j].ToString()).X * scale, 0);

            }
        }

        //Vypocita potrebnou rotaci primnky AB okolo originu v radianech tak, aby se ji bod C co nejvice priblizoval
        public static float VypocitejRotaci(Vector2 origin, Vector2 bodA, Vector2 bodB, Vector2 bodC)
        {
            float idealniRotace = (float)Math.Atan2(bodC.Y - origin.Y, bodC.X - origin.X);
            float krok = (float)Math.PI / 4f;

            bodA = RotaceBodu(bodA, origin, idealniRotace);
            bodB = RotaceBodu(bodB, origin, idealniRotace);

            //Binarni vyhledavani rotace
            while (krok > float.Epsilon)
            {
                float vzdalenost1 = VypocitejVzdalenostBoduOdPrimky(RotaceBodu(bodA, origin, -krok), RotaceBodu(bodB, origin, -krok), bodC);
                float vzdalenost2 = VypocitejVzdalenostBoduOdPrimky(RotaceBodu(bodA, origin, krok), RotaceBodu(bodB, origin, krok), bodC);
                float vzdalenost3 = VypocitejVzdalenostBoduOdPrimky(RotaceBodu(bodA, origin, 0), RotaceBodu(bodB, origin, 0), bodC);

                if (Math.Min(vzdalenost1, Math.Min(vzdalenost2, vzdalenost3)) == vzdalenost1)
                {
                    idealniRotace -= krok;
                    bodA = RotaceBodu(bodA, origin, -krok);
                    bodB = RotaceBodu(bodB, origin, -krok);
                }
                else if (Math.Min(vzdalenost1, Math.Min(vzdalenost2, vzdalenost3)) == vzdalenost2)
                {
                    idealniRotace += krok;
                    bodA = RotaceBodu(bodA, origin, krok);
                    bodB = RotaceBodu(bodB, origin, krok);
                }
                krok /= 2f;
            }

            return idealniRotace;
        }

        //Vrati vzdalenost bodu C od primky AB
        public static float VypocitejVzdalenostBoduOdPrimky(Vector2 bodA, Vector2 bodB, Vector2 bodC)
        {
            double A = bodB.Y - bodA.Y;
            double B = bodA.X - bodB.X;
            double C = bodB.X * bodA.Y - bodA.X * bodB.Y;

            return (float)(Math.Abs(A * bodC.X + B * bodC.Y + C) / Math.Sqrt(A * A + B * B));
        }

        //Otoci bod kolem originu o urcity pocet radianu
        public static Vector2 RotaceBodu(Vector2 bodA, Vector2 origin, float rotace)
        {
            Vector2 posunutyBod = bodA - origin;
            Vector2 otocenyBod = new Vector2(
                posunutyBod.X * (float)Math.Cos(rotace) - posunutyBod.Y * (float)Math.Sin(rotace),
                posunutyBod.X * (float)Math.Sin(rotace) + posunutyBod.Y * (float)Math.Cos(rotace)
            );

            return otocenyBod + origin;
        }

        public void ZmenaRozliseni(Object sender, EventArgs e)
        {
            if ((_graphics.PreferredBackBufferWidth != _graphics.GraphicsDevice.Viewport.Width) ||
                (_graphics.PreferredBackBufferHeight != _graphics.GraphicsDevice.Viewport.Height))
            {
                _graphics.PreferredBackBufferWidth = _graphics.GraphicsDevice.Viewport.Width;
                _graphics.PreferredBackBufferHeight = _graphics.GraphicsDevice.Viewport.Height;
                _graphics.ApplyChanges();

                velikostOkna.X = _graphics.PreferredBackBufferWidth;
                velikostOkna.Y = _graphics.PreferredBackBufferHeight;
            }
        }

        //Spusti komponenty co zajistuji chod hru
        public void SpustKomponentyHry()
        {
            komponentaMapa = new KomponentaMapa(this);
            komponentaZbrane = new KomponentaZbrane(this);
            komponentaMonstra = new KomponentaMonstra(this);
            komponentaPostavy = new KomponentaPostavy(this);
            komponentaMultiplayer = new KomponentaMultiplayer(this);
            komponentaHrac = new KomponentaHrac(this);
            komponentaKamera = new KomponentaKamera(this);
            komponentaSvetlo = new KomponentaSvetlo(this);
            komponentaMinimapa = new KomponentaMinimapa(this);
            komponentaKonzole = new KomponentaKonzole(this);
            komponentaDialogy = new KomponentaDialogy(this);
            komponentaObchod = new KomponentaObchod(this);
            komponentaMenu = new KomponentaMenu(this);
            Components.Add(komponentaMapa);
            Components.Add(komponentaZbrane);
            Components.Add(komponentaMonstra);
            Components.Add(komponentaPostavy);
            Components.Add(komponentaMultiplayer);
            Components.Add(komponentaHrac);
            Components.Add(komponentaKamera);
            Components.Add(komponentaSvetlo);
            Components.Add(komponentaMinimapa);
            Components.Add(komponentaDialogy);
            Components.Add(komponentaObchod);
            Components.Add(komponentaKonzole);
            Components.Add(komponentaMenu);

            NastavKameru();
        }

        //Vypne komponenty co zajistuji chod hru
        public void VypniKomponentyHry()
        {
            if (komponentaSvetlo != null)
                komponentaSvetlo.ZastavPocitaniSvetla();

            Components.Remove(komponentaMapa);
            Components.Remove(komponentaZbrane);
            Components.Remove(komponentaMonstra);
            Components.Remove(komponentaPostavy);
            Components.Remove(komponentaMultiplayer);
            Components.Remove(komponentaHrac);
            Components.Remove(komponentaKamera);
            Components.Remove(komponentaSvetlo);
            Components.Remove(komponentaMinimapa);
            Components.Remove(komponentaDialogy);
            Components.Remove(komponentaObchod);
            Components.Remove(komponentaKonzole);
            Components.Remove(komponentaMenu);
        }

        //Ukonci celou hru
        public void VypniHru()
        {
            if (komponentaSvetlo != null)
                komponentaSvetlo.ZastavPocitaniSvetla();

            ukonceno = true;

            Exit();
        }

        public static bool Contains(Keys[] klavesy, Keys klavesa)
        {
            foreach (Keys klavesa2 in klavesy)
            {
                if (klavesa == klavesa2)
                    return true;
            }
            return false;
        }

        public static char KeyToChar(Keys Key, bool Shift = false)
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

        public static void ZapisHodnotuDoIni(string cesta, string klic, string hodnota)
        {
            if (File.Exists(cesta))
            {
                try
                {
                    List<string> radkySouboru = File.ReadAllLines(cesta).ToList();
                    bool nalezeno = false;
                    for (int i = 0; i < radkySouboru.Count; i++)
                    {
                        string klicVSouboru = radkySouboru[i].Split('=')[0];
                        if (klicVSouboru == klic)
                        {
                            radkySouboru[i] = klic + "=" + hodnota;
                            nalezeno = true;
                        }
                    }
                    if (!nalezeno)
                        radkySouboru.Add(klic + "=" + hodnota);

                    File.WriteAllLines(cesta, radkySouboru);
                }
                catch
                {
                    throw new Exception("Config file is not in correct format!");
                }
            }
            else
            {
                File.WriteAllText(cesta, klic + "=" + hodnota);
            }
        }

        public static string ZiskejHodnotuZIni(string cesta, string klic)
        {
            if (File.Exists(cesta))
            {
                try
                {
                    List<string> radkySouboru = File.ReadAllLines(cesta).ToList();
                    for (int i = 0; i < radkySouboru.Count; i++)
                    {
                        string klicVSouboru = radkySouboru[i].Split('=')[0];
                        if (klicVSouboru == klic)
                            return radkySouboru[i].Split('=')[1];
                    }
                    return null;
                }
                catch
                {
                    throw new Exception("Config file is not in correct format!");
                }
            }
            else
                return null;
        }
    
    }
}