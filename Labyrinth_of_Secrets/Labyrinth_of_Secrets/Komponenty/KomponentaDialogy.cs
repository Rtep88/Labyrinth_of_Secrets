using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Reflection.Metadata;
using System.Text;
using System.Xml;
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
        private const int RYCHLOST_TEXTU = 20;

        public bool dialogJeOtevren = false;
        private XmlNode dialog;
        private XmlNode aktualniDialog;
        private Postava otevrenaPostava;
        private float pocetNactenychPismen = 0;
        public int zvolenaVolba = 0;

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
            if (hra.komponentaMenu.pauza && hra.komponentaMultiplayer.typZarizeni != KomponentaMultiplayer.TypZarizeni.Klient)
                return;

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
                        dialog = postavy[i].dialogy[postavy[i].kolikatyDialog];
                        aktualniDialog = dialog.FirstChild;
                        otevrenaPostava = postavy[i];
                        pocetNactenychPismen = 0;
                        zvolenaVolba = 0;
                    }

                    break;
                }
            }

            if (dialogJeOtevren)
            {
                if (pocetNactenychPismen < aktualniDialog.Attributes["message"].Value.Length)
                {
                    pocetNactenychPismen += (float)gameTime.ElapsedGameTime.TotalSeconds * RYCHLOST_TEXTU;
                    if (hra.NoveZmacknutaKlavesa(Keys.Enter))
                        pocetNactenychPismen = aktualniDialog.Attributes["message"].Value.Length;
                }
                else
                {
                    if (hra.NoveZmacknutaKlavesa(Keys.Up) && zvolenaVolba > 0)
                    {
                        zvolenaVolba--;
                    }
                    if (hra.NoveZmacknutaKlavesa(Keys.Down) && zvolenaVolba < aktualniDialog.ChildNodes.Count - 1)
                    {
                        zvolenaVolba++;
                    }
                    if (hra.NoveZmacknutaKlavesa(Keys.Enter))
                    {
                        pocetNactenychPismen = 0;

                        if (aktualniDialog.ChildNodes.Count > 0 && aktualniDialog.ChildNodes[zvolenaVolba].FirstChild != null)
                            aktualniDialog = aktualniDialog.ChildNodes[zvolenaVolba].FirstChild;
                        else
                        {
                            while (true)
                            {
                                if (aktualniDialog.NextSibling != null)
                                {
                                    aktualniDialog = aktualniDialog.NextSibling;
                                    break;
                                }
                                else if (aktualniDialog.ParentNode != null && aktualniDialog.ParentNode.ParentNode != null && aktualniDialog.ParentNode.ParentNode.Name == "page")
                                    aktualniDialog = aktualniDialog.ParentNode.ParentNode;
                                else
                                {
                                    dialogJeOtevren = false;

                                    if (otevrenaPostava.kolikatyDialog + 1 < otevrenaPostava.dialogy.Count)
                                        otevrenaPostava.kolikatyDialog++;

                                    break;
                                }
                            }
                        }

                        zvolenaVolba = 0;
                    }
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
                string[] slovaDialogu = aktualniDialog.Attributes["message"].Value.Split(' ');
                int pocetVykreslenychPismen = 0;
                float velikostFontu = 0.16f * pomerRozliseni;
                while (j < slovaDialogu.Length)
                {
                    if (j + 1 >= slovaDialogu.Length)
                    {
                        string text = string.Join(' ', Hra.SubArray(slovaDialogu, i, j - i + 1));
                        if (pocetVykreslenychPismen + text.Length > pocetNactenychPismen)
                            text = text.Substring(0, (int)pocetNactenychPismen - pocetVykreslenychPismen);

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
                            if (pocetVykreslenychPismen + text.Length > pocetNactenychPismen)
                                text = text.Substring(0, (int)pocetNactenychPismen - pocetVykreslenychPismen);

                            hra.VykresliTextSOkrajem(Hra.pixeloidSans, poziceDialogu + new Vector2(ODSAZENI_TEXTU * pomerRozliseni,
                                ODSAZENI_TEXTU * pomerRozliseni + odsazeniVysky), text,
                                velikostFontu, Color.White, Color.Black, 0.07f, 8, true);
                            pocetVykreslenychPismen += text.Length;

                            i = j + 1;
                            j = i;
                            odsazeniVysky += Hra.pixeloidSans.MeasureString(text).Y * velikostFontu;

                            if (pocetVykreslenychPismen >= pocetNactenychPismen)
                                break;

                            continue;
                        }
                    }
                    j++;
                }

                float vyskaTextu = Hra.pixeloidSans.MeasureString("ABCDE").Y * velikostFontu;
                float odsazeniVoleb = vyskaTextu * aktualniDialog.ChildNodes.Count + ODSAZENI_TEXTU * pomerRozliseni;
                if (pocetNactenychPismen >= aktualniDialog.Attributes["message"].Value.Length)
                {
                    for (int k = 0; k < aktualniDialog.ChildNodes.Count; k++)
                    {
                        hra.VykresliTextSOkrajem(Hra.pixeloidSans, poziceDialogu + new Vector2(ODSAZENI_TEXTU * pomerRozliseni,
                            VELIKOST_DIALOGU_Y * pomerRozliseni - odsazeniVoleb + vyskaTextu * k), aktualniDialog.ChildNodes[k].Attributes["message"].Value,
                            velikostFontu, k == zvolenaVolba ? Color.Yellow : Color.White, Color.Black, 0.07f, 8, true);
                    }
                }

            }

            hra._spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
