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

        //Enumy
        enum TypZarizeni
        {
            SinglePlayer,
            Server,
            Klient
        }

        //Promenne
        public string jmeno = "";
        private TypZarizeni typZarizeni = TypZarizeni.SinglePlayer;
        private Dictionary<string, Rectangle> hraci = new Dictionary<string, Rectangle>();

        private List<IPEndPoint> klienti = new List<IPEndPoint>(); //Pro server
        private UdpClient udpServer; //Pro server
        private IPEndPoint odesilatel = new IPEndPoint(IPAddress.Any, PORT); //Pro server

        private UdpClient udpKlient; //Pro klienta
        private IPEndPoint adresaServeru = new IPEndPoint(IPAddress.Any, PORT); //Pro klient

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
            Kamera _kamera = hra.komponentaKamera._kamera;
            Vector2 nula = new Vector2(_kamera.pozice.X + _kamera.GetViewMatrix().Translation.X / _kamera.zoom, _kamera.pozice.Y + _kamera.GetViewMatrix().Translation.Y / _kamera.zoom);
            Point opravdovaPoziceKamery = new Vector2(-_kamera.GetViewMatrix().Translation.X / _kamera.zoom, -_kamera.GetViewMatrix().Translation.Y / _kamera.zoom).ToPoint();
            Point opravdovaVelikostOkna = new Vector2(hra.velikostOkna.X / _kamera.zoom, hra.velikostOkna.Y / _kamera.zoom).ToPoint();
            PosliData(Encoding.UTF8.GetBytes($"{jmeno};{opravdovaPoziceKamery.X};{opravdovaPoziceKamery.Y};{opravdovaVelikostOkna.X};{opravdovaVelikostOkna.Y}"));

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            hra._spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: hra.komponentaKamera._kamera.GetViewMatrix());
            foreach (var hrac in hraci)
            {
                hra._spriteBatch.DrawString(hra.comicSans, hrac.Key, new Vector2(hrac.Value.X, hrac.Value.Y - hra.comicSans.MeasureString(jmeno).Y * 0.05f / hra.komponentaKamera._kamera.zoom), Color.Red,
                    0, Vector2.Zero, 0.05f / hra.komponentaKamera._kamera.zoom, SpriteEffects.None, 0);

                int tloustkaOkraje = (int)(2 / hra.komponentaKamera._kamera.zoom);
                hra._spriteBatch.Draw(hra.pixel, new Rectangle(hrac.Value.X, hrac.Value.Y, hrac.Value.Width, tloustkaOkraje), Color.Red);
                hra._spriteBatch.Draw(hra.pixel, new Rectangle(hrac.Value.X, hrac.Value.Y, tloustkaOkraje, hrac.Value.Height), Color.Red);
                hra._spriteBatch.Draw(hra.pixel, new Rectangle(hrac.Value.X, hrac.Value.Y + hrac.Value.Height, hrac.Value.Width + tloustkaOkraje, tloustkaOkraje), Color.Red);
                hra._spriteBatch.Draw(hra.pixel, new Rectangle(hrac.Value.X + hrac.Value.Width, hrac.Value.Y, tloustkaOkraje, hrac.Value.Height + tloustkaOkraje), Color.Red);
            }
            hra._spriteBatch.End();
            base.Draw(gameTime);
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

            Rectangle poziceHrace = new Rectangle(int.Parse(dataVStringu[1]), int.Parse(dataVStringu[2]), int.Parse(dataVStringu[3]), int.Parse(dataVStringu[4]));

            if (!hraci.ContainsKey(dataVStringu[0]))
                hraci.Add(dataVStringu[0], poziceHrace);
            else
                hraci[dataVStringu[0]] = poziceHrace;
        }

        #region Klient
        public void PripojSeNaServer(IPAddress adresa)
        {
            if (typZarizeni != TypZarizeni.SinglePlayer)
                return;

            typZarizeni = TypZarizeni.Klient;
            adresaServeru = new IPEndPoint(adresa, PORT);
            udpKlient = new UdpClient();
            udpKlient.Connect(adresaServeru);
            udpKlient.Send(new byte[0], 0);
            udpKlient.BeginReceive(new AsyncCallback(ReceiveCallbackClient), null);
            hra.komponentaKonzole.radky.Insert(0, new Radek("Úspěšně připojeno na server", Color.LimeGreen));
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

            typZarizeni = TypZarizeni.Server;
            udpServer = new UdpClient(PORT);
            udpServer.BeginReceive(new AsyncCallback(ReceiveCallback), null);
            hra.komponentaKonzole.radky.Insert(0, new Radek("Server spuštěn na 127.0.0.1:" + PORT));
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
                PosliVsemKlientum(data);
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