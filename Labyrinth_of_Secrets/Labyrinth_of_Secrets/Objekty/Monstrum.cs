using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Labyrinth_of_Secrets
{
    public class Monstrum
    {
        public Vector2 pozice;
        public Point velikost = new Point(KomponentaHrac.VELIKOST_HRACE_X, KomponentaHrac.VELIKOST_HRACE_Y);
        public int typMonstra;
        public float rychlost = 20f;

        public int maxZivoty = 100;
        public int zivoty;

        public Monstrum(Vector2 pozice)
        {
            this.pozice = pozice;
            rychlost *= new Random().Next(700, 1300) / 1000f;
            zivoty = maxZivoty;
        }

        public Monstrum()
        {

        }
    }
}
