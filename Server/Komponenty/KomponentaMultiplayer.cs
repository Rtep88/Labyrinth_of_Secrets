using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Unicode;
using System.Threading;

namespace Labyrinth_of_Secrets
{
    public class KomponentaMultiplayer
    {
        //Zaklad
        private Hra hra;


        //Konstanty
        public static int PORT = 34200;
        public static int MAX_VELIKOST_PACKETU = 4096;
        public static int VTERINY_DO_ODPOJENI = 3;

        //Enumy
        public enum TypPacketu
        {
            PohybHrace,
            ZiskatVelikostMapy,
            VraceniVelikostiMapy,
            ZiskatCastMapy,
            VraceniCastiMapy,
            ZadostOPripojeni,
            PotvrzujiPripojeni,
            DobrePripojujiSe,
            OdpojilSeKlient,
            UpdateMonster,
            NovyProjektil,
            UpdateProjektilu,
            UpdateHrace,
            KoupitZbran
        }

        //Promenne
        public Dictionary<string, Hrac> hraci = new Dictionary<string, Hrac>();
        private byte[] mapaVBytech;
        private IPEndPoint odesilatel = new IPEndPoint(IPAddress.Any, PORT);
        private List<Klient> klienti = new List<Klient>(); //Pro server
        private UdpClient udpServer; //Pro server

        private float casUpdatu = 0.1f;

        public KomponentaMultiplayer(Hra hra)
        {
            this.hra = hra;
        }

        public void Update(float deltaTime)
        {
            //Kontroluje jestli klient odpovida
            for (int i = 0; i < klienti.Count; i++)
            {
                if ((int)((ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - klienti[i].casPosledniOdpovedi) > VTERINY_DO_ODPOJENI * 1000)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Klient {klienti[i].jmeno} přestal odpovídat proto ho odpojuji!");
                    Console.ForegroundColor = ConsoleColor.White;
                    hraci[klienti[i].jmeno].jePripojen = false;
                    hraci[klienti[i].jmeno].pozicePrebrana = false;

                    for (int j = 0; j < 10; j++)
                        PosliVsemKlientum(Encoding.UTF8.GetBytes($"{(short)TypPacketu.OdpojilSeKlient};{klienti[i].jmeno}"));

                    klienti.RemoveAt(i);
                    i--;
                }
            }

            if (hra.komponentaZbrane.zmenaProjektilu)
            {
                PosliUpdateProjektilu();
                hra.komponentaZbrane.zmenaProjektilu = false;
            }
            casUpdatu -= deltaTime;
            if (casUpdatu <= 0)
            {
                PosliUpdateMonster();
                PosliUpdateProjektilu();
                PosliUpdateHracu();
                casUpdatu = 0.1f;
            }
        }

        public void SpustServer()
        {
            udpServer = new UdpClient(new IPEndPoint(IPAddress.Any, PORT));
            udpServer.BeginReceive(new AsyncCallback(ReceiveCallback), null);
            Console.WriteLine("Server zapnut na 0.0.0.0:" + PORT);
            mapaVBytech = hra.komponentaMapa.PrevedMapuNaBytovePole();
        }

        public void PosliDataKonkretniAdrese(byte[] data, IPEndPoint adresa)
        {
            udpServer.Send(data, data.Length, adresa);
        }

        void PosliVsemKlientum(byte[] data)
        {
            foreach (Klient klient in klienti)
            {
                udpServer.Send(data, data.Length, klient.ipAdresa);
            }
        }

