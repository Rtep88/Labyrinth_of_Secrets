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
        public Point velikost = new Point(6, 12);
        public int typMonstra;
        public float rychlost = 20f;
        public string sledovanyHrac = "";
        public LinkedList<Point> cesta = new LinkedList<Point>();

        public Monstrum(Vector2 pozice)
        {
            this.pozice = pozice;
            cesta = new LinkedList<Point>();
            cesta.AddFirst((pozice + velikost.ToVector2() / 2).ToPoint() / new Point(KomponentaMapa.VELIKOST_BLOKU));
            rychlost *= new Random().Next(700, 1300) / 1000f;
        }

        public Monstrum()
        {

        }
    }
}
