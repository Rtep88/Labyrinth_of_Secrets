using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Labyrinth_of_Secrets
{
    public class Radek
    {
        public string text = "";
        public Color barva;

        public Radek(string Text, Color Barva)
        {
            text = Text;
            barva = Barva;
        }

        public Radek(string Text, int Cas, Color Barva)
        {
            text = Text;
            barva = Barva;
        }
    }
}
