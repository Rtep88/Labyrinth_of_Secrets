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
    public class KomponentaSvetlo : DrawableGameComponent
    {
        //Zaklad
        private Hra hra;

        //Textury
        private Texture2D svetlo;

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
        private const int POCET_SVETLA_NA_BLOK = 16; //Udava jak moc dopodrobna se svetlo bude pocitat - Minimum je 1 a maximum je omezen pouze systemovymi prostredky ale doporucuji neprekracovat 32 :)
        public Color GLOBALNI_BARVA = new Color(0, 0, 0); //Nenastavujte hodnoty na 255 jinak se svetlo bude nespravne mazat
        private Color BARVA_SVETLA_CURSORU = new Color(255, 255, 255);
        private const int SILA_SVETLA_CURSORU = 300;

        //Promene
        private Color[] docasnaDataSvetla;
        private Color[][] dataSvetla = new Color[2][];
        private bool[][] jeAktualni = new bool[2][];
        private List<Point>[] bodyNaSmazani = new List<Point>[2];
        public List<ZdrojSvetla> svetelneZdroje = new List<ZdrojSvetla>();
        public bool fullBright = false;

        public Thread pocitaniSvetla;
        public bool pocitaniSvetlaBezi = false;

        public KomponentaSvetlo(Hra hra) : base(hra)
        {
            this.hra = hra;
        }

        public override void Initialize()
        {
            SpustPocitaniSvetla();

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

            if (!fullBright)
            {
                lock (dataSvetla)
                {
                    svetlo.SetData(dataSvetla[1]);
                    hra._spriteBatch.Begin(SpriteSortMode.Deferred, Multiply, transformMatrix: hra.komponentaKamera._kamera.GetViewMatrix());
                    hra._spriteBatch.Draw(svetlo, new Rectangle(0, 0, KomponentaMapa.VELIKOST_MAPY_X * KomponentaMapa.VELIKOST_BLOKU, KomponentaMapa.VELIKOST_MAPY_Y * KomponentaMapa.VELIKOST_BLOKU), Color.White);
                    hra._spriteBatch.End();
                }
            }

            base.Draw(gameTime);
        }

        public void ZastavPocitaniSvetla()
        {
            if (pocitaniSvetla == null || !pocitaniSvetlaBezi)
                return;

            pocitaniSvetlaBezi = false;
            pocitaniSvetla.Join();
        }

        public void SpustPocitaniSvetla()
        {
            if (pocitaniSvetlaBezi)
                return;

            pocitaniSvetla = new Thread(PocitejSvetlo);

            pocitaniSvetlaBezi = true;
            pocitaniSvetla.Start();
        }

        //Resetuje vsechny promenne pocitani svetla
        public void ResetujPromenne()
        {
            dataSvetla = new Color[2][];
            jeAktualni = new bool[2][];
            bodyNaSmazani = new List<Point>[2];
            svetelneZdroje = new List<ZdrojSvetla>();
            docasnaDataSvetla = new Color[KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK * KomponentaMapa.VELIKOST_MAPY_Y * POCET_SVETLA_NA_BLOK];
            for (int i = 0; i < 2; i++)
            {
                dataSvetla[i] = new Color[KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK * KomponentaMapa.VELIKOST_MAPY_Y * POCET_SVETLA_NA_BLOK];
                jeAktualni[i] = new bool[KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK * KomponentaMapa.VELIKOST_MAPY_Y * POCET_SVETLA_NA_BLOK];
                svetlo = new Texture2D(GraphicsDevice, KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK, KomponentaMapa.VELIKOST_MAPY_Y * POCET_SVETLA_NA_BLOK, false, SurfaceFormat.Color);
                bodyNaSmazani[i] = new List<Point>();
            }
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

        //Nekonecna smycka co pocita svetlo v novem vlakne
        public void PocitejSvetlo()
        {
            while (!hra.ukonceno && pocitaniSvetlaBezi)
            {
                Vector2 opravdovaPoziceKamery = new Vector2(-hra.komponentaKamera._kamera.GetViewMatrix().Translation.X / hra.komponentaKamera._kamera.zoom, -hra.komponentaKamera._kamera.GetViewMatrix().Translation.Y / hra.komponentaKamera._kamera.zoom);
                Vector2 opravdovaVelikostOkna = new Vector2(hra.velikostOkna.X / hra.komponentaKamera._kamera.zoom, hra.velikostOkna.Y / hra.komponentaKamera._kamera.zoom);
                Point poziceMysi = ((opravdovaPoziceKamery + Mouse.GetState().Position.ToVector2() * opravdovaVelikostOkna / hra.velikostOkna.ToVector2()) / KomponentaMapa.VELIKOST_BLOKU * new Vector2(POCET_SVETLA_NA_BLOK)).ToPoint();

                foreach (Point odkudSmazat in bodyNaSmazani[0])
                {
                    SmazNeboPresunSvetloOdBodu(odkudSmazat, dataSvetla[0], false, null);
                }
                bodyNaSmazani[0] = new List<Point>();

                PridejSvetloOdBodu(poziceMysi, SILA_SVETLA_CURSORU, BARVA_SVETLA_CURSORU, true);

                foreach (ZdrojSvetla svetlo in svetelneZdroje)
                {
                    PridejSvetloOdBodu(svetlo.odkud, svetlo.silaSvetla, svetlo.barvaSvetla, false);
                }

                lock (dataSvetla)
                {
                    Color[] docasnePole = dataSvetla[1];
                    dataSvetla[1] = dataSvetla[0];
                    dataSvetla[0] = docasnePole;
                    List<Point> docasnyList = bodyNaSmazani[0];
                    bodyNaSmazani[0] = bodyNaSmazani[1];
                    bodyNaSmazani[1] = docasnyList;
                    bool[] docasnePoleBoolu = jeAktualni[1];
                    jeAktualni[1] = jeAktualni[0];
                    jeAktualni[0] = docasnePoleBoolu;
                }
            }
        }

        //Smaze svetlo(neho ho presune do jineho pole) co se dotyka zadaneho bodu
        public void SmazNeboPresunSvetloOdBodu(Point odkud, Color[] zJakehoPole, bool presunoutJinam, Color[] kamPresunout)
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

                while (fronta.Count > 0)
                {
                    Point aktualniBod = fronta.Dequeue();

                    for (int i = 0; i < mozneSmery.Count; i++)
                    {
                        Point novyBod = aktualniBod + mozneSmery[i];

                        if (novyBod.X > 0 && novyBod.Y > 0 && novyBod.X < KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK && novyBod.Y < KomponentaMapa.VELIKOST_MAPY_Y * POCET_SVETLA_NA_BLOK)
                        {
                            Color kopirovanaBarva = zJakehoPole[novyBod.X + novyBod.Y * KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK];

                            if (kopirovanaBarva != GLOBALNI_BARVA)
                            {
                                if (presunoutJinam)
                                {
                                    Color aktBarva = kamPresunout[novyBod.X + novyBod.Y * KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK];
                                    kamPresunout[novyBod.X + novyBod.Y * KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK] = NormalizujBarvu(aktBarva.R + kopirovanaBarva.R, aktBarva.G + kopirovanaBarva.G, aktBarva.B + kopirovanaBarva.B);
                                }
                                else
                                    jeAktualni[0][novyBod.X + novyBod.Y * KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK] = false;

                                zJakehoPole[novyBod.X + novyBod.Y * KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK] = GLOBALNI_BARVA;
                                fronta.Enqueue(novyBod);
                            }
                        }
                    }
                }
            }
        }

        //Vytvori svetlo od bodu se zadanou silou, barvou a take dostane zda se ma svetlo pro pristi vypocet smazat
        public void PridejSvetloOdBodu(Point odkud, int pocatecniSila, Color barva, bool docasneSvetlo)
        {
            if (odkud.X >= 0 && odkud.Y >= 0 && odkud.X < KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK && odkud.Y < KomponentaMapa.VELIKOST_MAPY_Y * POCET_SVETLA_NA_BLOK)
            {
                if (!docasneSvetlo && jeAktualni[0][odkud.X + odkud.Y * KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK] == true)
                    return;

                jeAktualni[0][odkud.X + odkud.Y * KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK] = true;

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

                docasnaDataSvetla[odkud.X + odkud.Y * KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK] = barva;

                while (fronta.Count > 0)
                {
                    (Point, Vector2) prvek = fronta.Dequeue();

                    for (int i = 0; i < mozneSmery.Count; i++)
                    {
                        Point novyBod = prvek.Item1 + mozneSmery[i];
                        Vector2 novaVzdalenost = prvek.Item2;

                        if (novyBod.X > 0 && novyBod.Y > 0 && novyBod.X < KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK && novyBod.Y < KomponentaMapa.VELIKOST_MAPY_Y * POCET_SVETLA_NA_BLOK)
                        {
                            if (!hra.komponentaMapa.mapa[novyBod.X / POCET_SVETLA_NA_BLOK, novyBod.Y / POCET_SVETLA_NA_BLOK].statickaNeprusvitnost)
                                novaVzdalenost += new Vector2(Math.Abs(mozneSmery[i].X), Math.Abs(mozneSmery[i].Y)) * new Vector2(hra.komponentaMapa.mapa[novyBod.X / POCET_SVETLA_NA_BLOK, novyBod.Y / POCET_SVETLA_NA_BLOK].neprusvitnost);
                            else
                                novaVzdalenost += new Vector2(Math.Abs(mozneSmery[i].X), Math.Abs(mozneSmery[i].Y)) * new Vector2(hra.komponentaMapa.mapa[novyBod.X / POCET_SVETLA_NA_BLOK, novyBod.Y / POCET_SVETLA_NA_BLOK].neprusvitnost * Math.Max(pocatecniSila / 255f, 1));

                            int noveSvetlo = 255 - (int)(Math.Sqrt(novaVzdalenost.X * novaVzdalenost.X + novaVzdalenost.Y * novaVzdalenost.Y) * 8 / POCET_SVETLA_NA_BLOK / pocatecniSila * 255);
                            Color aktBarva = docasnaDataSvetla[novyBod.X + novyBod.Y * KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK];
                            if (aktBarva == GLOBALNI_BARVA)
                                aktBarva = Color.Black;

                            if (aktBarva.R + aktBarva.G + aktBarva.B >= (int)(noveSvetlo * (barva.R / 255f)) + (int)(noveSvetlo * (barva.G / 255f)) + (int)(noveSvetlo * (barva.B / 255f)) || noveSvetlo < 0)
                                continue;

                            Color novaBarva = new Color((int)(noveSvetlo * (barva.R / 255f)), (int)(noveSvetlo * (barva.G / 255f)), (int)(noveSvetlo * (barva.B / 255f)));
                            docasnaDataSvetla[novyBod.X + novyBod.Y * KomponentaMapa.VELIKOST_MAPY_X * POCET_SVETLA_NA_BLOK] = novaBarva;

                            fronta.Enqueue((novyBod, novaVzdalenost));
                        }
                    }
                }

                SmazNeboPresunSvetloOdBodu(odkud, docasnaDataSvetla, true, dataSvetla[0]);
            }
        }

        //Nastavi barvu tak aby nepresahla 255 ale zachovala si svoji barvu
        Color NormalizujBarvu(int R, int G, int B)
        {
            if (R > 255 || G > 255 || B > 255)
            {
                int max = Math.Max(Math.Max(R, G), B);
                float delitel = max / 255f;
                R = (int)(R / delitel);
                G = (int)(G / delitel);
                B = (int)(B / delitel);
            }
            return new Color(R, G, B);
        }
    }
}
