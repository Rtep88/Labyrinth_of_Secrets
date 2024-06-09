using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Labyrinth_of_Secrets
{
    public class Textbox
    {
        //Vzhled
        public Vector2 pozice;
        public Vector2 velikost;
        public string text = "";
        public Color barva;
        public float okraj;

        //Funkcnost
        public string data = "";
        private int poziceKurzoru = 0;
        private float casKurzoru = 0.99f;

        //Kliknutost
        public bool mysZmacknuta = false;
        public bool mysZmacknutaNaMe = false;
        public bool mysJeNaMe = false;
        public bool jsemOznacen = false;

        public Textbox(Vector2 pozice, Vector2 velikost, Color barva, string data, float okraj)
        {
            this.pozice = pozice;
            this.velikost = velikost;
            this.barva = barva;
            this.data = data;
            this.okraj = okraj;
        }

        public void UpdatujTextbox(Hra hra, GameTime gameTime, MouseState stavMysi, Vector2 relativniPozice, float roztahnutiSouradnic, bool hraJeAktivni)
        {
            if (jsemOznacen)
                casKurzoru += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (casKurzoru >= 1f)
                casKurzoru = 0;

            Vector2 pozice = this.pozice * roztahnutiSouradnic + relativniPozice;
            Vector2 velikost = this.velikost * roztahnutiSouradnic;

            mysJeNaMe = Hra.KolizeObdelniku(pozice.X, pozice.Y, velikost.X, velikost.Y, stavMysi.X, stavMysi.Y, 1, 1) && hraJeAktivni;

            if (!mysJeNaMe && mysZmacknutaNaMe)
                mysZmacknutaNaMe = false;

            if (mysJeNaMe && !mysZmacknuta && stavMysi.LeftButton == ButtonState.Pressed)
                mysZmacknutaNaMe = true;

            if (mysJeNaMe && mysZmacknutaNaMe && stavMysi.LeftButton == ButtonState.Released)
            {
                jsemOznacen = true;
                mysZmacknutaNaMe = false;
            }

            if (!mysJeNaMe && mysZmacknuta && stavMysi.LeftButton == ButtonState.Released)
            {
                jsemOznacen = false;
                casKurzoru = 0.99f;
                poziceKurzoru = text.Length;
            }

            mysZmacknuta = stavMysi.LeftButton == ButtonState.Pressed;

            if (jsemOznacen)
            {
                Keys[] zmackleKlavesy = Keyboard.GetState().GetPressedKeys();
                foreach (Keys klavesa in zmackleKlavesy)
                {
                    char znak = Hra.KeyToChar(klavesa, (Keyboard.GetState().CapsLock ^ Keyboard.GetState().IsKeyDown(Keys.LeftShift)));
                    if (!Hra.Contains(hra.naposledyZmackleKlavesy, klavesa) && (znak >= 32 || klavesa == Keys.Left || klavesa == Keys.Right || klavesa == Keys.Back || klavesa == Keys.Delete))
                    {
                        if (klavesa != Keys.Left && klavesa != Keys.Right && klavesa != Keys.Back && klavesa != Keys.Delete)
                        {
                            text = text.Substring(0, poziceKurzoru) + znak + text.Substring(poziceKurzoru);
                            poziceKurzoru++;
                        }
                        else if (klavesa == Keys.Back && text.Length > 0 && poziceKurzoru != 0)
                        {
                            text = text.Substring(0, poziceKurzoru - 1) + text.Substring(poziceKurzoru);
                            poziceKurzoru--;
                        }
                        else if (klavesa == Keys.Delete && text.Length > 0 && poziceKurzoru != text.Length)
                        {
                            text = text.Substring(0, poziceKurzoru) + text.Substring(poziceKurzoru + 1);
                        }
                        else if (klavesa == Keys.Left && poziceKurzoru != 0)
                            poziceKurzoru--;
                        else if (klavesa == Keys.Right && poziceKurzoru != text.Length)
                            poziceKurzoru++;
                    }
                }
            }
        }

        public void VykresliTextbox(Hra hra, Vector2 relativniPozice, float roztahnutiSouradnic)
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
            hra._spriteBatch.Draw(Hra.pixel, pozice + new Vector2(okraj), null, new Color(1.4f * barva.R / 255f, 1.4f * barva.G / 255f, 1.4f * barva.B / 255f), 0, Vector2.Zero, velikost - new Vector2(okraj * 2), SpriteEffects.None, 0);

            //Vykresleni textu
            string textPredKurzorem = text.Substring(0, poziceKurzoru);
            string textZaKurzorem = text.Substring(poziceKurzoru);
            Vector2 velikostTextuPredKurzorem = Hra.pixeloidSans.MeasureString(textPredKurzorem);
            Vector2 velikostTextuZaKurzorem = Hra.pixeloidSans.MeasureString(textZaKurzorem);
            Vector2 volneMisto = (velikost - new Vector2(okraj * 2)) * 0.75f;
            Vector2 velikostTextu = Hra.pixeloidSans.MeasureString(textPredKurzorem + " " + textZaKurzorem);
            float meritkoTextu = Math.Min(volneMisto.X / velikostTextu.X, volneMisto.Y / velikostTextu.Y);
            hra.VykresliTextSOkrajem(Hra.pixeloidSans, pozice + velikost / 2 - velikostTextu * meritkoTextu / 2, textPredKurzorem,
                meritkoTextu, Color.White, Color.Black, 0.07f, 8, true);
            if (casKurzoru < 0.5)
                hra.VykresliTextSOkrajem(Hra.pixeloidSans, pozice + velikost / 2 - velikostTextu * meritkoTextu / 2 +
                    new Vector2(velikostTextuPredKurzorem.X + Hra.pixeloidSans.MeasureString(" ").X / 2f - Hra.pixeloidSans.MeasureString("|").X / 2f, 0) * meritkoTextu,
                    "|", meritkoTextu, Color.White, Color.Black, 0.07f, 8, true);
            hra.VykresliTextSOkrajem(Hra.pixeloidSans, pozice + velikost / 2 - velikostTextu * meritkoTextu / 2 +
                new Vector2((velikostTextu.X - velikostTextuZaKurzorem.X) * meritkoTextu, 0),
                textZaKurzorem, meritkoTextu, Color.White, Color.Black, 0.07f, 8, true);
        }

        public static string VratTextPodleDat(List<Textbox> textboxy, string data)
        {
            foreach (Textbox textbox in textboxy)
            {
                if (textbox.data == data)
                    return textbox.text;
            }

            return "";
        }
    }
}
