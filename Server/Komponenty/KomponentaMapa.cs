using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Labyrinth_of_Secrets
{
    public class KomponentaMapa
    {
        //Zaklad
        private Hra hra;

        //Konstanty
        public const int VELIKOST_BLOKU = 32; //Velikost vykresleni bloku
        public static int VELIKOST_MAPY_X = 101; //Urcuje sirku mapy - Musi by delitelne 6 po pricteni 1 a melo by byt stejne jako X (minimum je 11)
        public static int VELIKOST_MAPY_Y = 101; //Urcuje vysku mapy - Musi by delitelne 6 po pricteni 1 a melo by byt stejne jako Y (minimum je 11)
        public static int MAX_POCET_OBCHODU = 20; //Max pocet obchodu - Musi byt minimalne 2
        public static int MIN_DELKA_ODBOCKY = 5; //Urcuje kolik musi minimalne mit odbocka delku

        //Enumy
        public enum Smer
        {
            Nahore,
            Vpravo,
            Dole,
            Vlevo
        }

        //Promenne
        public Pole[,] mapa;
        public List<Point> obchody = new List<Point>();
        public Point start = new Point(-1);
        public Point vychod = new Point(-1);
        public List<Point> cestaZeStartuDoCile = new List<Point>();
        public bool ukazCestu = false;
        private bool[,] projite; //Pro hledani cesty
        private List<Node> nody; //Pro hledani cesty
        private int[,] predpocitaneNody; //Pro hledani cesty

        public KomponentaMapa(Hra hra)
        {
            this.hra = hra;
        }

        //Resetuje vsechny promenne mapy
        void ResetujPromenne()
        {
            mapa = new Pole[VELIKOST_MAPY_X, VELIKOST_MAPY_Y];
            obchody = new List<Point>();
            start = new Point(-1);
            vychod = new Point(-1);
            cestaZeStartuDoCile = new List<Point>();
        }

        //Vygeneruje uplne celou mapu a umisti na ni vsechny struktury co na ni maji byt
        public void VygenerujMapu()
        {
            mapa = new Pole[VELIKOST_MAPY_X, VELIKOST_MAPY_Y];

            if ((VELIKOST_MAPY_X + 1) % 6 != 0 || (VELIKOST_MAPY_Y + 1) % 6 != 0 || MAX_POCET_OBCHODU < 2 ||
                VELIKOST_MAPY_X != VELIKOST_MAPY_Y || VELIKOST_MAPY_X < 11 || VELIKOST_MAPY_Y < 11)
                throw new Exception("Neplatně nastevené argumenty pro generování mapy!");

            ResetujPromenne();
            VyplnCelouMapuZdmi();
            PridejObchody(MAX_POCET_OBCHODU);
            VyplnMapuBludistem(NajdiMistoProVychod());
            PropojObchodyZBludistem();
            OdstranPrilizKratkeOdbocky(vychod, MIN_DELKA_ODBOCKY);
            start = NajdiMistoProStart();
            cestaZeStartuDoCile = NajdiCestuMeziBody(start, vychod);

            foreach (Point bod in cestaZeStartuDoCile)
                mapa[bod.X, bod.Y].naHlavniCeste = true;

            VytvorStrom();
            PredpocitejNody();
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
                        mapa[x, y] = new Pole(Pole.TypPole.Obchod);
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

        //Najde na mape volne misto na kterem hrac bude zacinat
        Point NajdiMistoProStart()
        {
            int ohnisko = 0;
            Point stred = new Point(VELIKOST_MAPY_X / 2, VELIKOST_MAPY_Y / 2);
            while (stred.X - ohnisko >= 0 && stred.Y - ohnisko >= 0 && stred.X + ohnisko < VELIKOST_MAPY_X && stred.Y + ohnisko < VELIKOST_MAPY_Y)
            {
                for (int x = -ohnisko; x <= ohnisko; x++)
                {
                    for (int y = -ohnisko; y <= ohnisko; y++)
                    {
                        if ((stred.X + x) % 2 == 1 && (stred.Y + y) % 2 == 1 && mapa[stred.X + x, stred.Y + y].typPole == Pole.TypPole.Prazdne)
                        {
                            mapa[stred.X + x, stred.Y + y] = new Pole(Pole.TypPole.Start);
                            return new Point(stred.X + x, stred.Y + y);
                        }
                    }
                }
                ohnisko++;
            }

            throw new Exception("Neplatně nastevené argumenty pro generování mapy!");
        }

        //Najde na mape volne misto od ktereho lze zacit generovat bludiste
        Point NajdiMistoProVychod()
        {
            List<Point> volneMista = new List<Point>();
            for (int y = 1; y < VELIKOST_MAPY_Y - 1; y += 2)
            {
                for (int x = 1; x < VELIKOST_MAPY_X - 1; x += 2)
                {
                    if (x != 1 && y != 1 && x != VELIKOST_MAPY_X - 2 && y != VELIKOST_MAPY_Y - 2)
                        continue;

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
                        volneMista.Add(new Point(x, y));
                    }
                }
            }
            if (volneMista.Count != 0)
                return volneMista[hra.rnd.Next(0, volneMista.Count)];
            else
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
            mapa[pocatecniPozice.X, pocatecniPozice.Y] = new Pole(Pole.TypPole.Prazdne);
            if (pocatecniPozice.X == 1)
            {
                mapa[pocatecniPozice.X - 1, pocatecniPozice.Y] = new Pole(Pole.TypPole.Vychod);
                vychod = new Point(pocatecniPozice.X - 1, pocatecniPozice.Y);
            }
            else if (pocatecniPozice.Y == 1)
            {
                mapa[pocatecniPozice.X, pocatecniPozice.Y - 1] = new Pole(Pole.TypPole.Vychod);
                vychod = new Point(pocatecniPozice.X, pocatecniPozice.Y - 1);
            }
            else if (pocatecniPozice.X == VELIKOST_MAPY_X - 2)
            {
                mapa[pocatecniPozice.X + 1, pocatecniPozice.Y] = new Pole(Pole.TypPole.Vychod);
                vychod = new Point(pocatecniPozice.X + 1, pocatecniPozice.Y);
            }
            else if (pocatecniPozice.Y == VELIKOST_MAPY_Y - 2)
            {
                mapa[pocatecniPozice.X, pocatecniPozice.Y + 1] = new Pole(Pole.TypPole.Vychod);
                vychod = new Point(pocatecniPozice.X, pocatecniPozice.Y + 1);
            }

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

                    mapa[novaPozice.X, novaPozice.Y] = new Pole(Pole.TypPole.Prazdne);
                    mapa[novaPozice.X - smer.X, novaPozice.Y - smer.Y] = new Pole(Pole.TypPole.Prazdne);
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
                        mapa[poziceBouraneSteny.X, poziceBouraneSteny.Y] = new Pole(Pole.TypPole.Prazdne);
                        mapa[poziceObchodnika.X, poziceObchodnika.Y] = new Pole(Pole.TypPole.Obchodnik);
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
                        mapa[x, y] = new Pole(Pole.TypPole.Zed);
                    }
                }
            }
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
                    if (novaPozice.X < 0 || novaPozice.Y < 0 || novaPozice.X > VELIKOST_MAPY_X - 1 ||
                        novaPozice.Y > VELIKOST_MAPY_Y - 1 || projitostPoli[novaPozice.X, novaPozice.Y])
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
                            mapa[aktualniPoziceKeSmazani.X, aktualniPoziceKeSmazani.Y] = new Pole(Pole.TypPole.Zed);
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

        public Node VytvorStrom()
        {
            projite = new bool[VELIKOST_MAPY_X, VELIKOST_MAPY_Y];
            nody = new List<Node>();
            projite[vychod.X, vychod.Y] = true;
            Node root = new Node(vychod, new List<ushort>());
            root.index = nody.Count;
            nody.Add(root);
            root.deti.Add(VytvorStromRekurzivne(vychod, root, 0));
            return root;
        }

        private Node VytvorStromRekurzivne(Point odkud, Node rodic, ushort cislo)
        {
            List<Point> mozneSmery = new List<Point>()
            {
                new Point(-1, 0),
                new Point(0, -1),
                new Point(1, 0),
                new Point(0, 1)
            };

            Point aktualniPozice = odkud;
            List<Point> moznePokracovani;
            ushort hloubka = 0;
            Node node = new Node(aktualniPozice, new List<ushort>(rodic.cesta) { cislo });
            node.index = nody.Count;
            nody.Add(node);
            mapa[odkud.X, odkud.Y].dalsi = node;
            mapa[odkud.X, odkud.Y].hloubka = hloubka++;

            do
            {
                moznePokracovani = new List<Point>();
                foreach (Point smer in mozneSmery)
                {
                    Point novaPozice = aktualniPozice + smer;
                    if (novaPozice.X >= 0 && novaPozice.X < VELIKOST_MAPY_X && novaPozice.Y >= 0 && novaPozice.Y < VELIKOST_MAPY_Y && (mapa[novaPozice.X, novaPozice.Y].typPole != Pole.TypPole.Zed) && projite[novaPozice.X, novaPozice.Y] == false)
                    {
                        projite[novaPozice.X, novaPozice.Y] = true;
                        moznePokracovani.Add(novaPozice);
                    }
                }
                if (moznePokracovani.Count == 1)
                {
                    mapa[aktualniPozice.X, aktualniPozice.Y].smeryOdVychodu = new List<Point>() { moznePokracovani[0] - aktualniPozice };
                    mapa[moznePokracovani[0].X, moznePokracovani[0].Y].smerKVychodu = aktualniPozice - moznePokracovani[0];
                    mapa[moznePokracovani[0].X, moznePokracovani[0].Y].dalsi = node;
                    mapa[moznePokracovani[0].X, moznePokracovani[0].Y].hloubka = hloubka++;
                    aktualniPozice = moznePokracovani[0];
                }
            } while (moznePokracovani.Count == 1);

            node.pozice = aktualniPozice;

            if (moznePokracovani.Count > 1)
            {
                ushort i = 0;
                mapa[aktualniPozice.X, aktualniPozice.Y].smeryOdVychodu = new List<Point>();
                foreach (Point pokracovani in moznePokracovani)
                {
                    mapa[aktualniPozice.X, aktualniPozice.Y].smeryOdVychodu.Add(pokracovani - aktualniPozice);
                    mapa[pokracovani.X, pokracovani.Y].smerKVychodu = aktualniPozice - pokracovani;
                    mapa[pokracovani.X, pokracovani.Y].dalsi = node;
                    node.deti.Add(VytvorStromRekurzivne(pokracovani, node, i++));
                }
                mapa[aktualniPozice.X, aktualniPozice.Y].krizovatka = true;
            }

            return node;
        }

        public void PredpocitejNody()
        {
            predpocitaneNody = new int[nody.Count, nody.Count];
            for (int i = 0; i < nody.Count; i++)
            {
                for (int j = 0; j < nody.Count; j++)
                {
                    int k;
                    for (k = 0; k < nody[i].cesta.Count && k < nody[j].cesta.Count; k++)
                    {
                        if (nody[i].cesta[k] != nody[j].cesta[k])
                            break;
                    }
                    predpocitaneNody[i, j] = k - 1;
                }
            }
        }

        public Point NajdiDalsiKrokNaCesteMeziBody(Point start, Point cil)
        {
            if (start.X < 0 || start.X >= VELIKOST_MAPY_X || start.Y < 0 || start.Y >= VELIKOST_MAPY_Y || cil.X < 0 || cil.X >= VELIKOST_MAPY_X || cil.Y < 0 ||
                cil.Y >= VELIKOST_MAPY_Y || mapa[start.X, start.Y].typPole == Pole.TypPole.Zed || mapa[cil.X, cil.Y].typPole == Pole.TypPole.Zed)
                return new Point(-1);

            Node rodicStartu = mapa[start.X, start.Y].dalsi;
            Node rodicCile = mapa[cil.X, cil.Y].dalsi;

            if (predpocitaneNody[rodicStartu.index, rodicCile.index] == rodicStartu.cesta.Count - 1)
            {
                if (rodicCile.cesta.Count == rodicStartu.cesta.Count)
                {
                    if (mapa[start.X, start.Y].hloubka > mapa[cil.X, cil.Y].hloubka)
                        return start + mapa[start.X, start.Y].smerKVychodu;
                    else if (mapa[start.X, start.Y].hloubka < mapa[cil.X, cil.Y].hloubka)
                        return start + mapa[start.X, start.Y].smeryOdVychodu[0];
                    else
                        return start;
                }
                else if (!mapa[start.X, start.Y].krizovatka)
                    return start + mapa[start.X, start.Y].smeryOdVychodu[0];
                else
                    return start + mapa[start.X, start.Y].smeryOdVychodu[rodicCile.cesta[predpocitaneNody[rodicStartu.index, rodicCile.index] + 1]];
            }
            else
                return start + mapa[start.X, start.Y].smerKVychodu;
        }

        //Najde cestu mezi dvema body - pocita s tim ze cesta vzdy existuje
        public List<Point> NajdiCestuMeziBody(Point start, Point cil)
        {
            List<Point> mozneSmery = new List<Point>()
            {
                new Point(-1, 0),
                new Point(0, -1),
                new Point(1, 0),
                new Point(0, 1)
            };

            Point[,] projite = new Point[VELIKOST_MAPY_X, VELIKOST_MAPY_Y];
            Queue<Point> naProjiti = new Queue<Point>();
            naProjiti.Enqueue(start);

            if (!(start.X >= 0 && start.X < VELIKOST_MAPY_X && start.Y >= 0 && start.Y < VELIKOST_MAPY_Y) || mapa[start.X, start.Y].typPole == Pole.TypPole.Zed)
                return new List<Point>();

            while (naProjiti.Count > 0)
            {
                Point aktualniBod = naProjiti.Dequeue();
                if (aktualniBod == cil)
                    break;
                foreach (Point smer in mozneSmery)
                {
                    Point novaPozice = aktualniBod + smer;
                    if (novaPozice.X >= 0 && novaPozice.X < VELIKOST_MAPY_X && novaPozice.Y >= 0 && novaPozice.Y < VELIKOST_MAPY_Y && mapa[novaPozice.X, novaPozice.Y].typPole != Pole.TypPole.Zed && projite[novaPozice.X, novaPozice.Y] == new Point())
                    {
                        projite[novaPozice.X, novaPozice.Y] = new Point(-smer.X, -smer.Y);
                        naProjiti.Enqueue(novaPozice);
                    }
                }
            }

            List<Point> cesta = new List<Point>() { cil };
            Point aktualni = cil;
            while (aktualni != start)
            {
                aktualni += projite[aktualni.X, aktualni.Y];
                cesta.Add(aktualni);
            }
            cesta.Reverse();
            return cesta;
        }

        public byte[] PrevedMapuNaBytovePole()
        {
            List<byte> mapaVBytech = new List<byte>
            {
                (byte)(VELIKOST_MAPY_X / 255),
                (byte)(VELIKOST_MAPY_X % 255),
                (byte)(VELIKOST_MAPY_Y / 255),
                (byte)(VELIKOST_MAPY_Y % 255)
            };

            for (int y = 0; y < VELIKOST_MAPY_Y; y++)
            {
                for (int x = 0; x < VELIKOST_MAPY_X; x++)
                {
                    mapaVBytech.Add((byte)mapa[x, y].typPole);
                }
            }

            List<KomponentaSvetlo.ZdrojSvetla> odkazNaZdrojeSvetla = hra.komponentaSvetlo.svetelneZdroje;

            mapaVBytech.Add((byte)(odkazNaZdrojeSvetla.Count / 255 / 255));
            mapaVBytech.Add((byte)(odkazNaZdrojeSvetla.Count % (255 * 255) / 255));
            mapaVBytech.Add((byte)(odkazNaZdrojeSvetla.Count % 255));

            for (int i = 0; i < odkazNaZdrojeSvetla.Count; i++)
            {
                mapaVBytech.AddRange(BitConverter.GetBytes(odkazNaZdrojeSvetla[i].silaSvetla));
                mapaVBytech.AddRange(BitConverter.GetBytes(odkazNaZdrojeSvetla[i].odkud.X));
                mapaVBytech.AddRange(BitConverter.GetBytes(odkazNaZdrojeSvetla[i].odkud.Y));
                mapaVBytech.Add(odkazNaZdrojeSvetla[i].barvaSvetla.R);
                mapaVBytech.Add(odkazNaZdrojeSvetla[i].barvaSvetla.G);
                mapaVBytech.Add(odkazNaZdrojeSvetla[i].barvaSvetla.B);
                mapaVBytech.Add(odkazNaZdrojeSvetla[i].barvaSvetla.A);
            }
            return Encoding.UTF8.GetBytes(Convert.ToBase64String(mapaVBytech.ToArray()));
        }
    }
}