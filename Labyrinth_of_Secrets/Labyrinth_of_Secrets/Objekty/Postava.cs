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
    public class Postava
    {
        public string jmeno;
        public Vector2 pozice;
        public Point velikost = new Point(KomponentaHrac.VELIKOST_HRACE_X, KomponentaHrac.VELIKOST_HRACE_Y);
        public List<List<string>> dialogy;
        public int kolikatyDialog = 0;
        public bool jeProdavac = false;

        public Postava(string jmeno, Vector2 pozice, List<List<string>> dialogy)
        {
            this.jmeno = jmeno;
            this.pozice = pozice;
            this.dialogy = dialogy;
        }

        public Postava(string jmeno, Vector2 pozice, bool jeProdavac)
        {
            this.jmeno = jmeno;
            this.pozice = pozice;
            dialogy = null;
            this.jeProdavac = jeProdavac;
        }
    }
}
