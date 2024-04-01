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

        public TypZbrane typZbrane;
        public float rychlostZbrane;
        public float aktCas = 0;

        public Zbran(TypZbrane typZbrane)
        {
            this.typZbrane = typZbrane;
            switch (typZbrane)
            {
                case TypZbrane.Pistole:
                    rychlostZbrane = 0.2f;
                    break;
                case TypZbrane.Odstrelovaci:
                    rychlostZbrane = 1f;
                    break;
                case TypZbrane.Kulomet:
                    rychlostZbrane = 0.05f;
                    break;
                case TypZbrane.Brokovnice:
                    rychlostZbrane = 0.7f;
                    break;
            }
        }

        public void PouzijZbran(Vector2 stredHrace, Vector2 smer, List<Projektil> projektily)
        {
            switch (typZbrane)
            {
                case TypZbrane.Pistole:
                    projektily.Add(new Projektil(stredHrace, smer, Projektil.TypProjektilu.Normalni));
                    break;
                case TypZbrane.Odstrelovaci:
                    projektily.Add(new Projektil(stredHrace, smer, Projektil.TypProjektilu.Odstrelovaci));
                    break;
                case TypZbrane.Kulomet:
                    projektily.Add(new Projektil(stredHrace, smer, Projektil.TypProjektilu.Lehka));
                    break;
                case TypZbrane.Brokovnice:
                    float uhel = MathHelper.ToRadians(10);
                    projektily.Add(new Projektil(stredHrace, Vector2.Transform(smer, Matrix.CreateRotationZ(-uhel * 2)), Projektil.TypProjektilu.Normalni));
                    projektily.Add(new Projektil(stredHrace, Vector2.Transform(smer, Matrix.CreateRotationZ(-uhel)), Projektil.TypProjektilu.Normalni));
                    projektily.Add(new Projektil(stredHrace, smer, Projektil.TypProjektilu.Normalni));
                    projektily.Add(new Projektil(stredHrace, Vector2.Transform(smer, Matrix.CreateRotationZ(uhel)), Projektil.TypProjektilu.Normalni));
                    projektily.Add(new Projektil(stredHrace, Vector2.Transform(smer, Matrix.CreateRotationZ(uhel * 2)), Projektil.TypProjektilu.Normalni));
                    break;
            }
        }
    }
}
