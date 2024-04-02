using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Labyrinth_of_Secrets
{
    public class Hrac
    {
        public string jmeno;
        public Vector2 pozice;
        public Vector2 poziceMysi;
        public Zbran.TypZbrane vybranaZbran;

        public Hrac(string jmeno)
        {
            this.jmeno = jmeno;
        }
    }
}
