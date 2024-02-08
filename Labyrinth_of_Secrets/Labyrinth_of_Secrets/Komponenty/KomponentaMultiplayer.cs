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

        //Enumy
        enum TypZarizeni
        {
            SinglePlayer,
            Server,
            Klient
        }

        enum TypPacketu
        {
            PohybHrace,
            ZiskatVelikostMapy,
            VraceniVelikostiMapy,
            ZiskatCastMapy,
            VraceniCastiMapy
        }

        //Promenne
        public string jmeno = "";
        private TypZarizeni typZarizeni = TypZarizeni.SinglePlayer;
        private Dictionary<string, Rectangle> hraci = new Dictionary<string, Rectangle>();
        private byte[] mapaVBytech;

        private List<IPEndPoint> klienti = new List<IPEndPoint>(); //Pro server
        private UdpClient udpServer; //Pro server
        private IPEndPoint odesilatel = new IPEndPoint(IPAddress.Any, PORT); //Pro server

        private UdpClient udpKlient; //Pro klienta
        private IPEndPoint adresaServeru = new IPEndPoint(IPAddress.Any, PORT); //Pro klient
        private int velikostMapyVBytech; //Pro klient
        private int pocetZiskanychCastiMapy;

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
            //Poslani pozice
            Kamera _kamera = hra.komponentaKamera._kamera;
            Point opravdovaPoziceKamery = new Vector2(-_kamera.GetViewMatrix().Translation.X / _kamera.zoom, -_kamera.GetViewMatrix().Translation.Y / _kamera.zoom).ToPoint();
            Point opravdovaVelikostOkna = new Vector2(hra.velikostOkna.X / _kamera.zoom, hra.velikostOkna.Y / _kamera.zoom).ToPoint();
            PosliData(Encoding.UTF8.GetBytes($"{(short)TypPacketu.PohybHrace};{jmeno};{opravdovaPoziceKamery.X};{opravdovaPoziceKamery.Y};{opravdovaVelikostOkna.X};{opravdovaVelikostOkna.Y}"));

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

            while(velikostMapyVBytech == -1)
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

            while(pocetZiskanychCastiMapy * MAX_VELIKOST_PACKETU < velikostMapyVBytech)
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
                udpKlient.Send(data, data.Length);
            else if (typZarizeni == TypZarizeni.Server)
                PosliVsemKlientum(data);
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
                    if (TypZarizeni.Klient == typZarizeni)
                        hra.komponentaKonzole.radky.Insert(0, new Radek("Error", Color.Red));
                    else
                        PosliData(Encoding.UTF8.GetBytes($"{(short)TypPacketu.VraceniVelikostiMapy};{mapaVBytech.Length}"));
                    break;
                case TypPacketu.VraceniVelikostiMapy:
                    velikostMapyVBytech = int.Parse(dataVStringu[1]);
                    break;
                case TypPacketu.ZiskatCastMapy:
                    int chtenaCastMapy = int.Parse(dataVStringu[1]) * MAX_VELIKOST_PACKETU;
                    string dataMapy = "";
                    for (int i = chtenaCastMapy; i < chtenaCastMapy + MAX_VELIKOST_PACKETU && i < mapaVBytech.Length; i++)
                        dataMapy += (char)mapaVBytech[i];
                    PosliData(Encoding.UTF8.GetBytes($"{(short)TypPacketu.VraceniCastiMapy};{int.Parse(dataVStringu[1])};{dataMapy}"));
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
            }
        }

        #region Klient
        public void PripojSeNaServer(IPAddress adresa)
        {
            if (typZarizeni != TypZarizeni.SinglePlayer)
                return;

            adresaServeru = new IPEndPoint(adresa, PORT);
            udpKlient = new UdpClient();
            udpKlient.Connect(adresaServeru);
            udpKlient.Send(new byte[0], 0);
            udpKlient.BeginReceive(new AsyncCallback(ReceiveCallbackClient), null);
            hra.komponentaKonzole.radky.Insert(0, new Radek("Úspěšně připojeno na server", Color.LimeGreen));
            typZarizeni = TypZarizeni.Klient;

            ZiskejDataMapy();
        }

        //Zpracovani dotazu serveru
        void ReceiveCallbackClient(IAsyncResult ar)
        {
            byte[] data = udpKlient.EndReceive(ar, ref adresaServeru);
            ZpracujData(data);
            udpKlient.BeginReceive(new AsyncCallback(ReceiveCallbackClient), null);
        }
        #endregion Klient

        #region Server
        public void SpustServer()
        {
            if (typZarizeni != TypZarizeni.SinglePlayer)
                return;

            udpServer = new UdpClient(PORT);
            udpServer.BeginReceive(new AsyncCallback(ReceiveCallback), null);
            hra.komponentaKonzole.radky.Insert(0, new Radek("Server spuštěn na 127.0.0.1:" + PORT));
            typZarizeni = TypZarizeni.Server;

            mapaVBytech = hra.komponentaMapa.PrevedMapuNaBytovePole();
        }

        //Zpracovani dotazu clienta
        void ReceiveCallback(IAsyncResult ar)
        {
            byte[] data = udpServer.EndReceive(ar, ref odesilatel);

            //Pokud je toto prvni dotaz tak si ho pridam do listu
            if (!klienti.Contains(odesilatel))
                klienti.Add(odesilatel);

            if (data.Length > 0)
            {
                ZpracujData(data);
            }
            udpServer.BeginReceive(new AsyncCallback(ReceiveCallback), null);
        }

        void PosliVsemKlientum(byte[] data)
        {
            foreach (IPEndPoint klient in klienti)
            {
                if (odesilatel != klient)
                    udpServer.Send(data, data.Length, klient);
            }
        }
        #endregion Server
    }
}