namespace Labyrinth_of_Secrets
{
    public class Hrac
    {
        public string jmeno;
        public Vector2 pozice;
        public Vector2 poziceMysi;
        public Zbran.TypZbrane vybranaZbran;
        public List<Zbran> zbrane = new List<Zbran>();
        public float zivoty = Hra.MAX_ZIVOTY;
        public int penize = 0;
        public bool jePripojen;
        public bool pozicePrebrana;

        public Hrac(string jmeno)
        {
            this.jmeno = jmeno;
            jePripojen = false;
            pozicePrebrana = false;

            for (int i = 0; i < 4; i++)
                zbrane.Add(new Zbran((Zbran.TypZbrane)i));
        }
    }
}
