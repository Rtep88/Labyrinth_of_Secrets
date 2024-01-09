using Microsoft.Xna.Framework;
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
            Obchod,
            Obchodnik,
            Start,
            Vychod
        }
        public TypPole typPole = TypPole.Prazdne;
        public bool naHlavniCeste = false;
        public int zdrojSvetla = 0;
        public Color barvaSvetla = Color.Black;
        public int neprusvitnost = 10;
        public bool statickaNeprusvitnost = false;

        public Pole(TypPole typPole)
        {
            this.typPole = typPole;
            switch (typPole)
            {
                case TypPole.Prazdne:
                    break;
                case TypPole.Zed:
                    neprusvitnost = 60;
                    statickaNeprusvitnost = true;
                    break;
                case TypPole.Obchod:
                    break;
                case TypPole.Obchodnik:
                    zdrojSvetla = 300;
                    barvaSvetla = new Color(new Random().Next(0, 256), new Random().Next(0, 256), new Random().Next(0, 256));
                    break;
                case TypPole.Start:
                    break;
                case TypPole.Vychod:
                    zdrojSvetla = 500;
                    barvaSvetla = Color.White;
                    break;
                default:
                    break;
            }
        }
    }
}
