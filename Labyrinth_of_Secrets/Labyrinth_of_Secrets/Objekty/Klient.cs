using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Labyrinth_of_Secrets
{
    public class Klient
    {
        public IPEndPoint ipAdresa;
        public string jmeno;
        public ulong casPosledniOdpovedi;

        public Klient(IPEndPoint ipAdresa, string jmeno, ulong casPosledniOdpovedi)
        {
            this.ipAdresa = ipAdresa;
            this.jmeno = jmeno;
            this.casPosledniOdpovedi = casPosledniOdpovedi;
        }
    }
}
