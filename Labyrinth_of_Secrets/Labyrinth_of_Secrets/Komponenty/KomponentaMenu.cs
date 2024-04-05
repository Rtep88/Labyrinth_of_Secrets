using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        //Konstanty
        private const int UKAZATEL_ZDRAVI_X_VEL = 350;
        private const int UKAZATEL_ZDRAVI_Y_VEL = 40;
        private const int UKAZATEL_ZDRAVI_OKRAJ_VEL = 6;
        private const int UKAZATEL_ZDRAVI_ODSAZENI = 10;

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
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }


        public override void Draw(GameTime gameTime)
        {
            hra._spriteBatch.Begin();

            //Moje zivoty
            hra._spriteBatch.Draw(Hra.pixel, new Vector2(hra.velikostOkna.X - UKAZATEL_ZDRAVI_X_VEL - UKAZATEL_ZDRAVI_OKRAJ_VEL - UKAZATEL_ZDRAVI_ODSAZENI, UKAZATEL_ZDRAVI_ODSAZENI), null, new Color(40, 40, 40), 0, Vector2.Zero,
                new Vector2(UKAZATEL_ZDRAVI_X_VEL + UKAZATEL_ZDRAVI_OKRAJ_VEL, UKAZATEL_ZDRAVI_Y_VEL + UKAZATEL_ZDRAVI_OKRAJ_VEL), SpriteEffects.None, 0);
            hra._spriteBatch.Draw(Hra.pixel, new Vector2(hra.velikostOkna.X - UKAZATEL_ZDRAVI_X_VEL - UKAZATEL_ZDRAVI_OKRAJ_VEL / 2 - UKAZATEL_ZDRAVI_ODSAZENI, UKAZATEL_ZDRAVI_OKRAJ_VEL / 2 + UKAZATEL_ZDRAVI_ODSAZENI), null, Color.Black, 0, Vector2.Zero,
            new Vector2(UKAZATEL_ZDRAVI_X_VEL, UKAZATEL_ZDRAVI_Y_VEL), SpriteEffects.None, 0);
            hra._spriteBatch.Draw(Hra.pixel, new Vector2(hra.velikostOkna.X - UKAZATEL_ZDRAVI_X_VEL - UKAZATEL_ZDRAVI_OKRAJ_VEL / 2 - UKAZATEL_ZDRAVI_ODSAZENI, UKAZATEL_ZDRAVI_OKRAJ_VEL / 2 + UKAZATEL_ZDRAVI_ODSAZENI), null, Color.Red, 0, Vector2.Zero,
                new Vector2(UKAZATEL_ZDRAVI_X_VEL, UKAZATEL_ZDRAVI_Y_VEL) * new Vector2(hra.komponentaHrac.zivoty / (float)KomponentaHrac.MAX_ZIVOTY, 1), SpriteEffects.None, 0);

            hra._spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
