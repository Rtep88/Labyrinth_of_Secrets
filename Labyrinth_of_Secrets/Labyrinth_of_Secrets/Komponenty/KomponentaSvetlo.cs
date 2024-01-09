using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Labyrinth_of_Secrets
{
    class KomponentaSvetlo : DrawableGameComponent
    {
        //Zaklad
        private Hra hra;

        //Textury
        private Texture2D svetlo;
        
        //Struktury
        private struct ZdrojSvetla
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
        private const int POCET_SVETLA_NA_BLOK = 16; //Udava jak moc dopodrobna se svetlo bude pocitat - Minimum je 1 a maximum je omezen pouze systemovymi prostredky ale doporucuji neprekracovat 32 :)
        private static Color GLOBALNI_BARVA = new Color(0, 0, 0);
        private static Color BARVA_SVETLA_CURSORU = new Color(255, 255, 255);
        private const int SILA_SVETLA_CURSORU = 300;

        //Promene
        private Color[] dataSvetlaProVypocet;
        private Color[] dataSvetlaProVykresleni;
        private static List<ZdrojSvetla> svetelneZdroje = new List<ZdrojSvetla>();
        private List<Point>[] bodyNaSmazani = new List<Point>[2];
        private bool vykreslovani = false;
        private bool vymenovani = false;

        public KomponentaSvetlo(Hra hra) : base(hra)
        {
            this.hra = hra;
        }

        public override void Initialize()
        {
            dataSvetlaProVypocet = new Color[KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK * KomponentaMapa.VELIKOST_MAPY_Y * POCET_SVETLA_NA_BLOK];
            dataSvetlaProVykresleni = new Color[KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK * KomponentaMapa.VELIKOST_MAPY_Y * POCET_SVETLA_NA_BLOK];
            svetlo = new Texture2D(GraphicsDevice, KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK, KomponentaMapa.VELIKOST_MAPY_Y * POCET_SVETLA_NA_BLOK, false, SurfaceFormat.Color);
            bodyNaSmazani[0] = new List<Point>();
            bodyNaSmazani[1] = new List<Point>();
            Thread pocitaniSvetla = new Thread(PocitejSvetlo);
            pocitaniSvetla.Start();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            BlendState Multiply = new BlendState()
            {
                AlphaSourceBlend = Blend.DestinationAlpha,
                AlphaDestinationBlend = Blend.Zero,
                AlphaBlendFunction = BlendFunction.Add,
                ColorSourceBlend = Blend.DestinationColor,
                ColorDestinationBlend = Blend.Zero,
                ColorBlendFunction = BlendFunction.Add
            };

            while (vymenovani) { Thread.Sleep(10); }

            vykreslovani = true;
            svetlo.SetData(dataSvetlaProVykresleni);
            hra._spriteBatch.Begin(SpriteSortMode.Deferred, Multiply, transformMatrix: KomponentaKamera._kamera.GetViewMatrix());
            hra._spriteBatch.Draw(svetlo, new Rectangle(0, 0, KomponentaMapa.VELIKOST_MAPY_X * KomponentaMapa.VELIKOST_BLOKU, KomponentaMapa.VELIKOST_MAPY_Y * KomponentaMapa.VELIKOST_BLOKU), Color.White);
            hra._spriteBatch.End();
            vykreslovani = false;

            base.Draw(gameTime);
        }

        //Nastavi blokum na mape aby svitily
        static public void PridejZdrojeSvetla()
        {
            for (int y = 0; y < KomponentaMapa.VELIKOST_MAPY_Y; y++)
            {
                for (int x = 0; x < KomponentaMapa.VELIKOST_MAPY_X; x++)
                {
                    if (KomponentaMapa.mapa[x, y].zdrojSvetla > 0)
                        svetelneZdroje.Add(new ZdrojSvetla(new Point(x, y) * new Point(POCET_SVETLA_NA_BLOK) + new Point(POCET_SVETLA_NA_BLOK / 2), KomponentaMapa.mapa[x, y].zdrojSvetla, KomponentaMapa.mapa[x, y].barvaSvetla));
                }
            }
        }

        //Nekonecna smycka co pocita svetlo v novem vlakne
        public void PocitejSvetlo()
        {
            while (!hra.ukonceno)
            {                
                Vector2 opravdovaPoziceKamery = new Vector2(-KomponentaKamera._kamera.GetViewMatrix().Translation.X / KomponentaKamera._kamera.zoom, -KomponentaKamera._kamera.GetViewMatrix().Translation.Y / KomponentaKamera._kamera.zoom);
                Vector2 opravdovaVelikostOkna = new Vector2(hra.velikostOkna.X / KomponentaKamera._kamera.zoom, hra.velikostOkna.Y / KomponentaKamera._kamera.zoom);
                Point poziceMysi = ((opravdovaPoziceKamery + Mouse.GetState().Position.ToVector2() * opravdovaVelikostOkna / hra.velikostOkna.ToVector2()) / KomponentaMapa.VELIKOST_BLOKU * new Vector2(POCET_SVETLA_NA_BLOK)).ToPoint();

                foreach (Point odkudSmazat in bodyNaSmazani[0])
                {
                    SmazSvetloOdBodu(odkudSmazat);
                }
                bodyNaSmazani[0] = new List<Point>();

                PridejSvetloOdBodu(poziceMysi, SILA_SVETLA_CURSORU, BARVA_SVETLA_CURSORU, true);

                foreach (ZdrojSvetla svetlo in svetelneZdroje)
                {
                    PridejSvetloOdBodu(svetlo.odkud, svetlo.silaSvetla, svetlo.barvaSvetla, false);
                }

                while (vykreslovani) { Thread.Sleep(10); }

                vymenovani = true;
                Color[] docasnePole = dataSvetlaProVykresleni;
                dataSvetlaProVykresleni = dataSvetlaProVypocet;
                dataSvetlaProVypocet = docasnePole;
                List<Point> docasnyList = bodyNaSmazani[0];
                bodyNaSmazani[0] = bodyNaSmazani[1];
                bodyNaSmazani[1] = docasnyList;
                vymenovani = false;
                
            }
        }

        //Smaze svetlo co se dotyka zadaneho bodu
        public void SmazSvetloOdBodu(Point odkud)
        {
            if (odkud.X >= 0 && odkud.Y >= 0 && odkud.X < KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK && odkud.X < KomponentaMapa.VELIKOST_MAPY_Y * POCET_SVETLA_NA_BLOK)
            {
                List<Point> mozneSmery = new List<Point>()
                {
                    new Point(-1, 0),
                    new Point(0, -1),
                    new Point(1, 0),
                    new Point(0, 1)
                };

                Queue<Point> fronta = new Queue<Point>();
                fronta.Enqueue(odkud);
                dataSvetlaProVypocet[odkud.X + odkud.Y * KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK] = GLOBALNI_BARVA;

                while (fronta.Count > 0)
                {
                    Point aktualniBod = fronta.Dequeue();

                    for (int i = 0; i < mozneSmery.Count; i++)
                    {
                        Point novyBod = aktualniBod + mozneSmery[i];

                        if (novyBod.X > 0 && novyBod.Y > 0 && novyBod.X < KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK && novyBod.Y < KomponentaMapa.VELIKOST_MAPY_Y * POCET_SVETLA_NA_BLOK
                            && dataSvetlaProVypocet[novyBod.X + novyBod.Y * KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK] != GLOBALNI_BARVA)
                        {
                            dataSvetlaProVypocet[novyBod.X + novyBod.Y * KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK] = GLOBALNI_BARVA;
                            fronta.Enqueue(novyBod);
                        }
                    }
                }
            }
        }

        //Vytvori svetlo od bodu se zadanou silou a barvou a take dostane zda ma svetlo pro pristi vypocet smazat
        public void PridejSvetloOdBodu(Point odkud, int pocatecniSila, Color barva, bool docasneSvetlo)
        {
            if (odkud.X >= 0 && odkud.Y >= 0 && odkud.X < KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK && odkud.Y < KomponentaMapa.VELIKOST_MAPY_Y * POCET_SVETLA_NA_BLOK)
            {
                List<Point> mozneSmery = new List<Point>()
                {
                    new Point(-1, 0),
                    new Point(0, -1),
                    new Point(1, 0),
                    new Point(0, 1)
                };

                Queue<(Point, Vector2)> fronta = new Queue<(Point, Vector2)>();
                fronta.Enqueue((odkud, new Vector2()));

                if (docasneSvetlo)
                    bodyNaSmazani[0].Add(odkud);

                dataSvetlaProVypocet[odkud.X + odkud.Y * KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK] = barva;

                while (fronta.Count > 0)
                {
                    (Point, Vector2) prvek = fronta.Dequeue();

                    for (int i = 0; i < mozneSmery.Count; i++)
                    {
                        Point novyBod = prvek.Item1 + mozneSmery[i];
                        Vector2 novaVzdalenost = prvek.Item2;

                        if (novyBod.X > 0 && novyBod.Y > 0 && novyBod.X < KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK && novyBod.Y < KomponentaMapa.VELIKOST_MAPY_Y * POCET_SVETLA_NA_BLOK)
                        {
                            if (!KomponentaMapa.mapa[novyBod.X / POCET_SVETLA_NA_BLOK, novyBod.Y / POCET_SVETLA_NA_BLOK].statickaNeprusvitnost)
                                novaVzdalenost += new Vector2(Math.Abs(mozneSmery[i].X), Math.Abs(mozneSmery[i].Y)) * new Vector2(KomponentaMapa.mapa[novyBod.X / POCET_SVETLA_NA_BLOK, novyBod.Y / POCET_SVETLA_NA_BLOK].neprusvitnost);
                            else
                                novaVzdalenost += new Vector2(Math.Abs(mozneSmery[i].X), Math.Abs(mozneSmery[i].Y)) * new Vector2(KomponentaMapa.mapa[novyBod.X / POCET_SVETLA_NA_BLOK, novyBod.Y / POCET_SVETLA_NA_BLOK].neprusvitnost * Math.Max(pocatecniSila / 255f, 1));

                            int noveSvetlo = 255 - (int)(Math.Sqrt(novaVzdalenost.X * novaVzdalenost.X + novaVzdalenost.Y * novaVzdalenost.Y) * 8 / POCET_SVETLA_NA_BLOK / pocatecniSila * 255);
                            Color aktBarva = dataSvetlaProVypocet[novyBod.X + novyBod.Y * KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK];

                            if (aktBarva.R + aktBarva.G + aktBarva.B >= (int)(noveSvetlo * (barva.R / 255f)) + (int)(noveSvetlo * (barva.G / 255f)) + (int)(noveSvetlo * (barva.B / 255f)) || noveSvetlo < 0)
                                continue;

                            Color novaBarva = new Color((int)(noveSvetlo * (barva.R / 255f)), (int)(noveSvetlo * (barva.G / 255f)), (int)(noveSvetlo * (barva.B / 255f)));
                            dataSvetlaProVypocet[novyBod.X + novyBod.Y * KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK] = novaBarva;

                            fronta.Enqueue((novyBod, novaVzdalenost));
                        }
                    }
                }
            }
        }
    }
}