        public void ZpracujData(byte[] data)
        {
            string[] dataVStringu = Encoding.UTF8.GetString(data).Split(';');
            TypPacketu typPacketu = (TypPacketu)int.Parse(dataVStringu[0]);

            switch (typPacketu)
            {
                case TypPacketu.PohybHrace:
                    Vector2 poziceHrace = new Vector2(PrevedStringNaFloat(dataVStringu[2]), PrevedStringNaFloat(dataVStringu[3]));
                    Vector2 poziceMysiHrace = new Vector2(PrevedStringNaFloat(dataVStringu[2]), PrevedStringNaFloat(dataVStringu[3]));
                    if (!hraci.ContainsKey(dataVStringu[1]))
                    {
                        hraci.Add(dataVStringu[1], new Hrac(dataVStringu[1]) { pozice = poziceHrace, poziceMysi = poziceMysiHrace, vybranaZbran = (Zbran.TypZbrane)int.Parse(dataVStringu[6]) });
                    }
                    else
                    {
                        hraci[dataVStringu[1]].jePripojen = true;
                        if (!hraci[dataVStringu[1]].pozicePrebrana && Math.Abs(hraci[dataVStringu[1]].pozice.X - poziceHrace.X) < 1 && Math.Abs(hraci[dataVStringu[1]].pozice.Y - poziceHrace.Y) < 1)
                        {
                            hraci[dataVStringu[1]].pozicePrebrana = true;
                        }
                        if (hraci[dataVStringu[1]].pozicePrebrana)
                        {
                            hraci[dataVStringu[1]].pozice = poziceHrace;
                            hraci[dataVStringu[1]].poziceMysi = poziceMysiHrace;
                            hraci[dataVStringu[1]].vybranaZbran = (Zbran.TypZbrane)int.Parse(dataVStringu[6]);
                        }
                    }
                    PosliVsemKlientum(data);
                    break;
                case TypPacketu.ZiskatVelikostMapy:
                    PosliDataKonkretniAdrese(Encoding.UTF8.GetBytes($"{(short)TypPacketu.VraceniVelikostiMapy};{mapaVBytech.Length}"), odesilatel);
                    break;
                case TypPacketu.VraceniVelikostiMapy:
                    break;
                case TypPacketu.ZiskatCastMapy:
                    int chtenaCastMapy = int.Parse(dataVStringu[1]) * MAX_VELIKOST_PACKETU;
                    string dataMapy = "";
                    for (int i = chtenaCastMapy; i < chtenaCastMapy + MAX_VELIKOST_PACKETU && i < mapaVBytech.Length; i++)
                        dataMapy += (char)mapaVBytech[i];
                    PosliDataKonkretniAdrese(Encoding.UTF8.GetBytes($"{(short)TypPacketu.VraceniCastiMapy};{int.Parse(dataVStringu[1])};{dataMapy}"), odesilatel);
                    break;
                case TypPacketu.VraceniCastiMapy:
                    break;
                case TypPacketu.OdpojilSeKlient:
                    if (klienti.Count(x => PorovnejIPAdresy(odesilatel, x.ipAdresa) &&
                        x.jmeno == dataVStringu[1]) > 0)
                    {
                        hraci[dataVStringu[1]].jePripojen = false;
                        hraci[dataVStringu[1]].pozicePrebrana = false;
                        klienti.Remove(klienti.First(x => PorovnejIPAdresy(odesilatel, x.ipAdresa) && x.jmeno == dataVStringu[1]));
                        Console.WriteLine("Odpojil se klient " + dataVStringu[1]);

                        for (int j = 0; j < 10; j++)
                            PosliVsemKlientum(Encoding.UTF8.GetBytes($"{(short)TypPacketu.OdpojilSeKlient};{dataVStringu[1]}"));
                    }
                    break;
                case TypPacketu.NovyProjektil:
                    if (hraci.ContainsKey(dataVStringu[1]))
                    {
                        Zbran zbran = new Zbran((Zbran.TypZbrane)int.Parse(dataVStringu[4]));
                        Vector2 poziceMysi = new Vector2(PrevedStringNaFloat(dataVStringu[2]), PrevedStringNaFloat(dataVStringu[3]));
                        Vector2 poziceVystreleni;
                        Vector2 poziceZacatkuHlavne;
                        float rotaceVystreleni;
                        if (poziceMysi.X >= hraci[dataVStringu[1]].pozice.X + Hra.VELIKOST_HRACE_X / 2)
                        {
                            Vector2 origin = hraci[dataVStringu[1]].pozice + new Vector2(Hra.VELIKOST_HRACE_X, Hra.VELIKOST_HRACE_Y / 2);
                            poziceVystreleni = hraci[dataVStringu[1]].pozice + new Vector2(Hra.VELIKOST_HRACE_X, Hra.VELIKOST_HRACE_Y / 2) - zbran.origin * zbran.meritkoVykresleni + zbran.spawnProjektilu * zbran.meritkoVykresleni;
                            rotaceVystreleni = Hra.VypocitejRotaci(origin, poziceVystreleni - new Vector2(5, 0), poziceVystreleni, poziceMysi);
                            poziceZacatkuHlavne = Hra.RotaceBodu(poziceVystreleni - new Vector2(zbran.velikostZbrane.X * zbran.meritkoVykresleni, 0), origin, rotaceVystreleni);
                            poziceVystreleni = Hra.RotaceBodu(poziceVystreleni, origin, rotaceVystreleni);
                        }
                        else
                        {
                            Vector2 origin = hraci[dataVStringu[1]].pozice + new Vector2(0, Hra.VELIKOST_HRACE_Y / 2);
                            Vector2 invertovanyOriginZbrane = new Vector2(zbran.origin.X * 2, zbran.velikostZbrane.Y) - zbran.origin;
                            Vector2 invertovanySpawnProjektilu = new Vector2(zbran.spawnProjektilu.X * 2, zbran.velikostZbrane.Y) - zbran.spawnProjektilu;
                            poziceVystreleni = hraci[dataVStringu[1]].pozice + new Vector2(0, Hra.VELIKOST_HRACE_Y / 2) - invertovanyOriginZbrane * zbran.meritkoVykresleni + invertovanySpawnProjektilu * zbran.meritkoVykresleni;
                            rotaceVystreleni = Hra.VypocitejRotaci(origin, poziceVystreleni - new Vector2(5, 0), poziceVystreleni, poziceMysi);
                            poziceZacatkuHlavne = Hra.RotaceBodu(poziceVystreleni - new Vector2(zbran.velikostZbrane.X * zbran.meritkoVykresleni, 0), origin, rotaceVystreleni);
                            poziceVystreleni = Hra.RotaceBodu(poziceVystreleni, origin, rotaceVystreleni);
                        }

                        Point aktualniBlok = new Point((int)((hraci[dataVStringu[1]].pozice.X + Hra.VELIKOST_HRACE_X / 2f) / KomponentaMapa.VELIKOST_BLOKU),
                            (int)((hraci[dataVStringu[1]].pozice.Y + Hra.VELIKOST_HRACE_Y / 2f) / KomponentaMapa.VELIKOST_BLOKU));

                        Point odkudKontrolovat = new Point(Math.Max(0, aktualniBlok.X - 2), Math.Max(0, aktualniBlok.Y - 2));
                        Point kamKontrolovat = new Point(Math.Min(KomponentaMapa.VELIKOST_MAPY_X - 1, aktualniBlok.X + 2),
                            Math.Min(KomponentaMapa.VELIKOST_MAPY_Y - 1, aktualniBlok.Y + 2));

                        List<Point> poziceNaKontrolu = new List<Point>();
                        for (int x = odkudKontrolovat.X; x <= kamKontrolovat.X; x++)
                            for (int y = odkudKontrolovat.Y; y <= kamKontrolovat.Y; y++)
                                poziceNaKontrolu.Add(new Point(x, y));

                        bool kolize = false;
                        for (int j = 0; j < poziceNaKontrolu.Count && !kolize; j++)
                        {
                            int x = poziceNaKontrolu[j].X;
                            int y = poziceNaKontrolu[j].Y;

                            if (hra.komponentaMapa.mapa[x, y].typPole != Pole.TypPole.Zed)
                                continue;

                            Rectangle obdelnikBloku = new Rectangle(x * KomponentaMapa.VELIKOST_BLOKU, y * KomponentaMapa.VELIKOST_BLOKU,
                                KomponentaMapa.VELIKOST_BLOKU, KomponentaMapa.VELIKOST_BLOKU);

                            kolize = Hra.KolizeCarySObdelnikem(poziceZacatkuHlavne, poziceVystreleni, obdelnikBloku);
                        }

                        Vector2 rotaceVystreleniJakoVector = new Vector2((float)Math.Cos(rotaceVystreleni), (float)Math.Sin(rotaceVystreleni));

                        if (!kolize)
                            zbran.PouzijZbran(poziceVystreleni, rotaceVystreleniJakoVector, hra.komponentaZbrane.noveProjektily);
                    }
                    break;
                case TypPacketu.KoupitZbran:
                    string jmenoHrace = dataVStringu[1];
                    if (hraci.ContainsKey(jmenoHrace))
                    {
                        Zbran zbran = hraci[jmenoHrace].zbrane.Find(x => x.typZbrane == (Zbran.TypZbrane)int.Parse(dataVStringu[2]));
                        if (zbran != null)
                        {
                            if (zbran.levelZbrane < 10 && hraci[jmenoHrace].penize - zbran.cenaUpgradu >= 0)
                            {
                                hraci[jmenoHrace].penize -= zbran.cenaUpgradu;
                                zbran.levelZbrane++;
                                zbran.zraneniZbrane = zbran.zraneniZbrane * zbran.levelZbrane / (zbran.levelZbrane - 1);
                                zbran.cenaUpgradu = (int)(zbran.cenaUpgradu * 1.75f);
                            }
                        }
                    }
                    break;
            }
        }

