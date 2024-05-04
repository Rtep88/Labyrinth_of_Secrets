using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Labyrinth_of_Secrets
{
    public class Zbran
    {
        public enum TypZbrane
        {
            Pistole,
            Odstrelovaci,
            Kulomet,
            Brokovnice
        }

        //Vzhled
        public Vector2 origin;
        public Vector2 spawnProjektilu;
        public Vector2 velikostZbrane;
        public float meritkoVykresleni = 0.4f;

        //Funkcnost
        public float rychlostZbrane;
        public TypZbrane typZbrane;
        public float aktCas = 0;
        public float zraneniZbrane = 0;
        public int levelZbrane = 1;
        public int cenaUpgradu = 20;

        public Zbran(TypZbrane typZbrane)
        {
            this.typZbrane = typZbrane;
            switch (typZbrane)
            {
                case TypZbrane.Pistole:
                    rychlostZbrane = 0.2f;
                    origin = new Vector2(4, 10);
                    spawnProjektilu = new Vector2(21, 3);
                    velikostZbrane = new Vector2(24, 16);
                    zraneniZbrane = 2;
                    break;
                case TypZbrane.Odstrelovaci:
                    rychlostZbrane = 1f;
                    origin = new Vector2(3, 9);
                    spawnProjektilu = new Vector2(31, 6);
                    velikostZbrane = new Vector2(32, 16);
                    zraneniZbrane = 12;
                    break;
                case TypZbrane.Kulomet:
                    rychlostZbrane = 0.05f;
                    origin = new Vector2(12, 10);
                    spawnProjektilu = new Vector2(38, 4);
                    velikostZbrane = new Vector2(40, 16);
                    zraneniZbrane = 0.8f;
                    break;
                case TypZbrane.Brokovnice:
                    rychlostZbrane = 0.7f;
                    origin = new Vector2(3, 9);
                    spawnProjektilu = new Vector2(31, 3);
                    velikostZbrane = new Vector2(32, 16);
                    zraneniZbrane = 2;
                    break;
            }
        }

        public void PouzijZbran(Vector2 stredHrace, Vector2 smer, List<Projektil> projektily)
        {
            switch (typZbrane)
            {
                case TypZbrane.Pistole:
                    projektily.Add(new Projektil(stredHrace, smer, Projektil.TypProjektilu.Normalni, (int)zraneniZbrane));
                    break;
                case TypZbrane.Odstrelovaci:
                    projektily.Add(new Projektil(stredHrace, smer, Projektil.TypProjektilu.Odstrelovaci, (int)zraneniZbrane));
                    break;
                case TypZbrane.Kulomet:
                    projektily.Add(new Projektil(stredHrace, smer, Projektil.TypProjektilu.Lehka, (int)zraneniZbrane));
                    break;
                case TypZbrane.Brokovnice:
                    float uhel = MathHelper.ToRadians(10);
                    projektily.Add(new Projektil(stredHrace, Vector2.Transform(smer, Matrix.CreateRotationZ(-uhel * 2)), Projektil.TypProjektilu.Normalni, (int)zraneniZbrane));
                    projektily.Add(new Projektil(stredHrace, Vector2.Transform(smer, Matrix.CreateRotationZ(-uhel)), Projektil.TypProjektilu.Normalni, (int)zraneniZbrane));
                    projektily.Add(new Projektil(stredHrace, smer, Projektil.TypProjektilu.Normalni, (int)zraneniZbrane));
                    projektily.Add(new Projektil(stredHrace, Vector2.Transform(smer, Matrix.CreateRotationZ(uhel)), Projektil.TypProjektilu.Normalni, (int)zraneniZbrane));
                    projektily.Add(new Projektil(stredHrace, Vector2.Transform(smer, Matrix.CreateRotationZ(uhel * 2)), Projektil.TypProjektilu.Normalni, (int)zraneniZbrane));
                    break;
            }
        }
    }
}
