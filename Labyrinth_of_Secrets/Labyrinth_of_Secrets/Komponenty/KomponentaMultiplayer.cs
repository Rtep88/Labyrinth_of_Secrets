using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Unicode;
using System.Threading;
using System.Diagnostics;

namespace Labyrinth_of_Secrets
{
    public class KomponentaMultiplayer : DrawableGameComponent
    {
        //Zaklad
        private Hra hra;

        //Konstanty
        const int PORT = 34200;
        const int MAX_VELIKOST_PACKETU = 4096;
        const int VTERINY_DO_ODPOJENI = 3;

        //Enumy
        public enum TypZarizeni
        {
            SinglePlayer,
            Klient
        }

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
        public TypZarizeni typZarizeni = TypZarizeni.SinglePlayer;
        public Dictionary<string, Hrac> hraci = new Dictionary<string, Hrac>();
        private UdpClient udpKlient;
        private byte[] mapaVBytech;
        private IPEndPoint odesilatel = new IPEndPoint(IPAddress.Any, PORT);
        private IPEndPoint adresaServeru = new IPEndPoint(IPAddress.Any, PORT);
        private int velikostMapyVBytech;
        private int pocetZiskanychCastiMapy;
        private ulong posledniCasOdpovediServer;
        public Process procesServeru;
        public bool serverSpusten = false;
        public KomponentaMultiplayer(Hra hra) : base(hra)
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
            //Kontroluje jestli server odpovida
            if (typZarizeni == TypZarizeni.Klient && (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - posledniCasOdpovediServer > VTERINY_DO_ODPOJENI * 1000)
            {
                hra.komponentaKonzole.radky.Insert(0, new Radek("Server přestal odpovídat proto se odpojuji!", Color.Red));
                OdpojSeOdServer();

                hra.komponentaHlavniMenu.PrepniStavMenu(KomponentaHlavniMenu.StavMenu.HlavniMenu);
            }

            //Poslani pozice
            if (typZarizeni == TypZarizeni.Klient)
            {
                Kamera _kamera = hra.komponentaKamera._kamera;
                Vector2 opravdovaPoziceKamery = new Vector2(-_kamera.GetViewMatrix().Translation.X / _kamera.zoom, -_kamera.GetViewMatrix().Translation.Y / _kamera.zoom);
                Vector2 opravdovaVelikostOkna = new Vector2(hra.velikostOkna.X / _kamera.zoom, hra.velikostOkna.Y / _kamera.zoom);
                Vector2 opravdovaPoziceMysi = opravdovaPoziceKamery + Mouse.GetState().Position.ToVector2() * opravdovaVelikostOkna / hra.velikostOkna.ToVector2();
                PosliData(Encoding.UTF8.GetBytes($"{(short)TypPacketu.PohybHrace};{hra.jmeno};{PrevedFloatNaString(hra.komponentaHrac.poziceHrace.X)};{PrevedFloatNaString(hra.komponentaHrac.poziceHrace.Y)};{PrevedFloatNaString(opravdovaPoziceMysi.X)};{PrevedFloatNaString(opravdovaPoziceMysi.Y)};{(short)hra.komponentaZbrane.aktualniZbran}"));
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            hra._spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: hra.komponentaKamera._kamera.GetViewMatrix());

            if (!hra.komponentaMinimapa.jeOtevrena)
            {
                foreach (var hrac in hraci)
                {
                    hra.komponentaHrac.VykresliHraceSJmenovkou(hrac.Value.pozice, hrac.Value.jmeno);
                    hra.komponentaZbrane.VykresliZbranUHrace(hrac.Value.vybranaZbran, hrac.Value.pozice, hrac.Value.poziceMysi);
                }
            }

            hra._spriteBatch.End();
            base.Draw(gameTime);
        }

        public void ZiskejDataMapy()
        {
            velikostMapyVBytech = -1;
            int kolikratVyzkouseno = 0;

            while (velikostMapyVBytech == -1)
            {
                if (kolikratVyzkouseno % 100 == 0)
                    PosliData(Encoding.UTF8.GetBytes($"{(short)TypPacketu.ZiskatVelikostMapy}"));
                Thread.Sleep(10);
                kolikratVyzkouseno++;
            }

            kolikratVyzkouseno = 0;
            int minulyPocetZiskanychCastiMapy = 0;
            pocetZiskanychCastiMapy = 0;
            mapaVBytech = new byte[velikostMapyVBytech];

            while (pocetZiskanychCastiMapy * MAX_VELIKOST_PACKETU < velikostMapyVBytech)
            {
                if (kolikratVyzkouseno % 100 == 0)
                    PosliData(Encoding.UTF8.GetBytes($"{(short)TypPacketu.ZiskatCastMapy};{pocetZiskanychCastiMapy}"));
                Thread.Sleep(10);
                if (minulyPocetZiskanychCastiMapy != pocetZiskanychCastiMapy)
                {
                    kolikratVyzkouseno = 0;
                    minulyPocetZiskanychCastiMapy = pocetZiskanychCastiMapy;
                }
            }

            hra.komponentaMapa.PrevedBytyNaMapu(mapaVBytech);

            hra.komponentaPostavy.NactiPostavy();
        }

