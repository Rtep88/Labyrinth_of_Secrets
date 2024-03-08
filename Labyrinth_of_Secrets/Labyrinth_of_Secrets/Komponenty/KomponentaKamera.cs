using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labyrinth_of_Secrets
{
    public class KomponentaKamera : GameComponent
    {
        //Zaklad
        private Hra hra;
        public Kamera _kamera;

        //Konstanty
        public const int RYCHLOST_KAMERY = 5;
        public const float NORMALNI_ZOOM = 8;
        public const float ODDALENY_ZOOM = 0.5f;

        //Promenne
        public bool volnaKamera = false;

        public KomponentaKamera(Hra hra) : base(hra)
        {
            this.hra = hra;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            //Velikost vykresleni mapy
            Vector2 velikostMapy = new Vector2(KomponentaMapa.VELIKOST_MAPY_X * KomponentaMapa.VELIKOST_BLOKU, KomponentaMapa.VELIKOST_MAPY_Y * KomponentaMapa.VELIKOST_BLOKU);

            if (!hra.komponentaKonzole.jeOtevrena && volnaKamera) //Volna kamera
            {
                if (Keyboard.GetState().IsKeyDown(Keys.P))
                    _kamera.zoom *= 1.02f;
                else if (Keyboard.GetState().IsKeyDown(Keys.M) && velikostMapy.X * _kamera.zoom / 1.02 > hra.velikostOkna.X &&
                    velikostMapy.Y * _kamera.zoom / 1.02 > hra.velikostOkna.Y)
                    _kamera.zoom /= 1.02f;
                else if (Keyboard.GetState().IsKeyDown(Keys.M))
                    _kamera.zoom = Math.Max(hra.velikostOkna.X / velikostMapy.X, hra.velikostOkna.Y / velikostMapy.Y);
                if (Keyboard.GetState().IsKeyDown(Keys.Up))
                    _kamera.pozice -= new Vector2(0, RYCHLOST_KAMERY / _kamera.zoom);
                if (Keyboard.GetState().IsKeyDown(Keys.Right))
                    _kamera.pozice += new Vector2(RYCHLOST_KAMERY / _kamera.zoom, 0);
                if (Keyboard.GetState().IsKeyDown(Keys.Down))
                    _kamera.pozice += new Vector2(0, RYCHLOST_KAMERY / _kamera.zoom);
                if (Keyboard.GetState().IsKeyDown(Keys.Left))
                    _kamera.pozice -= new Vector2(RYCHLOST_KAMERY / _kamera.zoom, 0);
            }
            else if (!volnaKamera) //Sledovani hrace
            {
                float cilovyZoom = NORMALNI_ZOOM;
                if (Keyboard.GetState().IsKeyDown(Keys.L) && !hra.komponentaKonzole.jeOtevrena)
                    cilovyZoom = ODDALENY_ZOOM;
                Vector2 novaNula = new Vector2(_kamera.pozice.X + Kamera.GetViewMatrix(_kamera.pozice, _kamera.origin, cilovyZoom).Translation.X / cilovyZoom,
                    _kamera.pozice.Y + Kamera.GetViewMatrix(_kamera.pozice, _kamera.origin, cilovyZoom).Translation.Y / cilovyZoom);
                Vector2 novaOpravdovaVelikostOkna = new Vector2(hra.velikostOkna.X / cilovyZoom, hra.velikostOkna.Y / cilovyZoom);
                Vector2 cilovaPozice = new Vector2(novaNula.X + hra.komponentaHrac.poziceHrace.X - novaOpravdovaVelikostOkna.X / 2f + KomponentaHrac.VELIKOST_HRACE_X / 2f,
                    novaNula.Y + hra.komponentaHrac.poziceHrace.Y - novaOpravdovaVelikostOkna.Y / 2f + KomponentaHrac.VELIKOST_HRACE_Y / 2f);
                float novyZoom = MathHelper.Lerp(_kamera.zoom, cilovyZoom, 0.1f);
                Vector2 novePozice = new Vector2(MathHelper.Lerp(_kamera.pozice.X, cilovaPozice.X, 0.1f), MathHelper.Lerp(_kamera.pozice.Y, cilovaPozice.Y, 0.1f));
                _kamera.pozice = novePozice;
                _kamera.zoom = novyZoom;
            }

            //Nastaveni zoomu tak aby kamera nebyla mimo vykreslovaci oblast
            if (_kamera.zoom < Math.Max(hra.velikostOkna.X / velikostMapy.X, hra.velikostOkna.Y / velikostMapy.Y))
                _kamera.zoom = Math.Max(hra.velikostOkna.X / velikostMapy.X, hra.velikostOkna.Y / velikostMapy.Y);

            //Vypocet potrebnych hodnot pro praci s kamerou
            Vector2 nula = new Vector2(_kamera.pozice.X + _kamera.GetViewMatrix().Translation.X / _kamera.zoom, _kamera.pozice.Y + _kamera.GetViewMatrix().Translation.Y / _kamera.zoom);
            Vector2 opravdovaPoziceKamery = new Vector2(-_kamera.GetViewMatrix().Translation.X / _kamera.zoom, -_kamera.GetViewMatrix().Translation.Y / _kamera.zoom);
            Vector2 opravdovaVelikostOkna = new Vector2(hra.velikostOkna.X / _kamera.zoom, hra.velikostOkna.Y / _kamera.zoom);

            //Posunuti kamery tak aby nebyla mimo vykreslovaci oblast
            if (opravdovaPoziceKamery.X < 0)
                _kamera.pozice = new Vector2(nula.X, _kamera.pozice.Y);
            if (opravdovaPoziceKamery.Y < 0)
                _kamera.pozice = new Vector2(_kamera.pozice.X, nula.Y);
            if (opravdovaPoziceKamery.X + opravdovaVelikostOkna.X > velikostMapy.X)
                _kamera.pozice = new Vector2(nula.X + velikostMapy.X - opravdovaVelikostOkna.X, _kamera.pozice.Y);
            if (opravdovaPoziceKamery.Y + opravdovaVelikostOkna.Y > velikostMapy.Y)
                _kamera.pozice = new Vector2(_kamera.pozice.X, nula.Y + velikostMapy.X - opravdovaVelikostOkna.Y);

            base.Update(gameTime);
        }
    }
}