        void PosliUpdateMonster()
        {
            byte[] monstraVBytech = hra.komponentaMonstra.PrevedMonstraNaByty();
            string monstraJakoString = Encoding.UTF8.GetString(monstraVBytech);
            PosliVsemKlientum(Encoding.UTF8.GetBytes($"{(short)TypPacketu.UpdateMonster};{monstraJakoString}"));
        }

        void PosliUpdateProjektilu()
        {
            byte[] projektilyVBytech = hra.komponentaZbrane.PrevedProjektilyNaByty();
            string projektilyJakoString = Encoding.UTF8.GetString(projektilyVBytech);
            PosliVsemKlientum(Encoding.UTF8.GetBytes($"{(short)TypPacketu.UpdateProjektilu};{projektilyJakoString}"));
        }

        void PosliUpdateHracu()
        {
            foreach (var hrac in hraci)
            {
                if (!hrac.Value.jePripojen)
                    continue;

                byte[] hracVBytech = hra.PrevedHraceNaBytovePole(hrac.Value, !hrac.Value.pozicePrebrana);
                string hracJakoString = Encoding.UTF8.GetString(hracVBytech);
                PosliVsemKlientum(Encoding.UTF8.GetBytes($"{(short)TypPacketu.UpdateHrace};{hrac.Value.jmeno};{hracJakoString}"));
            }
        }

