using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labyrinth_of_Secrets
{
    public class Pole
    {
        public enum TypPole
        {
            Prazdne,
            Zed,
            Obchod
        }
        public TypPole typPole = TypPole.Prazdne;

        public Pole(TypPole typPole)
        {
            this.typPole = typPole;
        }
    }
}
