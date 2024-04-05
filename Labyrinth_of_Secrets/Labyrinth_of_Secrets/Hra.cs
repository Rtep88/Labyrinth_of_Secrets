using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Labyrinth_of_Secrets
{
    public class Hra : Game
    {
        //Zaklad
        private GraphicsDeviceManager _graphics;
        public SpriteBatch _spriteBatch;

        //Textury a fonty
        public static SpriteFont comicSans;
        public static Texture2D pixel;

        //Konstanty

        //Promenne
        public Point velikostOkna = new Point(1280, 720);
        public Point minulaVelikostOkna = new Point(1280, 720);
        public bool fullscreen = false;
        public Random rnd = new Random();
        public KeyboardState klavesy, klavesyMinule;
        public MouseState tlacitkaMysi, tlacitkaMysiMinule;
        public bool ukonceno = false;
        public KomponentaMapa komponentaMapa;
        public KomponentaZbrane komponentaZbrane;
        public KomponentaMonstra komponentaMonstra;
        public KomponentaHrac komponentaHrac;
        public KomponentaKamera komponentaKamera;
        public KomponentaSvetlo komponentaSvetlo;
        public KomponentaKonzole komponentaKonzole;
        public KomponentaMultiplayer komponentaMultiplayer;
        public KomponentaMenu komponentaMenu;

        public Hra()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            InactiveSleepTime = TimeSpan.Zero;
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            komponentaMapa = new KomponentaMapa(this);
            komponentaZbrane = new KomponentaZbrane(this);
            komponentaMonstra = new KomponentaMonstra(this);
            komponentaMultiplayer = new KomponentaMultiplayer(this);
            komponentaHrac = new KomponentaHrac(this);
            komponentaKamera = new KomponentaKamera(this);
            komponentaSvetlo = new KomponentaSvetlo(this);
            komponentaKonzole = new KomponentaKonzole(this);
            komponentaMenu = new KomponentaMenu(this);
            Components.Add(komponentaMapa);
            Components.Add(komponentaZbrane);
            Components.Add(komponentaMonstra);
            Components.Add(komponentaMultiplayer);
            Components.Add(komponentaHrac);
            Components.Add(komponentaKamera);
            Components.Add(komponentaSvetlo);
            Components.Add(komponentaKonzole);
            Components.Add(komponentaMenu);
            _graphics.HardwareModeSwitch = false;
            NastavRozliseni();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            comicSans = Content.Load<SpriteFont>("Fonts/ComicSansMS");
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new Color[] { Color.White });

            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
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

        //Ukonci bezici vlakna pri vypnuti hry
        protected override void OnExiting(object sender, EventArgs args)
        {
            ukonceno = true;

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

            if (komponentaKamera._kamera == null)
                komponentaKamera._kamera = new Kamera(GraphicsDevice.Viewport, 1);
            else
                komponentaKamera._kamera = new Kamera(GraphicsDevice.Viewport, komponentaKamera._kamera.zoom);
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
        public void VykresliTextSOkrajem(SpriteFont font, Vector2 pozice, string text, float scale, Color barva, Color barvaObrysu, float tloustkaOkraje, int presnost)
        {
            Vector2 pozice2 = pozice;

            for (int j = 0; j < text.Length; j++)
            {
                //Okraj
                for (int i = 0; i < presnost; i++)
                {
                    _spriteBatch.DrawString(font, text[j].ToString(), pozice2 + new Vector2(font.MeasureString(text[j].ToString()).Y) * scale * tloustkaOkraje * new Vector2((float)Math.Sin(i * Math.PI * 2 / presnost), (float)Math.Cos(i * Math.PI * 2 / presnost)), barvaObrysu, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
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
    }
}