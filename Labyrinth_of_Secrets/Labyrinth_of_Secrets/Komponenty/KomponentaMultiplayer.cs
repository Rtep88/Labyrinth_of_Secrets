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
            OdpojilSeKlient
        }

        //Promenne
        public string jmeno = "";
        public TypZarizeni typZarizeni = TypZarizeni.SinglePlayer;
        private Dictionary<string, Point> hraci = new Dictionary<string, Point>();
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
            jmeno = "DefaultUser_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString().Substring(6);
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
            }

            //Poslani pozice
            if (typZarizeni == TypZarizeni.Klient)
            {
                PosliData(Encoding.UTF8.GetBytes($"{(short)TypPacketu.PohybHrace};{jmeno};{(int)hra.komponentaHrac.poziceHrace.X};{(int)hra.komponentaHrac.poziceHrace.Y}"));
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            hra._spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: hra.komponentaKamera._kamera.GetViewMatrix());

            foreach (var hrac in hraci)
            {
                hra.komponentaHrac.VykresliHraceSJmenovkou(hrac.Value.ToVector2(), hrac.Key);
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
            string[] dataVStringu = Encoding.UTF8.GetString(data).Split(';');
            TypPacketu typPacketu = (TypPacketu)int.Parse(dataVStringu[0]);

            switch (typPacketu)
            {
                case TypPacketu.PohybHrace:
                    Point poziceHrace = new Point(int.Parse(dataVStringu[2]), int.Parse(dataVStringu[3]));
                    if (dataVStringu[1] != jmeno)
                    {
                        if (!hraci.ContainsKey(dataVStringu[1]))
                            hraci.Add(dataVStringu[1], poziceHrace);
                        else
                            hraci[dataVStringu[1]] = poziceHrace;
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
                    if (typZarizeni == TypZarizeni.Klient && PorovnejIPAdresy(adresaServeru, odesilatel) && hraci.ContainsKey(dataVStringu[1]))
                    {
                        hraci.Remove(dataVStringu[1]);
                        hra.komponentaKonzole.radky.Insert(0, new Radek("Odpojil se hráč " + dataVStringu[1], Color.White));
                    }
                    break;
            }
        }

        public void PripojSeNaServer(IPAddress adresa)
        {
            if (typZarizeni != TypZarizeni.SinglePlayer)
                return;

            typZarizeni = TypZarizeni.Klient;
            adresaServeru = new IPEndPoint(adresa, PORT);
            udpKlient = new UdpClient(0);

            udpKlient.Client.ReceiveTimeout = 200;
            bool navazanoSpojeni = false;
            for (int i = 0; i < 10; i++)
            {
                byte[] data = Encoding.UTF8.GetBytes((short)TypPacketu.ZadostOPripojeni + ";" + jmeno);
                PosliData(data);

                try
                {
                    data = udpKlient.Receive(ref odesilatel);
                    if (Encoding.UTF8.GetString(data) == (short)TypPacketu.PotvrzujiPripojeni + ";" + jmeno && PorovnejIPAdresy(odesilatel, adresaServeru))
                    {
                        navazanoSpojeni = true;
                        data = Encoding.UTF8.GetBytes((short)TypPacketu.DobrePripojujiSe + ";" + jmeno);
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
                hra.komponentaKonzole.radky.Insert(0, new Radek("Na tuto adresu se nelze připojit!", Color.Red));
                typZarizeni = TypZarizeni.SinglePlayer;
            }
        }

        public void OdpojSeOdServer()
        {
            if (hra.komponentaMultiplayer.procesServeru != null && !hra.komponentaMultiplayer.procesServeru.HasExited)
                hra.komponentaMultiplayer.procesServeru.StandardInput.WriteLine("exit");

            udpKlient.Dispose();
            hraci.Clear();
            typZarizeni = TypZarizeni.SinglePlayer;
        }

        //Zpracovani dotazu serveru
        void ReceiveCallbackClient(IAsyncResult ar)
        {
            if (typZarizeni == TypZarizeni.SinglePlayer)
                return;

            byte[] data = udpKlient.EndReceive(ar, ref odesilatel);

            if (PorovnejIPAdresy(odesilatel, adresaServeru))
            {
                posledniCasOdpovediServer = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                ZpracujData(data);
            }

            udpKlient.BeginReceive(new AsyncCallback(ReceiveCallbackClient), null);
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
            procesServeru.StartInfo.RedirectStandardError = true;
            procesServeru.StartInfo.RedirectStandardOutput = true;
            procesServeru.StartInfo.RedirectStandardInput = true;

            procesServeru.Start();

            procesServeru.BeginErrorReadLine();
            procesServeru.BeginOutputReadLine();

            for (int i = 0; i < 300 && !procesServeru.HasExited; i++)
            {
                if (serverSpusten)
                {
                    PripojSeNaServer(IPAddress.Parse("127.0.0.1"));
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
                if (e.Data.StartsWith("Server spuštěn"))
                    serverSpusten = true;
                hra.komponentaKonzole.radky.Insert(0, new Radek("[SERVER] " + e.Data));
            }
        }

        bool PorovnejIPAdresy(IPEndPoint ipAdresa1, IPEndPoint ipAdresa2)
        {
            return ipAdresa1.Address.Equals(ipAdresa2.Address) && ipAdresa1.Port == ipAdresa2.Port;
        }
    }

}