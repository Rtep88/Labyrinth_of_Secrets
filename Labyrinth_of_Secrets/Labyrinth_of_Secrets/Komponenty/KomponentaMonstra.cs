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
    public class KomponentaMonstra : DrawableGameComponent
    {
        //Zaklad
        private Hra hra;

        //Textury
        public static List<Texture2D> texturyMonster = new List<Texture2D>();

        //Struktury

        //Konstanty
        private const int MAX_POCET_MONSTER = 100;

        //Promenne
        public List<Monstrum> monstra = new List<Monstrum>();
        public List<Monstrum> novaMonstra = null;

        public KomponentaMonstra(Hra hra) : base(hra)
        {
            this.hra = hra;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            texturyMonster.Add(hra.Content.Load<Texture2D>("Images\\Player"));
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (hra.komponentaMultiplayer.typZarizeni == KomponentaMultiplayer.TypZarizeni.SinglePlayer)
            {
                NaspawnujMonstra();
                PohniMonstra((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
            else if (hra.komponentaMultiplayer.typZarizeni == KomponentaMultiplayer.TypZarizeni.Klient)
            {
                PohniMonstra((float)gameTime.ElapsedGameTime.TotalSeconds);
                if (novaMonstra != null)
                {
                    monstra = novaMonstra;
                    novaMonstra = null;
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (Monstrum monstrum in monstra)
            {
                hra._spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: hra.komponentaKamera._kamera.GetViewMatrix());

                hra._spriteBatch.Draw(texturyMonster[monstrum.typMonstra], monstrum.pozice, null, Color.Red, 0, Vector2.Zero,
                    monstrum.velikost.ToVector2() / new Vector2(texturyMonster[monstrum.typMonstra].Width, texturyMonster[monstrum.typMonstra].Height), SpriteEffects.None, 0);

                hra._spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        public void NaspawnujMonstra()
        {
            while (monstra.Count < MAX_POCET_MONSTER)
            {
                monstra.Add(new Monstrum(new Vector2(hra.rnd.Next(0, KomponentaMapa.VELIKOST_MAPY_X * KomponentaMapa.VELIKOST_BLOKU), hra.rnd.Next(0, KomponentaMapa.VELIKOST_MAPY_Y * KomponentaMapa.VELIKOST_BLOKU))));
            }
        }

        public void PohniMonstra(float deltaTime)
        {
            List<Monstrum> monstra2 = new List<Monstrum>();
            foreach (Monstrum monstrum in monstra)
            {
                Vector2 cil = hra.komponentaHrac.poziceHrace;

                if (hra.komponentaMultiplayer.typZarizeni == KomponentaMultiplayer.TypZarizeni.Klient)
                {
                    foreach (var hrac in hra.komponentaMultiplayer.hraci)
                    {
                        if (Vector2.Distance(cil, monstrum.pozice) > Vector2.Distance(hrac.Value, monstrum.pozice))
                        {
                            cil = hrac.Value;
                            if (monstrum.sledovanyHrac != hrac.Key)
                            {
                                monstrum.sledovanyHrac = hrac.Key;
                                monstrum.cesta = new LinkedList<Point>();
                                monstrum.cesta.AddFirst((monstrum.pozice + monstrum.velikost.ToVector2() / 2).ToPoint() / new Point(KomponentaMapa.VELIKOST_BLOKU));
                            }
                        }
                    }
                }
                Point startovniKostka = (monstrum.pozice + monstrum.velikost.ToVector2() / 2).ToPoint() / new Point(KomponentaMapa.VELIKOST_BLOKU);

                LinkedListNode<Point> aktualni = monstrum.cesta.First;
                while (aktualni != null && aktualni.Next != null && aktualni.Value != startovniKostka)
                    aktualni = aktualni.Next;
                while (aktualni != null && aktualni.Previous != null)
                    monstrum.cesta.Remove(aktualni.Previous);

                Point cilovaKostka = (cil + monstrum.velikost.ToVector2() / 2).ToPoint() / new Point(KomponentaMapa.VELIKOST_BLOKU);
                if (monstrum.cesta.Last() != cilovaKostka)
                {
                    LinkedListNode<Point> current = monstrum.cesta.Last;
                    bool obsahuje = false;
                    for (int i = 0; i < 10 && current.Previous != null; i++)
                    {
                        current = current.Previous;
                        if (current.Value == cilovaKostka)
                        {
                            obsahuje = true;
                            while (current.Next != null)
                                monstrum.cesta.Remove(current.Next);
                            break;
                        }
                    }
                    if (!obsahuje)
                    {
                        List<Point> cesta = hra.komponentaMapa.NajdiCestuMeziBody(monstrum.cesta.Last(), cilovaKostka);
                        if (cesta.Count == 0)
                            continue;
                        for (int i = 1; i < cesta.Count; i++)
                            monstrum.cesta.AddLast(cesta[i]);
                    }

                }

                if (monstrum.cesta.Count >= 2)
                    cil = (monstrum.cesta.First.Next.Value.ToVector2() + new Vector2(0.5f)) * new Vector2(KomponentaMapa.VELIKOST_BLOKU) - monstrum.velikost.ToVector2() / 2;

                Vector2 pohyb = Vector2.Normalize(cil - monstrum.pozice) * monstrum.rychlost * deltaTime;

                if (Math.Abs(monstrum.pozice.X - cil.X) > pohyb.X)
                    monstrum.pozice.X += pohyb.X;
                else
                    monstrum.pozice.X = cil.X;

                if (Math.Abs(monstrum.pozice.Y - cil.Y) > pohyb.Y)
                    monstrum.pozice.Y += pohyb.Y;
                else
                    monstrum.pozice.Y = cil.Y;
                monstra2.Add(monstrum);
            }

            monstra = monstra2;
        }

        public void PrevedBytyNaMonstra(byte[] prichoziBytyMonster)
        {
            byte[] bytyMonster = Convert.FromBase64String(Encoding.UTF8.GetString(prichoziBytyMonster));

            int pocetMonster = bytyMonster[0] * 255 + bytyMonster[1];
            List<Monstrum> monstraZBytu = new List<Monstrum>(pocetMonster);

            for (int i = 0; i < pocetMonster; i++)
            {
                Monstrum monstrum = new Monstrum();
                monstrum.velikost.X = bytyMonster[2 + i * 10];
                monstrum.velikost.Y = bytyMonster[2 + i * 10 + 1];
                monstrum.pozice.X = bytyMonster[2 + i * 10 + 2] * 255 + bytyMonster[2 + i * 10 + 3] + bytyMonster[2 + i * 10 + 4] / 255f;
                monstrum.pozice.Y = bytyMonster[2 + i * 10 + 5] * 255 + bytyMonster[2 + i * 10 + 6] + bytyMonster[2 + i * 10 + 7] / 255f;
                monstrum.rychlost = bytyMonster[2 + i * 10 + 8];
                monstrum.typMonstra = bytyMonster[2 + i * 10 + 9];
                monstrum.cesta = new LinkedList<Point>();
                monstrum.cesta.AddFirst((monstrum.pozice + monstrum.velikost.ToVector2() / 2).ToPoint() / new Point(KomponentaMapa.VELIKOST_BLOKU));
                monstraZBytu.Add(monstrum);
            }

            novaMonstra = monstraZBytu;
        }
    }
}
