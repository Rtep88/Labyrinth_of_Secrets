using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Labyrinth_of_Secrets
{
    public class KomponentaHrac : DrawableGameComponent
    {
        //Zaklad
        private Hra hra;

        //Textury
        public static Texture2D texturaHrace;

        //Struktury

        //Konstanty
        public const int VELIKOST_HRACE_X = 6; //Velikost vykresleni hrace
        public const int VELIKOST_HRACE_Y = 12; //Velikost vykresleni hrace
        public const int RYCHLOST_HRACE = 50;
        public const int MAX_ZIVOTY = 10000;
        public const int RYCHLOST_REGENERACE = 50;

        //Promene
        public Vector2 poziceHrace = new Vector2();
        public float zivoty = MAX_ZIVOTY;
        public int penize = 100000;

        public KomponentaHrac(Hra hra) : base(hra)
        {
            this.hra = hra;
        }

        public override void Initialize()
        {
            poziceHrace = new Vector2((hra.komponentaMapa.start.X + 0.5f) * KomponentaMapa.VELIKOST_BLOKU - VELIKOST_HRACE_X / 2f,
                (hra.komponentaMapa.start.Y + 0.5f) * KomponentaMapa.VELIKOST_BLOKU - VELIKOST_HRACE_Y / 2f);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            texturaHrace = hra.Content.Load<Texture2D>("Images\\Player");
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (hra.NoveZmacknutaKlavesa(Keys.Tab))
                PrevedBytovePoleNaHrace(PrevedHraceNaBytovePole());

            //Regenerace
            zivoty += (float)gameTime.ElapsedGameTime.TotalSeconds * RYCHLOST_REGENERACE;

            if (hra.komponentaMenu.pauza && hra.komponentaMultiplayer.typZarizeni != KomponentaMultiplayer.TypZarizeni.Klient)
                return;

            int pocetUpdatu = RYCHLOST_HRACE / 20 + 1;

            for (int i = 0; i < pocetUpdatu; i++)
            {
                //Pohyb hrace
                if (!hra.komponentaKonzole.jeOtevrena && !hra.komponentaObchod.obchodJeOtevreny && !hra.komponentaDialogy.dialogJeOtevren)
                {
                    Vector2 pohnuti = Vector2.Zero;
                    if (Keyboard.GetState().IsKeyDown(Keys.W))
                        pohnuti -= new Vector2(0, 1);
                    if (Keyboard.GetState().IsKeyDown(Keys.D))
                        pohnuti += new Vector2(1, 0);
                    if (Keyboard.GetState().IsKeyDown(Keys.S))
                        pohnuti += new Vector2(0, 1);
                    if (Keyboard.GetState().IsKeyDown(Keys.A))
                        pohnuti -= new Vector2(1, 0);

                    if (pohnuti.X != 0 || pohnuti.Y != 0)
                    {
                        pohnuti = Vector2.Normalize(pohnuti) * RYCHLOST_HRACE * (float)gameTime.ElapsedGameTime.TotalSeconds / pocetUpdatu;
                        poziceHrace += pohnuti;
                    }
                }

                //Kolize
                poziceHrace = hra.komponentaMapa.VypocitejKolizeSBloky(poziceHrace, new Vector2(VELIKOST_HRACE_X, VELIKOST_HRACE_Y));
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            hra._spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: hra.komponentaKamera._kamera.GetViewMatrix());

            if (!hra.komponentaMinimapa.jeOtevrena)
            {
                VykresliHraceSJmenovkou(poziceHrace, hra.jmeno);

                Kamera _kamera = hra.komponentaKamera._kamera;
                Vector2 opravdovaPoziceKamery = new Vector2(-_kamera.GetViewMatrix().Translation.X / _kamera.zoom, -_kamera.GetViewMatrix().Translation.Y / _kamera.zoom);
                Vector2 opravdovaVelikostOkna = new Vector2(hra.velikostOkna.X / _kamera.zoom, hra.velikostOkna.Y / _kamera.zoom);
                Vector2 opravdovaPoziceMysi = opravdovaPoziceKamery + Mouse.GetState().Position.ToVector2() * opravdovaVelikostOkna / hra.velikostOkna.ToVector2();
                hra.komponentaZbrane.VykresliZbranUHrace(hra.komponentaZbrane.zbrane[hra.komponentaZbrane.aktualniZbran].typZbrane, hra.komponentaHrac.poziceHrace, opravdovaPoziceMysi);
            }
            hra._spriteBatch.End();

            base.Draw(gameTime);
        }

        public void VykresliJmenovku(Vector2 pozicePostavy, Vector2 velikostPostavy, string jmeno)
        {
            float zvetseniTextu = 0.008f;
            Vector2 velikostTextu = Hra.comicSans.MeasureString(jmeno) * zvetseniTextu;
            Vector2 poziceTextu = new Vector2(pozicePostavy.X - velikostTextu.X / 2f + velikostPostavy.X / 2f, pozicePostavy.Y - velikostTextu.Y);

            hra._spriteBatch.Draw(Hra.pixel, poziceTextu - velikostTextu * 0.05f - new Vector2(0, velikostTextu.Y / 4), null, new Color(32, 32, 32, 128), 0, Vector2.Zero, velikostTextu * 1.1f, SpriteEffects.None, 0);
            hra._spriteBatch.DrawString(Hra.comicSans, jmeno, poziceTextu - new Vector2(0, velikostTextu.Y / 4), Color.White, 0, Vector2.Zero, zvetseniTextu, SpriteEffects.None, 0);
        }

        public void VykresliHraceSJmenovkou(Vector2 poziceHrace, string jmenoHrace)
        {
            VykresliJmenovku(poziceHrace, new Vector2(VELIKOST_HRACE_X, VELIKOST_HRACE_Y), jmenoHrace);

            hra._spriteBatch.Draw(texturaHrace, poziceHrace, null, Color.White, 0, Vector2.Zero,
                new Vector2((float)VELIKOST_HRACE_X / texturaHrace.Width, (float)VELIKOST_HRACE_Y / texturaHrace.Height), SpriteEffects.None, 0);
        }

        public byte[] PrevedHraceNaBytovePole()
        {
            List<byte> hracVBytech = new List<byte>();

            //Zapsani penez
            hracVBytech.AddRange(BitConverter.GetBytes(penize));

            byte[] zivotyByty = Encoding.UTF8.GetBytes(hra.komponentaMultiplayer.PrevedFloatNaString(zivoty));
            byte[] xByty = Encoding.UTF8.GetBytes(hra.komponentaMultiplayer.PrevedFloatNaString(poziceHrace.X));
            byte[] yByty = Encoding.UTF8.GetBytes(hra.komponentaMultiplayer.PrevedFloatNaString(poziceHrace.Y));

            //Zapsani zivotu
            hracVBytech.AddRange(BitConverter.GetBytes(zivotyByty.Length));
            hracVBytech.AddRange(zivotyByty);

            //Zapsani pozice X
            hracVBytech.AddRange(BitConverter.GetBytes(xByty.Length));
            hracVBytech.AddRange(xByty);

            //Zapsani pozice Y
            hracVBytech.AddRange(BitConverter.GetBytes(yByty.Length));
            hracVBytech.AddRange(yByty);

            //Zapsani zbrani
            hracVBytech.AddRange(BitConverter.GetBytes(hra.komponentaZbrane.zbrane.Count));
            foreach (Zbran zbran in hra.komponentaZbrane.zbrane)
            {
                hracVBytech.AddRange(BitConverter.GetBytes((int)zbran.typZbrane));
                hracVBytech.Add((byte)zbran.levelZbrane);
            }

            return Encoding.UTF8.GetBytes(Convert.ToBase64String(hracVBytech.ToArray()));
        }

        public void PrevedBytovePoleNaHrace(byte[] bytovePole)
        {
            bytovePole = Convert.FromBase64String(Encoding.UTF8.GetString(bytovePole));

            //Nacteni penez
            int i = 0;
            penize = BitConverter.ToInt32(Hra.SubArray(bytovePole, i, 4));
            i += 4;

            //Nacteni zivotu
            int pocetBytuNaZivoty = BitConverter.ToInt32(Hra.SubArray(bytovePole, i, 4));
            i += 4;
            zivoty = hra.komponentaMultiplayer.PrevedStringNaFloat(Encoding.UTF8.GetString(Hra.SubArray(bytovePole, i, pocetBytuNaZivoty)));
            i += pocetBytuNaZivoty;

            //Nacteni pozice X
            int pocetBytuNaXPozici = BitConverter.ToInt32(Hra.SubArray(bytovePole, i, 4));
            i += 4;
            poziceHrace.X = hra.komponentaMultiplayer.PrevedStringNaFloat(Encoding.UTF8.GetString(Hra.SubArray(bytovePole, i, pocetBytuNaXPozici)));
            i += pocetBytuNaXPozici;

            //Nacteni pozice Y
            int pocetBytuNaYPozici = BitConverter.ToInt32(Hra.SubArray(bytovePole, i, 4));
            i += 4;
            poziceHrace.Y = hra.komponentaMultiplayer.PrevedStringNaFloat(Encoding.UTF8.GetString(Hra.SubArray(bytovePole, i, pocetBytuNaYPozici)));
            i += pocetBytuNaXPozici;

            //Nacteni zbrani
            hra.komponentaZbrane.zbrane.Clear();
            int pocetZbrani = BitConverter.ToInt32(Hra.SubArray(bytovePole, i, 4));
            i += 4;
            for (int j = 0; j < pocetZbrani; j++)
            {
                Zbran zbran = new Zbran((Zbran.TypZbrane)BitConverter.ToInt32(Hra.SubArray(bytovePole, i, 4)));
                i += 4;
                zbran.levelZbrane = bytovePole[i++];
                hra.komponentaZbrane.zbrane.Add(zbran);
            }
        }
    }
}