        //Zpracovani dotazu clienta
        void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                byte[] data = udpServer.EndReceive(ar, ref odesilatel);

                if (data.Length > 0)
                {
                    try
                    {
                        if (klienti.Count(x => PorovnejIPAdresy(x.ipAdresa, odesilatel)) == 1)
                            klienti.Single(x => PorovnejIPAdresy(x.ipAdresa, odesilatel)).casPosledniOdpovedi = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                        string[] dataVStringu = Encoding.UTF8.GetString(data).Split(';');
                        //Pokud je toto prvni dotaz tak si ho pridam do listu
                        if ((int.Parse(dataVStringu[0]) == (int)TypPacketu.ZadostOPripojeni || int.Parse(dataVStringu[0]) == (int)TypPacketu.DobrePripojujiSe) &&
                            klienti.Count(x => PorovnejIPAdresy(x.ipAdresa, odesilatel) && x.jmeno == dataVStringu[1]) == 0)
                        {
                            if (int.Parse(dataVStringu[0]) == (int)TypPacketu.ZadostOPripojeni)
                            {
                                for (int i = 0; i < 10; i++)
                                {
                                    PosliDataKonkretniAdrese(Encoding.UTF8.GetBytes((short)TypPacketu.PotvrzujiPripojeni + ";" + dataVStringu[1]), odesilatel);
                                    Thread.Sleep(10);
                                }
                            }
                            if (int.Parse(dataVStringu[0]) == (int)TypPacketu.DobrePripojujiSe)
                            {
                                klienti.Add(new Klient(odesilatel, dataVStringu[1], (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
                                Console.WriteLine("Připojil se klient " + dataVStringu[1]);
                            }
                        }
                        else
                            ZpracujData(data);
                    }
                    catch
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Neočekávaná chyba při provadění dotazu od klienta!");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
                udpServer.BeginReceive(new AsyncCallback(ReceiveCallback), null);
            }
            catch { }
        }

        bool PorovnejIPAdresy(IPEndPoint ipAdresa1, IPEndPoint ipAdresa2)
        {
            return ipAdresa1.Address.Equals(ipAdresa2.Address) && ipAdresa1.Port == ipAdresa2.Port;
        }

        public string PrevedFloatNaString(float cislo)
        {
            int minus = cislo < 0 ? 1 : 0;
            cislo = Math.Abs(cislo);
            byte[] floatVBytech = new byte[] { (byte)(128 * minus + cislo % (128 * 255) / 255), (byte)(cislo % 255), (byte)(cislo % 1 * 255) };
            return Convert.ToBase64String(floatVBytech);
        }

        public float PrevedStringNaFloat(string cislo)
        {
            byte[] floatVBytech = Convert.FromBase64String(cislo);
            int znamenko = floatVBytech[0] >= 128 ? -1 : 1;
            return (floatVBytech[0] % 128 * 255 + floatVBytech[1] + floatVBytech[2] / 255f) * znamenko;
        }
    }
}