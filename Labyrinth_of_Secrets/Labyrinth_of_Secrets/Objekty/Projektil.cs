using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Labyrinth_of_Secrets
{
    public class Projektil
    {
        public enum TypProjektilu
        {
            Obycejna
        }

        public Vector2 pozice;
        public Vector2 velikost = new Vector2(1, 2);
        public Vector2 smer;
        public float rychlost = 80f;
        public int zraneni = 10;
        public TypProjektilu typProjektilu;

        public Projektil(Vector2 pozice, Vector2 smer)
        {
            this.pozice = pozice;
            this.smer = smer;
        }

        public Projektil()
        {

        }
    }
}
