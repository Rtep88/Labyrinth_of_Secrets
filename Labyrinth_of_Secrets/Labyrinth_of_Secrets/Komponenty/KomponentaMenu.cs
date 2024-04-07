using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Labyrinth_of_Secrets
{
    public class KomponentaMenu : DrawableGameComponent
    {
        //Zaklad
        private Hra hra;

        //Textury
        Texture2D texturaSlotuInventare;

        //Konstanty
        private const int UKAZATEL_ZDRAVI_X_VEL = 350;
        private const int UKAZATEL_ZDRAVI_Y_VEL = 25;
        private const int UKAZATEL_ZDRAVI_OKRAJ_VEL = 6;
        private const int ODSAZENI = 10;
        private const int INVENTAR_SLOT_VEL = 64;

        public KomponentaMenu(Hra hra) : base(hra)
        {
            this.hra = hra;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            texturaSlotuInventare = hra.Content.Load<Texture2D>("Images/Inventory_slot");

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }


        public override void Draw(GameTime gameTime)
        {
            hra._spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            for (int i = 0; i < hra.komponentaZbrane.zbrane.Count; i++)
            {
                Color barvaZbrane = i == hra.komponentaZbrane.aktualniZbran ? Color.White : new Color(190, 190, 190);
                Zbran zbran = hra.komponentaZbrane.zbrane[i];
                Rectangle obdelnikVykresleniSlotu = new Rectangle(new Point(ODSAZENI) + new Point(INVENTAR_SLOT_VEL * i, 0), new Point(INVENTAR_SLOT_VEL));
                hra._spriteBatch.Draw(texturaSlotuInventare, obdelnikVykresleniSlotu, barvaZbrane);

                Texture2D texturaZbrane = KomponentaZbrane.texturyZbrani[(int)zbran.typZbrane];
                int vetsiRozmer = Math.Max(texturaZbrane.Width, texturaZbrane.Height);
                float meritko = INVENTAR_SLOT_VEL / (float)vetsiRozmer / 3 * 2;

                hra._spriteBatch.Draw(texturaZbrane, new Vector2(ODSAZENI) + new Vector2(INVENTAR_SLOT_VEL * i, 0) + new Vector2(INVENTAR_SLOT_VEL) / 2f, null, barvaZbrane, 0,
                    new Vector2(texturaZbrane.Width, texturaZbrane.Height) / 2f, meritko, SpriteEffects.None, 0);

                hra._spriteBatch.Draw(Hra.pixel, new Vector2(ODSAZENI) + new Vector2(INVENTAR_SLOT_VEL * i + INVENTAR_SLOT_VEL / 2f, INVENTAR_SLOT_VEL / 6f * 5f), null, new Color((int)(100 * barvaZbrane.R / 255f), (int)(100 * barvaZbrane.R / 255f), (int)(100 * barvaZbrane.R / 255f)),
                    0, new Vector2(0.5f), new Vector2(INVENTAR_SLOT_VEL * 2f / 3, INVENTAR_SLOT_VEL / 10f), SpriteEffects.None, 0);

                hra._spriteBatch.Draw(Hra.pixel, new Vector2(ODSAZENI) + new Vector2(INVENTAR_SLOT_VEL * i + INVENTAR_SLOT_VEL / 2f - INVENTAR_SLOT_VEL * 2f / 3 * zbran.aktCas / zbran.rychlostZbrane / 2f,
                    INVENTAR_SLOT_VEL / 6f * 5f), null, new Color((int)(180 * barvaZbrane.R / 255f), (int)(180 * barvaZbrane.R / 255f), (int)(180 * barvaZbrane.R / 255f)), 0, new Vector2(0.5f), new Vector2(INVENTAR_SLOT_VEL * 2f / 3 * (zbran.rychlostZbrane - zbran.aktCas) / zbran.rychlostZbrane,
                    INVENTAR_SLOT_VEL / 10f), SpriteEffects.None, 0);
            }

            //Moje zivoty
            hra._spriteBatch.Draw(Hra.pixel, new Vector2(hra.velikostOkna.X - UKAZATEL_ZDRAVI_X_VEL - UKAZATEL_ZDRAVI_OKRAJ_VEL - ODSAZENI, ODSAZENI), null, new Color(40, 40, 40), 0, Vector2.Zero,
                new Vector2(UKAZATEL_ZDRAVI_X_VEL + UKAZATEL_ZDRAVI_OKRAJ_VEL, UKAZATEL_ZDRAVI_Y_VEL + UKAZATEL_ZDRAVI_OKRAJ_VEL), SpriteEffects.None, 0);
            hra._spriteBatch.Draw(Hra.pixel, new Vector2(hra.velikostOkna.X - UKAZATEL_ZDRAVI_X_VEL - UKAZATEL_ZDRAVI_OKRAJ_VEL / 2 - ODSAZENI, UKAZATEL_ZDRAVI_OKRAJ_VEL / 2 + ODSAZENI), null, Color.Black, 0, Vector2.Zero,
            new Vector2(UKAZATEL_ZDRAVI_X_VEL, UKAZATEL_ZDRAVI_Y_VEL), SpriteEffects.None, 0);
            hra._spriteBatch.Draw(Hra.pixel, new Vector2(hra.velikostOkna.X - UKAZATEL_ZDRAVI_X_VEL - UKAZATEL_ZDRAVI_OKRAJ_VEL / 2 - ODSAZENI, UKAZATEL_ZDRAVI_OKRAJ_VEL / 2 + ODSAZENI), null, Color.Red, 0, Vector2.Zero,
                new Vector2(UKAZATEL_ZDRAVI_X_VEL, UKAZATEL_ZDRAVI_Y_VEL) * new Vector2(hra.komponentaHrac.zivoty / (float)KomponentaHrac.MAX_ZIVOTY, 1), SpriteEffects.None, 0);

            hra._spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
