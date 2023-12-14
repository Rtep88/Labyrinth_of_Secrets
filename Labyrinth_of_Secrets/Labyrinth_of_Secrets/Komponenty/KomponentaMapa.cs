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
        public const int VELIKOST_MAPY_X = 101; //Urcuje sirku mapy - musi by delitelne 6 po pricteni 1
        public const int VELIKOST_MAPY_Y = 101; //Urcuje vysku mapy - Musi by delitelne 6 po pricteni 1
        public const int MAX_POCET_MISTNOSTI = 20; //Max pocet mistnosti

        //Promenne
        public Pole[,] mapa = new Pole[VELIKOST_MAPY_X, VELIKOST_MAPY_Y];

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
            for (int x = 0; x < VELIKOST_MAPY_X; x++)
            {
                for (int y = 0; y < VELIKOST_MAPY_Y; y++)
                {
                    if (mapa[x, y].typPole == Pole.TypPole.Zed)
                        hra._spriteBatch.Draw(brick, new Rectangle(x * VELIKOST_BLOKU, y * VELIKOST_BLOKU, VELIKOST_BLOKU, VELIKOST_BLOKU), Color.White);
                }
            }
            hra._spriteBatch.End();
            base.Draw(gameTime);
        }

        //Vygeneruje uplne celou mapu a umisti na ni vsechny struktury co na ni maji byt
        void VygenerujMapu()
        {
            if ((VELIKOST_MAPY_X + 1) % 6 != 0 || (VELIKOST_MAPY_Y + 1) % 6 != 0)
                throw new Exception("Neplatné nastevené argumenty pro generování mapy!");

            VyplnCelouMapuZdmi();
            PridejMistnosti(MAX_POCET_MISTNOSTI);
            VyplnMapuBludistem(NajdiVolneMisto());
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
        void PridejMistnosti(int pocet)
        {
            List<Point> mistaNaMistnosti = new List<Point>();

            for (int y = 0; y < (VELIKOST_MAPY_Y + 1) / 6; y++)
            {
                for (int x = 0; x < (VELIKOST_MAPY_X + 1) / 6; x++)
                {
                    mistaNaMistnosti.Add(new Point(x * 6, y * 6));
                }
            }

            while (pocet > 0 && mistaNaMistnosti.Count != 0)
            {
                int indexMistnosti = hra.rnd.Next(0, mistaNaMistnosti.Count);
                Point pozice = mistaNaMistnosti[indexMistnosti];

                for (int y = pozice.Y + 1; y < pozice.Y + 4; y++)
                {
                    for (int x = pozice.X + 1; x < pozice.X + 4; x++)
                    {
                        mapa[x, y].typPole = Pole.TypPole.Obchod;
                    }
                }

                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        mistaNaMistnosti.Remove(new Point((pozice.X / 6 + x) * 6, (pozice.Y / 6 + y) * 6));
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

            return new Point(-1, -1);
        }

        //Pouziva Randomized depth-first search algoritmus pro vygenerovani bludiste tak aby neprekrylo mistnost
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
                    Point novaPozice = aktualniPozice + new Point(smer.X * 2, smer.Y * 2);
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
    }
}