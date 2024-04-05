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
            Normalni,
            Odstrelovaci,
            Lehka
        }

        public Vector2 pozice;
        public Vector2 velikost;
        public Vector2 smer;
        public float rychlost;
        public int zraneni;
        public TypProjektilu typProjektilu;

        public Projektil(Vector2 pozice, Vector2 smer, TypProjektilu typProjektilu)
        {
            this.pozice = pozice;
            this.smer = smer;
            this.typProjektilu = typProjektilu;

            switch (typProjektilu)
            {
                case TypProjektilu.Normalni:
                    velikost = new Vector2(1, 2);
                    rychlost = 110f;
                    zraneni = 10;
                    break;
                case TypProjektilu.Odstrelovaci:
                    velikost = new Vector2(1, 3);
                    rychlost = 160f;
                    zraneni = 60;
                    break;
                case TypProjektilu.Lehka:
                    velikost = new Vector2(1, 1);
                    rychlost = 80f;
                    zraneni = 4;
                    break;
            }
        }

        public Projektil()
        {

        }

        public void PohniSe(float deltaTime)
        {
            pozice += smer * rychlost * deltaTime;
        }
    }
}
