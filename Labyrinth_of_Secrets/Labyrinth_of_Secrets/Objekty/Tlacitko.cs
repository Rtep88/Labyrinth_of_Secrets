using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Labyrinth_of_Secrets
{
    public class Tlacitko
    {
        //Vzhled
        public Vector2 pozice;
        public Vector2 velikost;
        public string text;
        public Color barva;
        public float okraj;

        //Funkcnost
        public string data = "";

        //Kliknutost
        public bool mysZmacknuta = false;
        public bool mysZmacknutaNaMe = false;
        public bool mysJeNaMe = false;

        public Tlacitko(Vector2 pozice, Vector2 velikost, string text, Color barva, string data, float okraj)
        {
            this.pozice = pozice;
            this.velikost = velikost;
            this.text = text;
            this.barva = barva;
            this.data = data;
            this.okraj = okraj;
        }

        public bool UpdatujTlacitko(MouseState stavMysi, Vector2 relativniPozice, float roztahnutiSouradnic, bool hraJeAktivni)
        {
            Vector2 pozice = this.pozice * roztahnutiSouradnic + relativniPozice;
            Vector2 velikost = this.velikost * roztahnutiSouradnic;
            bool nyniZmacknuto = false;

            mysJeNaMe = Hra.KolizeObdelniku(pozice.X, pozice.Y, velikost.X, velikost.Y, stavMysi.X, stavMysi.Y, 1, 1) && hraJeAktivni;

            if (!mysJeNaMe && mysZmacknutaNaMe)
                mysZmacknutaNaMe = false;

            if (mysJeNaMe && !mysZmacknuta && stavMysi.LeftButton == ButtonState.Pressed)
                mysZmacknutaNaMe = true;

            if (mysJeNaMe && mysZmacknutaNaMe && stavMysi.LeftButton == ButtonState.Released)
            {
                nyniZmacknuto = true;
                mysZmacknutaNaMe = false;
            }

            mysZmacknuta = stavMysi.LeftButton == ButtonState.Pressed;

            return nyniZmacknuto;
        }

        public void VykresliTlacitko(Hra hra, Vector2 relativniPozice, float roztahnutiSouradnic)
        {
            Vector2 pozice = this.pozice * roztahnutiSouradnic + relativniPozice;
            Vector2 velikost = this.velikost * roztahnutiSouradnic;
            float okraj = this.okraj * roztahnutiSouradnic;

            float nasobeniBarvy = 0.8f;
            if (mysJeNaMe)
                nasobeniBarvy = 0.6f;
            if (mysZmacknutaNaMe)
                nasobeniBarvy = 0.4f;

            //Vykresleni pozadi
            hra._spriteBatch.Draw(Hra.pixel, pozice, null, new Color(nasobeniBarvy / 3f * barva.R / 255f, nasobeniBarvy / 3f * barva.G / 255f, nasobeniBarvy / 3f * barva.B / 255f), 0, Vector2.Zero, velikost, SpriteEffects.None, 0);
            hra._spriteBatch.Draw(Hra.pixel, pozice + new Vector2(okraj), null, new Color(nasobeniBarvy * barva.R / 255f, nasobeniBarvy * barva.G / 255f, nasobeniBarvy * barva.B / 255f), 0, Vector2.Zero, velikost - new Vector2(okraj * 2), SpriteEffects.None, 0);

            //Vykresleni textu
            nasobeniBarvy += 0.2f;
            Vector2 volneMisto = (velikost - new Vector2(okraj * 2)) * 0.75f;
            Vector2 velikostTextu = Hra.pixeloidSans.MeasureString(text);
            float meritkoTextu = Math.Min(volneMisto.X / velikostTextu.X, volneMisto.Y / velikostTextu.Y);
            hra.VykresliTextSOkrajem(Hra.pixeloidSans, pozice + velikost / 2 - velikostTextu * meritkoTextu / 2, text, meritkoTextu, new Color(nasobeniBarvy, nasobeniBarvy, nasobeniBarvy), Color.Black, 0.07f, 8, true);
        }
    }
}