        public void PosliData(byte[] data)
        {
            udpKlient.Send(data, data.Length, adresaServeru);
        }

        public void PosliDataKonkretniAdrese(byte[] data, IPEndPoint adresa)
        {
            udpKlient.Send(data, data.Length, adresa);
        }

        public void ZpracujData(byte[] data)
        {
            if (typZarizeni == TypZarizeni.SinglePlayer)
                return;

            string[] dataVStringu = Encoding.UTF8.GetString(data).Split(';');
            TypPacketu typPacketu = (TypPacketu)int.Parse(dataVStringu[0]);

            switch (typPacketu)
            {
                case TypPacketu.PohybHrace:
                    Vector2 poziceHrace = new Vector2(PrevedStringNaFloat(dataVStringu[2]), PrevedStringNaFloat(dataVStringu[3]));
                    Vector2 poziceMysiHrace = new Vector2(PrevedStringNaFloat(dataVStringu[4]), PrevedStringNaFloat(dataVStringu[5]));
                    if (dataVStringu[1] != hra.jmeno)
                    {
                        if (!hraci.ContainsKey(dataVStringu[1]))
                            hraci.Add(dataVStringu[1], new Hrac(dataVStringu[1]) { pozice = poziceHrace, poziceMysi = poziceMysiHrace, vybranaZbran = (Zbran.TypZbrane)int.Parse(dataVStringu[6]) });
                        else
                        {
                            hraci[dataVStringu[1]].pozice = poziceHrace;
                            hraci[dataVStringu[1]].poziceMysi = poziceMysiHrace;
                            hraci[dataVStringu[1]].vybranaZbran = (Zbran.TypZbrane)int.Parse(dataVStringu[6]);
                        }
                    }
                    break;
                case TypPacketu.ZiskatVelikostMapy:
                    break;
                case TypPacketu.VraceniVelikostiMapy:
                    velikostMapyVBytech = int.Parse(dataVStringu[1]);
                    break;
                case TypPacketu.ZiskatCastMapy:
                    break;
                case TypPacketu.VraceniCastiMapy:
                    if (pocetZiskanychCastiMapy != int.Parse(dataVStringu[1]))
                        break;
                    int vracenaCastMapy = int.Parse(dataVStringu[1]) * MAX_VELIKOST_PACKETU;
                    string vracenaDataMapy = dataVStringu[2];
                    for (int i = vracenaCastMapy; i < vracenaCastMapy + MAX_VELIKOST_PACKETU && i < mapaVBytech.Length; i++)
                        mapaVBytech[i] = (byte)vracenaDataMapy[i - vracenaCastMapy];
                    pocetZiskanychCastiMapy++;
                    break;
                case TypPacketu.OdpojilSeKlient:
                    if (hraci.ContainsKey(dataVStringu[1]))
                    {
                        hraci.Remove(dataVStringu[1]);
                        hra.komponentaKonzole.radky.Insert(0, new Radek("Odpojil se hráč " + dataVStringu[1], Color.White));
                    }
                    break;
                case TypPacketu.UpdateMonster:
                    hra.komponentaMonstra.PrevedBytyNaMonstra(Encoding.UTF8.GetBytes(dataVStringu[1]));
                    break;
                case TypPacketu.UpdateProjektilu:
                    hra.komponentaZbrane.PrevedBytyNaProjektily(Encoding.UTF8.GetBytes(dataVStringu[1]));
                    break;
                case TypPacketu.UpdateHrace:
                    if (hra.jmeno == dataVStringu[1])
                        hra.komponentaHrac.PrevedBytovePoleNaHrace(Encoding.UTF8.GetBytes(dataVStringu[2]));
                    break;
            }
        }

        public void PripojSeNaServer(IPAddress adresa, bool mohuHoditVyjimkuPriNeuspechu)
        {
            if (typZarizeni != TypZarizeni.SinglePlayer)
            {
                hra.komponentaKonzole.radky.Insert(0, new Radek("Už jsi připojený k serveru!", Color.Yellow));
                return;
            }

            hra.komponentaMonstra.ResetujPromenne();

            typZarizeni = TypZarizeni.Klient;
            adresaServeru = new IPEndPoint(adresa, PORT);
            udpKlient = new UdpClient(0);

            udpKlient.Client.ReceiveTimeout = 200;
            bool navazanoSpojeni = false;
            for (int i = 0; i < 10; i++)
            {
                byte[] data = Encoding.UTF8.GetBytes((short)TypPacketu.ZadostOPripojeni + ";" + hra.jmeno);
                PosliData(data);

                try
                {
                    data = udpKlient.Receive(ref odesilatel);
                    if (Encoding.UTF8.GetString(data) == (short)TypPacketu.PotvrzujiPripojeni + ";" + hra.jmeno && PorovnejIPAdresy(odesilatel, adresaServeru))
                    {
                        navazanoSpojeni = true;
                        data = Encoding.UTF8.GetBytes((short)TypPacketu.DobrePripojujiSe + ";" + hra.jmeno);
                        for (int j = 0; j < 10; j++)
                        {
                            PosliData(data);
                            Thread.Sleep(10);
                        }
                        break;
                    }
                }
                catch { }
            }
            udpKlient.Client.ReceiveTimeout = -1;

            if (navazanoSpojeni)
            {
                udpKlient.BeginReceive(new AsyncCallback(ReceiveCallbackClient), null);
                hra.komponentaKonzole.radky.Insert(0, new Radek("Úspěšně připojeno na server", Color.LimeGreen));

                ZiskejDataMapy();
            }
            else
            {
                if (mohuHoditVyjimkuPriNeuspechu)
                    throw new Exception("Can't connect to this server");
                hra.komponentaKonzole.radky.Insert(0, new Radek("Na tuto adresu se nelze připojit!", Color.Red));
                typZarizeni = TypZarizeni.SinglePlayer;
            }
        }

