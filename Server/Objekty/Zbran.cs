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
        public Vector2 origin;
        public Vector2 spawnProjektilu;
        public Vector2 velikostZbrane;
        public float meritkoVykresleni = 0.4f;

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
                    break;
                case TypZbrane.Odstrelovaci:
                    rychlostZbrane = 1f;
                    origin = new Vector2(3, 9);
                    spawnProjektilu = new Vector2(31, 6);
                    velikostZbrane = new Vector2(32, 16);
                    break;
                case TypZbrane.Kulomet:
                    rychlostZbrane = 0.05f;
                    origin = new Vector2(12, 10);
                    spawnProjektilu = new Vector2(38, 4);
                    velikostZbrane = new Vector2(40, 16);
                    break;
                case TypZbrane.Brokovnice:
                    rychlostZbrane = 0.7f;
                    origin = new Vector2(3, 9);
                    spawnProjektilu = new Vector2(31, 3);
                    velikostZbrane = new Vector2(32, 16);
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
                    float uhel = Hra.NaRadiany(10);
                    projektily.Add(new Projektil(stredHrace, Hra.RotaceBodu(smer, Vector2.Zero, -uhel * 2), Projektil.TypProjektilu.Normalni));
                    projektily.Add(new Projektil(stredHrace, Hra.RotaceBodu(smer, Vector2.Zero, -uhel), Projektil.TypProjektilu.Normalni));
                    projektily.Add(new Projektil(stredHrace, smer, Projektil.TypProjektilu.Normalni));
                    projektily.Add(new Projektil(stredHrace, Hra.RotaceBodu(smer, Vector2.Zero, uhel), Projektil.TypProjektilu.Normalni));
                    projektily.Add(new Projektil(stredHrace, Hra.RotaceBodu(smer, Vector2.Zero, uhel * 2), Projektil.TypProjektilu.Normalni));
                    break;
            }
        }
    }
}
