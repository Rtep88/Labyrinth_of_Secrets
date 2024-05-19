using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Reflection.Metadata;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Labyrinth_of_Secrets
{
    public class KomponentaDialogy : DrawableGameComponent
    {
        //Zaklad
        private Hra hra;

        //Konstanty
        private const int VELIKOST_DIALOGU_X = 1000;
        private const int VELIKOST_DIALOGU_Y = 400;
        private const int ODSAZENI = 100;
        private const int ODSAZENI_TEXTU = 10;

        //Promenne
        public bool dialogJeOtevren = false;
        private List<string> dialogy;
        private int aktualniDialog;
        private int otevrenaPostava;

        public KomponentaDialogy(Hra hra) : base(hra)
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
            List<Postava> postavy = hra.komponentaPostavy.postavy;
            for (int i = 0; i < postavy.Count; i++)
            {
                Vector2 vzdalenostOdHrace = new Vector2(Math.Abs(hra.komponentaHrac.poziceHrace.X + KomponentaHrac.VELIKOST_HRACE_X / 2f - (postavy[i].pozice.X + postavy[i].velikost.X / 2f))
                    - (KomponentaHrac.VELIKOST_HRACE_X + postavy[i].velikost.X) / 2f,
                    Math.Abs(hra.komponentaHrac.poziceHrace.Y + KomponentaHrac.VELIKOST_HRACE_Y / 2f - (postavy[i].pozice.Y + postavy[i].velikost.Y / 2f))
                    - (KomponentaHrac.VELIKOST_HRACE_Y + postavy[i].velikost.Y) / 2f);

                if (vzdalenostOdHrace.X < 3 && vzdalenostOdHrace.Y < 3 && !hra.komponentaMinimapa.jeOtevrena &&
                    !dialogJeOtevren && hra.NoveZmacknutaKlavesa(Keys.E))
                {
                    if (postavy[i].jeProdavac)
                        hra.komponentaObchod.PrepniOtevrenostObchodu();
                    else if (postavy[i].dialogy != null)
                    {
                        dialogJeOtevren = true;
                        dialogy = postavy[i].dialogy[postavy[i].kolikatyDialog];
                        aktualniDialog = 0;
                        otevrenaPostava = i;
                    }

                    break;
                }
            }

            if (dialogJeOtevren && hra.NoveZmacknutaKlavesa(Keys.Enter))
            {
                aktualniDialog++;

                if (aktualniDialog >= dialogy.Count)
                {
                    dialogJeOtevren = false;

                    if (postavy[otevrenaPostava].kolikatyDialog + 1 < postavy[otevrenaPostava].dialogy.Count)
                        postavy[otevrenaPostava].kolikatyDialog++;
                }
            }

            base.Update(gameTime);
        }


        public override void Draw(GameTime gameTime)
        {
            hra._spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            float pomerRozliseni = Math.Min(hra.velikostOkna.X / 1920f, hra.velikostOkna.Y / 1080f);
            List<Postava> postavy = hra.komponentaPostavy.postavy;
            for (int i = 0; i < postavy.Count; i++)
            {
                Vector2 vzdalenostOdHrace = new Vector2(Math.Abs(hra.komponentaHrac.poziceHrace.X + KomponentaHrac.VELIKOST_HRACE_X / 2f - (postavy[i].pozice.X + postavy[i].velikost.X / 2f))
                    - (KomponentaHrac.VELIKOST_HRACE_X + postavy[i].velikost.X) / 2f,
                    Math.Abs(hra.komponentaHrac.poziceHrace.Y + KomponentaHrac.VELIKOST_HRACE_Y / 2f - (postavy[i].pozice.Y + postavy[i].velikost.Y / 2f))
                    - (KomponentaHrac.VELIKOST_HRACE_Y + postavy[i].velikost.Y) / 2f);

                if (vzdalenostOdHrace.X < 3 && vzdalenostOdHrace.Y < 3 && !hra.komponentaObchod.obchodJeOtevreny &&
                    !hra.komponentaMinimapa.jeOtevrena && !dialogJeOtevren)//Navod jak otevrit dialog
                {
                    string textObchodu = "Zmáčkněte E pro interakci s postavou";
                    hra.VykresliTextSOkrajem(Hra.pixeloidSans, hra.velikostOkna.ToVector2() / 2 - Hra.pixeloidSans.MeasureString(textObchodu) * 0.1f / 2 * pomerRozliseni, textObchodu, 0.1f * pomerRozliseni, Color.White, Color.Black, 0.07f, 8, true);
                }
            }

            if (dialogJeOtevren)
            {
                Vector2 poziceDialogu = new Vector2(hra.velikostOkna.X / 2f - VELIKOST_DIALOGU_X / 2f * pomerRozliseni,
                    hra.velikostOkna.Y - (VELIKOST_DIALOGU_Y + ODSAZENI) * pomerRozliseni);

                hra._spriteBatch.Draw(Hra.pixel, poziceDialogu, null, new Color(40, 40, 40, 180), 0, Vector2.Zero,
                    new Vector2(VELIKOST_DIALOGU_X, VELIKOST_DIALOGU_Y) * pomerRozliseni, SpriteEffects.None, 0);

                int i = 0, j = 0;
                float odsazeniVysky = 0;
                string[] slovaDialogu = dialogy[aktualniDialog].Split(' ');
                while (j < slovaDialogu.Length)
                {
                    float velikostFontu = 0.08f;
                    if (j + 1 >= slovaDialogu.Length)
                    {
                        string text = string.Join(' ', Hra.SubArray(slovaDialogu, i, j - i + 1));

                        hra.VykresliTextSOkrajem(Hra.pixeloidSans, poziceDialogu + new Vector2(ODSAZENI_TEXTU * pomerRozliseni,
                            ODSAZENI_TEXTU * pomerRozliseni + odsazeniVysky), text,
                            velikostFontu, Color.White, Color.Black, 0.07f, 8, true);

                        break;
                    }
                    else
                    {
                        float delkaTextu = Hra.pixeloidSans.MeasureString(string.Join(' ', Hra.SubArray(slovaDialogu, i, j - i + 2))).X * velikostFontu;
                        if (delkaTextu >= (VELIKOST_DIALOGU_X - ODSAZENI_TEXTU * 2) * pomerRozliseni)
                        {
                            string text = string.Join(' ', Hra.SubArray(slovaDialogu, i, j - i + 1));

                            hra.VykresliTextSOkrajem(Hra.pixeloidSans, poziceDialogu + new Vector2(ODSAZENI_TEXTU * pomerRozliseni,
                                ODSAZENI_TEXTU * pomerRozliseni + odsazeniVysky), text,
                                velikostFontu, Color.White, Color.Black, 0.07f, 8, true);

                            i = j + 1;
                            j = i;
                            odsazeniVysky += Hra.pixeloidSans.MeasureString(text).Y * velikostFontu;
                            continue;
                        }
                    }
                    j++;
                }
            }

            hra._spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