        public void OdpojSeOdServer()
        {
            if (hra.komponentaMultiplayer.procesServeru != null && !hra.komponentaMultiplayer.procesServeru.HasExited)
                hra.komponentaMultiplayer.procesServeru.StandardInput.WriteLine("exit");

            udpKlient.Close();
            hraci.Clear();
            typZarizeni = TypZarizeni.SinglePlayer;
        }

        //Zpracovani dotazu serveru
        void ReceiveCallbackClient(IAsyncResult ar)
        {
            if (typZarizeni == TypZarizeni.SinglePlayer)
                return;

            try
            {
                byte[] data = udpKlient.EndReceive(ar, ref odesilatel);

                if (PorovnejIPAdresy(odesilatel, adresaServeru))
                {
                    posledniCasOdpovediServer = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    ZpracujData(data);
                }

                udpKlient.BeginReceive(new AsyncCallback(ReceiveCallbackClient), null);
            }
            catch { };
        }

        public void SpustServer()
        {
            if (typZarizeni != TypZarizeni.SinglePlayer)
            {
                hra.komponentaKonzole.radky.Insert(0, new Radek("Server už běží!", Color.Yellow));
                return;
            }

            procesServeru = new Process();
            procesServeru.EnableRaisingEvents = true;
            procesServeru.OutputDataReceived += new DataReceivedEventHandler(PrijmutaDataOdServer);
            procesServeru.ErrorDataReceived += new DataReceivedEventHandler(PrijmutErrorOdServeru);
            procesServeru.Exited += new EventHandler(ServerBylUkoncen);

            if (OperatingSystem.IsLinux())
                procesServeru.StartInfo.FileName = "./Server/Server";
            else if (OperatingSystem.IsWindows())
                procesServeru.StartInfo.FileName = "./Server/Server.exe";
            else
                throw new PlatformNotSupportedException();
            procesServeru.StartInfo.Arguments = $"-parentpid {Process.GetCurrentProcess().Id} -port {PORT}";
            procesServeru.StartInfo.UseShellExecute = false;
            procesServeru.StartInfo.CreateNoWindow = true;
            procesServeru.StartInfo.RedirectStandardError = true;
            procesServeru.StartInfo.RedirectStandardOutput = true;
            procesServeru.StartInfo.RedirectStandardInput = true;
            procesServeru.StartInfo.StandardErrorEncoding = Encoding.UTF8;
            procesServeru.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            procesServeru.StartInfo.StandardInputEncoding = Encoding.UTF8;

            procesServeru.Start();

            procesServeru.BeginErrorReadLine();
            procesServeru.BeginOutputReadLine();

            for (int i = 0; i < 300 && procesServeru != null && !procesServeru.HasExited; i++)
            {
                if (serverSpusten)
                {
                    PripojSeNaServer(IPAddress.Parse("127.0.0.1"), false);
                    break;
                }

                Thread.Sleep(100);
            }
        }

        void ServerBylUkoncen(object sender, EventArgs e)
        {
            hra.komponentaKonzole.radky.Insert(0, new Radek("Server byl ukončen", Color.White));
            procesServeru = null;
        }

        void PrijmutErrorOdServeru(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                hra.komponentaKonzole.radky.Insert(0, new Radek("[SERVER] " + e.Data, Color.Red));
        }

        void PrijmutaDataOdServer(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                if (e.Data.StartsWith("Server zapnut"))
                    serverSpusten = true;
                hra.komponentaKonzole.radky.Insert(0, new Radek("[SERVER] " + e.Data));
            }
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

        public void PosliInfoONovemProjektilu(Vector2 poziceMysi, Zbran.TypZbrane typZbrane)
        {
            PosliData(Encoding.UTF8.GetBytes($"{(short)TypPacketu.NovyProjektil};{hra.jmeno};{PrevedFloatNaString(poziceMysi.X)};{PrevedFloatNaString(poziceMysi.Y)};{(int)typZbrane}"));
        }

        public void PosliInfoONakupu(Zbran.TypZbrane typZbrane)
        {
            PosliData(Encoding.UTF8.GetBytes($"{(short)TypPacketu.KoupitZbran};{hra.jmeno};{(int)typZbrane}"));
        }
    }
}