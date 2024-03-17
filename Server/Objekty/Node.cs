using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labyrinth_of_Secrets
{
    public class Node
    {
        public Point pozice;
        public List<Node> deti;
        public List<ushort> cesta;
        public int index;

        public Node(Point pozice, List<ushort> cesta)
        {
            this.pozice = pozice;
            this.cesta = cesta;
            deti = new List<Node>();
        }
    }
}
