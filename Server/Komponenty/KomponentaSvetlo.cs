using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Labyrinth_of_Secrets
{
    public class KomponentaSvetlo
    {
        //Zaklad
        private Hra hra;

        //Struktury
        public struct ZdrojSvetla
        {
            public Point odkud;
            public int silaSvetla;
            public Color barvaSvetla;

            public ZdrojSvetla(Point odkud, int silaSvetla, Color barvaSvetla)
            {
                this.odkud = odkud;
                this.silaSvetla = silaSvetla;
                this.barvaSvetla = barvaSvetla;
            }
        }

        //Konstanty
        private const int POCET_SVETLA_NA_BLOK = 8; //Udava jak moc dopodrobna se svetlo bude pocitat - Minimum je 1 a maximum je omezen pouze systemovymi prostredky ale doporucuji neprekracovat 32 :)

        //Promene
        public List<ZdrojSvetla> svetelneZdroje = new List<ZdrojSvetla>();

        public KomponentaSvetlo(Hra hra)
        {
            this.hra = hra;
        }

        //Nastavi blokum na mape aby svitily
        public void PridejZdrojeSvetla()
        {
            for (int y = 0; y < KomponentaMapa.VELIKOST_MAPY_Y; y++)
            {
                for (int x = 0; x < KomponentaMapa.VELIKOST_MAPY_X; x++)
                {
                    if (hra.komponentaMapa.mapa[x, y].zdrojSvetla > 0)
                        svetelneZdroje.Add(new ZdrojSvetla(new Point(x, y) * new Point(POCET_SVETLA_NA_BLOK) + new Point(POCET_SVETLA_NA_BLOK / 2), hra.komponentaMapa.mapa[x, y].zdrojSvetla, hra.komponentaMapa.mapa[x, y].barvaSvetla));
                }
            }
        }
    }
}
