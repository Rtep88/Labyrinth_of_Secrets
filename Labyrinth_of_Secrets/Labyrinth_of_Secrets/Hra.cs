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

        //Konstanty

        //Promenne
        public Point velikostOkna = new Point(1280, 720);
        public bool fullscreen = false;
        public Random rnd = new Random();
        public KeyboardState klavesy, klavesyMinule;
        public MouseState tlacitkaMysi, tlacitkaMysiMinule;

        public Hra()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            KomponentaMapa komponentaMapa = new KomponentaMapa(this);
            KomponentaKamera komponentaKamera = new KomponentaKamera(this);
            Components.Add(komponentaMapa);
            Components.Add(komponentaKamera);
            _graphics.HardwareModeSwitch = false;
            NastavRozliseni();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            klavesyMinule = klavesy;
            klavesy = Keyboard.GetState();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            base.Draw(gameTime);
        }

        //Nastavi rozliseni okna
        public void NastavRozliseni()
        {
            if (_graphics.PreferredBackBufferHeight != velikostOkna.Y || _graphics.PreferredBackBufferWidth != velikostOkna.X || _graphics.IsFullScreen != fullscreen)
            {
                _graphics.PreferredBackBufferHeight = velikostOkna.Y;
                _graphics.PreferredBackBufferWidth = velikostOkna.X;
                _graphics.IsFullScreen = fullscreen;
                _graphics.ApplyChanges();
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
    }
}