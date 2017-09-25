using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Web;
using BMT.SICONA.Util;
using System.Text;
using BMT.SICONA.BL;
using BMT.SICONA.BE;

namespace BMT.SICONA.Wcf.App_Code
{
    public class Initializer
    {
        private static List<CardsBE> cards { set; get; }

        public static void AppInitialize()
        {
            // This will get called on startup
#if DEBUG
            System.Diagnostics.Debug.WriteLine("Inicializando Servicio de Escucha de Antenas");
#endif
            Util.Util.LogProceso("Inicializando Servicio de Escucha de Antenas");

            cards = new CardsBL().GetAllCards();
            new Initializer().StartListener();
        }
        
        const int LIMIT = 1; //5 concurrent clients
        private void StartListener()
        {
            List<int> puertos = new List<int>();

            puertos.Add(5494);
            puertos.Add(5495);
            puertos.Add(5496);
            puertos.Add(5497);

            foreach (int puerto in puertos)
            {
                IPAddress localAddr = IPAddress.Parse(ConfigurationManager.AppSettings["ServerIP"]);
                TcpListener listener = new TcpListener(localAddr, puerto);
                listener.Start();

#if DEBUG
            System.Diagnostics.Debug.WriteLine("Inicializando escucha desde puerto :" + puerto);
#endif
                Util.Util.LogProceso("Inicializando escucha desde puerto :" + puerto);

                for (int i = 0; i < LIMIT; i++)
                {
                    Thread t = new Thread(() => Service(listener));
                    t.Start();
                }
            }

        }

        private void Service(TcpListener listener)
        {
            while (true)
            {
                Socket soc = listener.AcceptSocket();

                Util.Util.LogProceso("Inicializando escucha desde socket con ip-puerto :" + soc.RemoteEndPoint);
#if DEBUG
            //System.Diagnostics.Debug.WriteLine("Inicializando escucha desde socket con ip-puerto :" + soc.RemoteEndPoint);
#endif
                try
                {

                    byte[] resp = new byte[2000]; //16 - 40960
                    var memStream = new MemoryStream();
                    var bytes = 0;

                    NetworkStream s = new NetworkStream(soc);
                    StreamReader sr = new StreamReader(s);


                    while (true)
                    {
                        //string name = sr.ReadLine();

                        //bytes = s.Read(resp, 0, resp.Length);
                        //memStream.Write(resp, 0, bytes);

                        string trama = "";
                        string cardID = "";

                        if (s.CanRead)
                        {
                            do
                            {
                                bytes = s.Read(resp, 0, resp.Length);
                                trama = Util.Util.ByteArrayToHexString(resp);

                                cardID = trama.Substring(0, 36);
                                if (cards.Exists(x => x.Id == cardID))
                                {
                                    System.Diagnostics.Debug.WriteLine("EXISTE " + cardID);
                                    Util.Util.LogProceso("EXISTE " + cardID);
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine("NO EXISTE " + cardID);
                                    Util.Util.LogProceso("NO EXISTE " + cardID);
                                }
                            }
                            while (s.DataAvailable);
                        }

                        //string trama = Util.Util.ByteArrayToHexString(resp);

                        System.Diagnostics.Debug.WriteLine(trama);
                        if (trama == "" || trama == null) break;

                    }
                    s.Close();
                }
                catch (Exception e)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("ERROR: "+ e.Message);
#endif

                    Util.Util.LogProceso("ERROR: " + e.Message);
                }
                finally
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("Disconnected: " + soc.RemoteEndPoint);
#endif
                    Util.Util.LogProceso("Disconnected: " + soc.RemoteEndPoint);
                    soc.Close();
                }
                //#if DEBUG
                //            System.Diagnostics.Debug.WriteLine("Disconnected: " + soc.RemoteEndPoint);
                //#endif
                //                soc.Close();
            }



        }

        //TODO: Usando contains (safe method)
        private void CheckCardIDByContains()
        {

        }

        //TODO: Usando substring
        private void CheckCardIDBySubstring() { }

    }
}