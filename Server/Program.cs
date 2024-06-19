using System.Diagnostics;
using System.Reflection;
using System.Text;
using Labyrinth_of_Secrets;

namespace Server
{
    class Program
    {
        const int FPS = 60;
        static float opravdovaFPS = FPS;
        static int idParenta = -1;
        static Hra hra;

        static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            //Naparsovani argumentu
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-parentpid":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out idParenta))
                            i++;
                        else
                        {
                            Console.WriteLine("Error: Invalid argument for -parentpid. Please provide a valid number.");
                            return;
                        }
                        break;
                    case "-port":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out KomponentaMultiplayer.PORT))
                            i++;
                        else
                        {
                            Console.WriteLine("Error: Invalid argument for -port. Please provide a valid number.");
                            return;
                        }
                        break;
                    case "-maxpacketsize":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out KomponentaMultiplayer.MAX_VELIKOST_PACKETU))
                            i++;
                        else
                        {
                            Console.WriteLine("Error: Invalid argument for -maxpacketsize. Please provide a valid number.");
                            return;
                        }
                        break;
                    case "-timetodisconnect":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out KomponentaMultiplayer.VTERINY_DO_ODPOJENI))
                            i++;
                        else
                        {
                            Console.WriteLine("Error: Invalid argument for -timetodisconnect. Please provide a valid number.");
                            return;
                        }
                        break;
                    case "-maxmonstercount":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out KomponentaMonstra.MAX_POCET_MONSTER))
                            i++;
                        else
                        {
                            Console.WriteLine("Error: Invalid argument for -maxmonstercount. Please provide a valid number.");
                            return;
                        }
                        break;
                    case "-mapsizex":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out KomponentaMapa.VELIKOST_MAPY_X))
                            i++;
                        else
                        {
                            Console.WriteLine("Error: Invalid argument for -mapsizex. Please provide a valid number.");
                            return;
                        }
                        break;
                    case "-mapsizey":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out KomponentaMapa.VELIKOST_MAPY_Y))
                            i++;
                        else
                        {
                            Console.WriteLine("Error: Invalid argument for -mapsizex. Please provide a valid number.");
                            return;
                        }
                        break;
                    case "-maxshopcount":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out KomponentaMapa.MAX_POCET_OBCHODU))
                            i++;
                        else
                        {
                            Console.WriteLine("Error: Invalid argument for -maxshopcount. Please provide a valid number.");
                            return;
                        }
                        break;
                    case "-minbranchlength":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out KomponentaMapa.MIN_DELKA_ODBOCKY))
                            i++;
                        else
                        {
                            Console.WriteLine("Error: Invalid argument for -minbranchlength. Please provide a valid number.");
                            return;
                        }
                        break;
                    default:
                        Console.WriteLine($"Error: Unknown argument: {args[i]}");
                        return;
                }
            }

            if (idParenta == -1)
                Console.Clear();

            hra = new Hra();
            VyberSvet();

            hra.komponentaMultiplayer.SpustServer();

            Thread kontrolaPrikazu = new Thread(KontrolujPrikazy);
            kontrolaPrikazu.Start();
            Stopwatch stopky = new Stopwatch();
            float deltaTime = 1f / FPS;
            while (true)
            {
                stopky.Restart();
                hra.komponentaZbrane.Update(deltaTime);
                hra.komponentaMonstra.Update(deltaTime);
                hra.komponentaMultiplayer.Update(deltaTime);

                if (idParenta != -1 && !Process.GetProcesses().Any(x => x.Id == idParenta))
                    Environment.Exit(0);

                while (stopky.ElapsedMilliseconds / 1000f < 1f / FPS) { Thread.Sleep(0); }

                deltaTime = stopky.ElapsedMilliseconds / 1000f;
                opravdovaFPS = 1 / deltaTime;
            }
        }

        static void KontrolujPrikazy()
        {
            while (true)
            {
                string prikaz = Console.ReadLine();
                if (prikaz == "exit")
                {
                    hra.komponentaMapa.UlozMapuNaDisk(hra.komponentaMapa.jmenoSveta);
                    Environment.Exit(0);
                }
                else if (prikaz == "save")
                {
                    hra.komponentaMapa.UlozMapuNaDisk(hra.komponentaMapa.jmenoSveta);
                    Console.WriteLine("Uloženo");
                }
                else if (prikaz == "fps")
                    Console.WriteLine("FPS: " + opravdovaFPS);
                else
                    Console.WriteLine("Neznámý příkaz: " + prikaz);
            }
        }

        static void VyberSvet()
        {
            List<string> svety = new List<string>();
            string cestaKDokumentum = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string cestaKSavum = Path.Combine(new string[] { cestaKDokumentum, ".Labyrinth_of_Secrets", "Saves" });
            Console.Clear();

            while (true)
            {
                if (Directory.Exists(cestaKSavum))
                    svety = Directory.GetDirectories(cestaKSavum).ToList();

                svety = svety.Where(x => File.Exists(Path.Combine(x, "world.bin"))).ToList();

                for (int i = 0; i < svety.Count; i++)
                {
                    Console.WriteLine(i + 1 + " - " + Path.GetFileNameWithoutExtension(svety[i]));
                }
                Console.WriteLine(Path.GetFileNameWithoutExtension(svety.Count + 1 + " - Vytvořit nový svět"));

                Console.Write("Vyberte číslo světa: ");
                string cisloSveta = Console.ReadLine();

                Console.Clear();

                int cisloSvetaInt = 0;
                try
                {
                    cisloSvetaInt = int.Parse(cisloSveta);
                }
                catch
                {
                    Console.WriteLine("To není číslo!");
                    continue;
                }

                if (cisloSvetaInt < 1 || cisloSvetaInt > svety.Count + 1)
                {
                    Console.WriteLine("Toto číslo není v rozsahu!");
                    continue;
                }

                if (cisloSvetaInt == svety.Count + 1)
                {
                    hra.komponentaMapa.jmenoSveta = "placeholder";
                    hra.komponentaMapa.VygenerujMapu();
                }
                else
                {
                    hra.komponentaMapa.jmenoSveta = Path.GetFileNameWithoutExtension(svety[cisloSvetaInt - 1]);
                    hra.komponentaMapa.NactiMapuZeSouboru(Path.GetFileNameWithoutExtension(svety[cisloSvetaInt - 1]));
                }

                break;
            }
        }
    }
}
