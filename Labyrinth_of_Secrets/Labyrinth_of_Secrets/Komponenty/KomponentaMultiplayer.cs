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
            Server,
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
        private Dictionary<string, Rectangle> hraci = new Dictionary<string, Rectangle>();
        private byte[] mapaVBytech;
        private IPEndPoint odesilatel = new IPEndPoint(IPAddress.Any, PORT);

        private List<Klient> klienti = new List<Klient>(); //Pro server
        private UdpClient udpServer; //Pro server

        private UdpClient udpKlient; //Pro klienta
        private IPEndPoint adresaServeru = new IPEndPoint(IPAddress.Any, PORT); //Pro klienta
        private int velikostMapyVBytech; //Pro klienta
        private int pocetZiskanychCastiMapy; //Pro klienta
        private ulong posledniCasOdpovediServer; //Pro klienta

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

            //Kontroluje jestli klient odpovida
            if (typZarizeni == TypZarizeni.Server)
            {
                for (int i = 0; i < klienti.Count; i++)
                {
                    if ((ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - klienti[i].casPosledniOdpovedi > VTERINY_DO_ODPOJENI * 1000)
                    {
                        hra.komponentaKonzole.radky.Insert(0, new Radek($"Klient {klienti[i].jmeno} přestal odpovídat proto ho odpojuji!", Color.Red));
                        hraci.Remove(klienti[i].jmeno);

                        for (int j = 0; j < 10; j++)
                            PosliVsemKlientum(Encoding.UTF8.GetBytes($"{(short)TypPacketu.OdpojilSeKlient};{klienti[i].jmeno}"));

                        klienti.RemoveAt(i);
                        i--;
                    }
                }
            }

            //Poslani pozice
            if (typZarizeni != TypZarizeni.SinglePlayer)
            {
                Kamera _kamera = hra.komponentaKamera._kamera;
                Point opravdovaPoziceKamery = new Vector2(-_kamera.GetViewMatrix().Translation.X / _kamera.zoom, -_kamera.GetViewMatrix().Translation.Y / _kamera.zoom).ToPoint();
                Point opravdovaVelikostOkna = new Vector2(hra.velikostOkna.X / _kamera.zoom, hra.velikostOkna.Y / _kamera.zoom).ToPoint();
                PosliData(Encoding.UTF8.GetBytes($"{(short)TypPacketu.PohybHrace};{jmeno};{opravdovaPoziceKamery.X};{opravdovaPoziceKamery.Y};{opravdovaVelikostOkna.X};{opravdovaVelikostOkna.Y}"));
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            hra._spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: hra.komponentaKamera._kamera.GetViewMatrix());
            foreach (var hrac in hraci)
            {
                hra._spriteBatch.DrawString(hra.comicSans, hrac.Key, new Vector2(hrac.Value.X, hrac.Value.Y - hra.comicSans.MeasureString(jmeno).Y * 0.05f / hra.komponentaKamera._kamera.zoom), Color.Red,
                    0, Vector2.Zero, 0.05f / hra.komponentaKamera._kamera.zoom, SpriteEffects.None, 0);

                int tloustkaOkraje = Math.Max(1, (int)(2 / hra.komponentaKamera._kamera.zoom));
                hra._spriteBatch.Draw(hra.pixel, new Rectangle(hrac.Value.X, hrac.Value.Y, hrac.Value.Width, tloustkaOkraje), Color.Red);
                hra._spriteBatch.Draw(hra.pixel, new Rectangle(hrac.Value.X, hrac.Value.Y, tloustkaOkraje, hrac.Value.Height), Color.Red);
                hra._spriteBatch.Draw(hra.pixel, new Rectangle(hrac.Value.X, hrac.Value.Y + hrac.Value.Height, hrac.Value.Width + tloustkaOkraje, tloustkaOkraje), Color.Red);
                hra._spriteBatch.Draw(hra.pixel, new Rectangle(hrac.Value.X + hrac.Value.Width, hrac.Value.Y, tloustkaOkraje, hrac.Value.Height + tloustkaOkraje), Color.Red);
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
            if (typZarizeni == TypZarizeni.Klient)
                udpKlient.Send(data, data.Length, adresaServeru);
            else if (typZarizeni == TypZarizeni.Server)
                PosliVsemKlientum(data);
        }

        public void PosliDataKonkretniAdrese(byte[] data, IPEndPoint adresa)
        {
            if (typZarizeni == TypZarizeni.Klient)
                udpKlient.Send(data, data.Length, adresa);
            else if (typZarizeni == TypZarizeni.Server)
                udpServer.Send(data, data.Length, adresa);
        }

        public void ZpracujData(byte[] data)
        {
            string[] dataVStringu = Encoding.UTF8.GetString(data).Split(';');
            TypPacketu typPacketu = (TypPacketu)int.Parse(dataVStringu[0]);

            switch (typPacketu)
            {
                case TypPacketu.PohybHrace:
                    Rectangle poziceHrace = new Rectangle(int.Parse(dataVStringu[2]), int.Parse(dataVStringu[3]), int.Parse(dataVStringu[4]), int.Parse(dataVStringu[5]));
                    if (dataVStringu[1] != jmeno)
                    {
                        if (!hraci.ContainsKey(dataVStringu[1]))
                            hraci.Add(dataVStringu[1], poziceHrace);
                        else
                            hraci[dataVStringu[1]] = poziceHrace;
                    }
                    PosliVsemKlientum(data);
                    break;
                case TypPacketu.ZiskatVelikostMapy:
                    PosliDataKonkretniAdrese(Encoding.UTF8.GetBytes($"{(short)TypPacketu.VraceniVelikostiMapy};{mapaVBytech.Length}"), odesilatel);
                    break;
                case TypPacketu.VraceniVelikostiMapy:
                    velikostMapyVBytech = int.Parse(dataVStringu[1]);
                    break;
                case TypPacketu.ZiskatCastMapy:
                    int chtenaCastMapy = int.Parse(dataVStringu[1]) * MAX_VELIKOST_PACKETU;
                    string dataMapy = "";
                    for (int i = chtenaCastMapy; i < chtenaCastMapy + MAX_VELIKOST_PACKETU && i < mapaVBytech.Length; i++)
                        dataMapy += (char)mapaVBytech[i];
                    PosliDataKonkretniAdrese(Encoding.UTF8.GetBytes($"{(short)TypPacketu.VraceniCastiMapy};{int.Parse(dataVStringu[1])};{dataMapy}"), odesilatel);
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
                    else if (typZarizeni == TypZarizeni.Server && klienti.Count(x => PorovnejIPAdresy(odesilatel, x.ipAdresa) &&
                        x.jmeno == dataVStringu[1]) > 0)
                    {
                        hraci.Remove(dataVStringu[1]);
                        klienti.Remove(klienti.First(x => PorovnejIPAdresy(odesilatel, x.ipAdresa) && x.jmeno == dataVStringu[1]));
                        hra.komponentaKonzole.radky.Insert(0, new Radek("Odpojil se klient " + dataVStringu[1], Color.White));

                        for (int j = 0; j < 10; j++)
                            PosliVsemKlientum(Encoding.UTF8.GetBytes($"{(short)TypPacketu.OdpojilSeKlient};{dataVStringu[1]}"));
                    }
                    break;
            }
        }

        #region Klient
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
        #endregion Klient

        #region Server
        public void SpustServer()
        {
            if (typZarizeni != TypZarizeni.SinglePlayer)
                return;

            udpServer = new UdpClient(new IPEndPoint(IPAddress.Any, PORT));
            udpServer.BeginReceive(new AsyncCallback(ReceiveCallback), null);
            hra.komponentaKonzole.radky.Insert(0, new Radek("Server spuštěn na 0.0.0.0:" + PORT));
            typZarizeni = TypZarizeni.Server;

            mapaVBytech = hra.komponentaMapa.PrevedMapuNaBytovePole();
        }

        //Zpracovani dotazu clienta
        void ReceiveCallback(IAsyncResult ar)
        {
            if (typZarizeni == TypZarizeni.SinglePlayer)
                return;

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
                            hra.komponentaKonzole.radky.Insert(0, new Radek("Připojil se klient " + dataVStringu[1], Color.White));
                        }
                    }
                    else
                        ZpracujData(data);
                }
                catch
                {
                    hra.komponentaKonzole.radky.Insert(0, new Radek("Neočekávaná chyba při provadění dotazu od klienta!", Color.Red));
                }
            }
            udpServer.BeginReceive(new AsyncCallback(ReceiveCallback), null);
        }

        void PosliVsemKlientum(byte[] data)
        {
            foreach (Klient klient in klienti)
            {
                udpServer.Send(data, data.Length, klient.ipAdresa);
            }
        }
        #endregion Server

        bool PorovnejIPAdresy(IPEndPoint ipAdresa1, IPEndPoint ipAdresa2)
        {
            return ipAdresa1.Address.Equals(ipAdresa2.Address) && ipAdresa1.Port == ipAdresa2.Port;
        }
    }
}