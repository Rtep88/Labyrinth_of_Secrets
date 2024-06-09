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
    public class Label
    {
        //Vzhled
        public Vector2 pozice;
        public Vector2 velikost;
        public string text;
        public Color barva;
        public float okraj;

        public Label(Vector2 pozice, Vector2 velikost, string text, Color barva, float okraj)
        {
            this.pozice = pozice;
            this.velikost = velikost;
            this.text = text;
            this.barva = barva;
            this.okraj = okraj;
        }

        public void VykresliLabel(Hra hra, Vector2 relativniPozice, float roztahnutiSouradnic)
        {
            Vector2 pozice = this.pozice * roztahnutiSouradnic + relativniPozice;
            Vector2 velikost = this.velikost * roztahnutiSouradnic;
            float okraj = this.okraj * roztahnutiSouradnic;

            //Vykresleni textu
            Vector2 volneMisto = (velikost - new Vector2(okraj * 2)) * 0.75f;
            Vector2 velikostTextu = Hra.pixeloidSans.MeasureString(text);
            float meritkoTextu = Math.Min(volneMisto.X / velikostTextu.X, volneMisto.Y / velikostTextu.Y);
            hra.VykresliTextSOkrajem(Hra.pixeloidSans, pozice + velikost / 2 - velikostTextu * meritkoTextu / 2, text, meritkoTextu, Color.White, Color.Black, 0.07f, 8, true);
        }
    }
}
