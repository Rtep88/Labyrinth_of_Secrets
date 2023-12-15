using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Labyrinth_of_Secrets
{
    class KomponentaMapa : DrawableGameComponent
    {
        //Zaklad
        private Hra hra;

        //Textury a fonty
        private Texture2D brick; //Placeholder textura

        //Konstanty
        public const int VELIKOST_BLOKU = 8; //Velikost vykresleni bloku
        public const int VELIKOST_MAPY_X = 101; //Urcuje sirku mapy - Musi by delitelne 6 po pricteni 1
        public const int VELIKOST_MAPY_Y = 101; //Urcuje vysku mapy - Musi by delitelne 6 po pricteni 1
        public const int MAX_POCET_OBCHODU = 20; //Max pocet obchodu - Musi byt minimalne 2
        public const int MIN_DELKA_ODBOCKY = 5; //Urcuje kolik musi minimalne mit odbocka delku

        //Enumy
        public enum Smer
        {
            Nahore,
            Vpravo,
            Dole,
            Vlevo
        }

        //Promenne
        public Pole[,] mapa = new Pole[VELIKOST_MAPY_X, VELIKOST_MAPY_Y];
        public List<Point> obchody = new List<Point>();

        public KomponentaMapa(Hra hra) : base(hra)
        {
            this.hra = hra;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            brick = hra.Content.Load<Texture2D>("Images\\Brick");
            VygenerujMapu();

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (hra.NoveZmacknutaKlavesa(Keys.Enter))
            {
                VygenerujMapu();
            }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            hra._spriteBatch.Begin();
            VykresliMapu();
            hra._spriteBatch.End();
            base.Draw(gameTime);
        }

        //Vykreslí všechny bloky na mapě
        void VykresliMapu()
        {
            for (int x = 0; x < VELIKOST_MAPY_X; x++)
            {
                for (int y = 0; y < VELIKOST_MAPY_Y; y++)
                {
                    if (mapa[x, y].typPole == Pole.TypPole.Zed)
                        hra._spriteBatch.Draw(brick, new Rectangle(x * VELIKOST_BLOKU, y * VELIKOST_BLOKU, VELIKOST_BLOKU, VELIKOST_BLOKU), Color.White);
                    if (mapa[x, y].typPole == Pole.TypPole.Obchodnik)
                        hra._spriteBatch.Draw(brick, new Rectangle(x * VELIKOST_BLOKU, y * VELIKOST_BLOKU, VELIKOST_BLOKU, VELIKOST_BLOKU), new Color(180, 180, 180));
                }
            }
        }

        //Vygeneruje uplne celou mapu a umisti na ni vsechny struktury co na ni maji byt
        void VygenerujMapu()
        {
            if ((VELIKOST_MAPY_X + 1) % 6 != 0 || (VELIKOST_MAPY_Y + 1) % 6 != 0 || MAX_POCET_OBCHODU < 2)
                throw new Exception("Neplatně nastevené argumenty pro generování mapy!");

            obchody = new List<Point>();
            VyplnCelouMapuZdmi();
            PridejObchody(MAX_POCET_OBCHODU);
            VyplnMapuBludistem(NajdiVolneMisto());
            PropojObchodyZBludistem();
            OdstranPrilizKratkeOdbocky(NajdiVchodDoObchodu(), MIN_DELKA_ODBOCKY);
        }

        //Umisti na kazdy blok na mape zed
        void VyplnCelouMapuZdmi()
        {
            for (int y = 0; y < VELIKOST_MAPY_Y; y++)
            {
                for (int x = 0; x < VELIKOST_MAPY_X; x++)
                {
                    mapa[x, y] = new Pole(Pole.TypPole.Zed);
                }
            }
        }

        //Nahodne na mape rozmisti prazdna mista na obchody tak aby se neprekryvaly
        void PridejObchody(int pocet)
        {
            List<Point> mistaNaObchody = new List<Point>();

            for (int y = 0; y < (VELIKOST_MAPY_Y + 1) / 6; y++)
            {
                for (int x = 0; x < (VELIKOST_MAPY_X + 1) / 6; x++)
                {
                    mistaNaObchody.Add(new Point(x * 6, y * 6));
                }
            }

            while (pocet > 0 && mistaNaObchody.Count != 0)
            {
                int indexMistaNaObchod = hra.rnd.Next(0, mistaNaObchody.Count);
                Point pozice = mistaNaObchody[indexMistaNaObchod];

                for (int y = pozice.Y + 1; y < pozice.Y + 4; y++)
                {
                    for (int x = pozice.X + 1; x < pozice.X + 4; x++)
                    {
                        mapa[x, y].typPole = Pole.TypPole.Obchod;
                    }
                }
                obchody.Add(pozice);

                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        mistaNaObchody.Remove(new Point((pozice.X / 6 + x) * 6, (pozice.Y / 6 + y) * 6));
                    }
                }

                pocet--;
            }
        }

        //Najde na mape volne misto na kterem lze zacit generovat bludiste
        Point NajdiVolneMisto()
        {
            for (int y = 1; y < VELIKOST_MAPY_Y - 1; y++)
            {
                for (int x = 1; x < VELIKOST_MAPY_X - 1; x++)
                {
                    bool vsechnyJsouZdi = true;
                    for (int y2 = -1; y2 <= 1 && vsechnyJsouZdi; y2++)
                    {
                        for (int x2 = -1; x2 <= 1 && vsechnyJsouZdi; x2++)
                        {
                            if (mapa[x + x2, y + y2].typPole != Pole.TypPole.Zed)
                                vsechnyJsouZdi = false;
                        }
                    }
                    if (vsechnyJsouZdi)
                    {
                        return new Point(x, y);
                    }
                }
            }

            throw new Exception("Neplatně nastevené argumenty pro generování mapy!");
        }

        //Pouziva Randomized depth-first search algoritmus pro vygenerovani bludiste tak aby neprekrylo obchod
        void VyplnMapuBludistem(Point pocatecniPozice)
        {
            List<Point> mozneSmery = new List<Point>()
            {
                new Point(-1, 0),
                new Point(0, -1),
                new Point(1, 0),
                new Point(0, 1)
            };

            Stack<Point> zasobnik = new Stack<Point>();
            zasobnik.Push(pocatecniPozice);
            mapa[pocatecniPozice.X, pocatecniPozice.Y].typPole = Pole.TypPole.Prazdne;

            while (zasobnik.Count > 0)
            {
                Point aktualniPozice = zasobnik.Pop();

                List<Point> mozneSmeryPromichany = new List<Point>(mozneSmery);
                hra.PromichejList(mozneSmeryPromichany);

                foreach (Point smer in mozneSmeryPromichany)
                {
                    Point novaPozice = aktualniPozice + smer * new Point(2);
                    if (novaPozice.X <= 0 || novaPozice.Y <= 0 || novaPozice.X >= VELIKOST_MAPY_X - 1 ||
                        novaPozice.Y >= VELIKOST_MAPY_Y - 1 || mapa[novaPozice.X, novaPozice.Y].typPole != Pole.TypPole.Zed)
                        continue;

                    mapa[novaPozice.X, novaPozice.Y].typPole = Pole.TypPole.Prazdne;
                    mapa[novaPozice.X - smer.X, novaPozice.Y - smer.Y].typPole = Pole.TypPole.Prazdne;
                    zasobnik.Push(aktualniPozice);
                    zasobnik.Push(novaPozice);
                    break;
                }
            }
        }

        //Udela v obchodech pruchod do zbytku bludiste a oznaci pole obchodnika
        void PropojObchodyZBludistem()
        {
            //V tomto listu jsou ulozeny mozne steny na probourani
            List<(Point, Point, Smer)> stredoveSteny = new List<(Point, Point, Smer)>()
            {
                (new Point(2, 4), new Point(2, 5), Smer.Dole),
                (new Point(2, 0), new Point(2, -1), Smer.Nahore),
                (new Point(0, 2), new Point(-1, 2), Smer.Vlevo),
                (new Point(4, 2), new Point(5, 2), Smer.Vpravo),
            };

            //V tomto listu jsou ulozeny mozne steny na probourani pokud by nebylo mozne probouranim steny z predchoziho listu vytvorit pruchod
            List<(Point, Point, Smer)> krajoveStredy = new List<(Point, Point, Smer)>()
            {
                (new Point(1, 4), new Point(1, 5), Smer.Dole),
                (new Point(3, 4), new Point(3, 5), Smer.Dole),
                (new Point(1, 0), new Point(1, -1), Smer.Nahore),
                (new Point(3, 0), new Point(3, -1), Smer.Nahore),
                (new Point(0, 1), new Point(-1, 1), Smer.Vlevo),
                (new Point(0, 3), new Point(-1, 3), Smer.Vlevo),
                (new Point(4, 1), new Point(5, 1), Smer.Vpravo),
                (new Point(4, 3), new Point(5, 3), Smer.Vpravo)
            };


            //Prevadi smer vchodu na pozici obchodnika
            Dictionary<Smer, Point> smerNaPozici = new Dictionary<Smer, Point>()
            {
                { Smer.Dole, new Point(2, 1) },
                { Smer.Nahore, new Point(2, 3) },
                { Smer.Vlevo, new Point(3, 2) },
                { Smer.Vpravo, new Point(1, 2) }
            };

            foreach (Point poziceObchodu in obchody)
            {
                hra.PromichejList(stredoveSteny);
                hra.PromichejList(krajoveStredy);
                List<(Point, Point, Smer)> stenyNaProjiti = stredoveSteny.Union(krajoveStredy).ToList();

                bool obchodPropojen = false;

                //Pokus o probourani steny
                foreach (var stena in stenyNaProjiti)
                {
                    Point poziceBouraneSteny = stena.Item1 + poziceObchodu;
                    Point poziceCestyPredObchodem = stena.Item2 + poziceObchodu;
                    Point poziceObchodnika = smerNaPozici[stena.Item3] + poziceObchodu;

                    if (poziceCestyPredObchodem.X == 0)
                        poziceCestyPredObchodem.X = -1;
                    if (poziceCestyPredObchodem.X == 4)
                        poziceCestyPredObchodem.X = 5;
                    if (poziceCestyPredObchodem.Y == 0)
                        poziceCestyPredObchodem.Y = -1;
                    if (poziceCestyPredObchodem.Y == 4)
                        poziceCestyPredObchodem.Y = 5;

                    if (poziceCestyPredObchodem.X >= 0 && poziceCestyPredObchodem.Y >= 0 && poziceCestyPredObchodem.X < VELIKOST_MAPY_X &&
                        poziceCestyPredObchodem.Y < VELIKOST_MAPY_Y && mapa[poziceCestyPredObchodem.X, poziceCestyPredObchodem.Y].typPole == Pole.TypPole.Prazdne)
                    {
                        mapa[poziceBouraneSteny.X, poziceBouraneSteny.Y].typPole = Pole.TypPole.Prazdne;
                        mapa[poziceObchodnika.X, poziceObchodnika.Y].typPole = Pole.TypPole.Obchodnik;
                        obchodPropojen = true;
                        break;
                    }
                }

                if (obchodPropojen)
                    continue;

                //Pokud obchod nelze propojit je odstranen
                for (int y = poziceObchodu.Y + 1; y < poziceObchodu.Y + 4; y++)
                {
                    for (int x = poziceObchodu.X + 1; x < poziceObchodu.X + 4; x++)
                    {
                        mapa[x, y].typPole = Pole.TypPole.Zed;
                    }
                }
            }
        }

        //Najde na mape volne blok ktery je pruchod do obchodu
        Point NajdiVchodDoObchodu()
        {
            List<Point> mozneSmery = new List<Point>()
            {
                new Point(-1, 0),
                new Point(0, -1),
                new Point(1, 0),
                new Point(0, 1)
            };

            for (int y = 1; y < VELIKOST_MAPY_Y - 1; y++)
            {
                for (int x = 1; x < VELIKOST_MAPY_X - 1; x++)
                {
                    foreach (Point smer in mozneSmery)
                    {
                        Point novaPozice = new Point(x, y) + smer;

                        if (mapa[x, y].typPole == Pole.TypPole.Prazdne && mapa[novaPozice.X, novaPozice.Y].typPole == Pole.TypPole.Obchod)
                            return new Point(x, y);
                    }
                }
            }

            throw new Exception("Neplatně nastevené argumenty pro generování mapy!");
        }

        //Odstrani vsechny pripojeni kratsi nez pozadovana hodnota
        void OdstranPrilizKratkeOdbocky(Point pocatecniPozice, int minDelkaOdbocky)
        {
            List<Point> mozneSmery = new List<Point>()
            {
                new Point(-1, 0),
                new Point(0, -1),
                new Point(1, 0),
                new Point(0, 1)
            };

            //Urcuje na kterych polich jsem uz byl
            bool[,] projitostPoli = new bool[VELIKOST_MAPY_X, VELIKOST_MAPY_Y];

            Stack<(Point, int)> zasobnik = new Stack<(Point, int)>();
            zasobnik.Push((pocatecniPozice, 1));
            projitostPoli[pocatecniPozice.X, pocatecniPozice.Y] = true;

            while (zasobnik.Count > 0)
            {
                (Point, int) stav = zasobnik.Pop();
                Point aktualniPozice = stav.Item1;
                int aktualniDelkaCesty = stav.Item2;
                List<Point> mozneDalsiTahy = new List<Point>();

                bool cestaDoObchodu = false;

                foreach (Point smer in mozneSmery)
                {
                    Point novaPozice = aktualniPozice + smer;
                    if (novaPozice.X <= 0 || novaPozice.Y <= 0 || novaPozice.X >= VELIKOST_MAPY_X - 1 ||
                        novaPozice.Y >= VELIKOST_MAPY_Y - 1 || projitostPoli[novaPozice.X, novaPozice.Y])
                        continue;

                    if (mapa[novaPozice.X, novaPozice.Y].typPole == Pole.TypPole.Prazdne)
                        mozneDalsiTahy.Add(smer);
                    if (mapa[novaPozice.X, novaPozice.Y].typPole == Pole.TypPole.Obchod)
                        cestaDoObchodu = true;
                }

                if (!cestaDoObchodu && mozneDalsiTahy.Count == 0 && minDelkaOdbocky > aktualniDelkaCesty)
                {
                    Point aktualniPoziceKeSmazani = aktualniPozice;

                    do
                    {
                        mozneDalsiTahy = new List<Point>();
                        foreach (Point smer in mozneSmery)
                        {
                            Point novaPozice = aktualniPoziceKeSmazani + smer;
                            if (novaPozice.X <= 0 || novaPozice.Y <= 0 || novaPozice.X >= VELIKOST_MAPY_X - 1 ||
                                novaPozice.Y >= VELIKOST_MAPY_Y - 1)
                                continue;

                            if (mapa[novaPozice.X, novaPozice.Y].typPole == Pole.TypPole.Prazdne)
                                mozneDalsiTahy.Add(smer);
                        }
                        if (mozneDalsiTahy.Count == 1)
                        {
                            mapa[aktualniPoziceKeSmazani.X, aktualniPoziceKeSmazani.Y].typPole = Pole.TypPole.Zed;
                            aktualniPoziceKeSmazani += mozneDalsiTahy[0];
                        }
                    } while (mozneDalsiTahy.Count == 1);
                }
                else if (mozneDalsiTahy.Count == 1)
                {
                    Point moznyDalsiTah = mozneDalsiTahy[0];
                    zasobnik.Push((aktualniPozice + moznyDalsiTah, aktualniDelkaCesty + 1));
                    projitostPoli[aktualniPozice.X + moznyDalsiTah.X, aktualniPozice.Y + moznyDalsiTah.Y] = true;
                }
                else if (mozneDalsiTahy.Count > 1)
                {
                    bool prvni = false;
                    foreach (Point moznyDalsiTah in mozneDalsiTahy)
                    {
                        if (prvni)
                            zasobnik.Push((aktualniPozice + moznyDalsiTah, aktualniDelkaCesty + 1));
                        else
                            zasobnik.Push((aktualniPozice + moznyDalsiTah, 1));
                        projitostPoli[aktualniPozice.X + moznyDalsiTah.X, aktualniPozice.Y + moznyDalsiTah.Y] = true;
                        prvni = false;
                    }
                }
            }
        }
    }
}