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
using BMT.SICONA.DA;

namespace BMT.SICONA.Wcf.App_Code
{
    public class Initializer
    {
        private static List<CardsBE> Cards { set; get; }
        private static int InitialLenght { set; get; }
        private static int FinalLenght { set; get; }

        public static void AppInitialize()
        {
            // This will get called on startup
#if DEBUG
            System.Diagnostics.Debug.WriteLine("Inicializando Servicio de Escucha de Antenas");
#endif
            Util.Util.LogProceso("Inicializando Servicio de Escucha de Antenas");

            Cards = new CardsBL().GetAllCards();
            new Initializer().StartListener();
        }

        const int LIMIT = 1; //5 concurrent clients
        private void StartListener()
        {
            List<PortBE> puertos = new List<PortBE>();

            puertos = new PortBL().GetAllPorts();

            foreach (PortBE puerto in puertos)
            {

                InitialLenght = Convert.ToInt32(ConfigurationManager.AppSettings["InitialLenght"]);
                FinalLenght = Convert.ToInt32(ConfigurationManager.AppSettings["FinalLenght"]);

                IPAddress localAddr = IPAddress.Parse(ConfigurationManager.AppSettings["ServerIP"]);
                TcpListener listener = new TcpListener(localAddr, Convert.ToInt32(puerto.Puerto));
                listener.Start();

#if DEBUG
            System.Diagnostics.Debug.WriteLine("Inicializando escucha desde puerto :" + puerto.Puerto);
#endif
                Util.Util.LogProceso("Inicializando escucha desde puerto :" + puerto.Puerto);
                try
                {
                    for (int i = 0; i < LIMIT; i++)
                    {
                        Thread t = new Thread(() => Service(listener));
                        t.Start();
                    }
                }
                catch (Exception exc)
                {
                    Util.Util.LogProceso("Fuera del entorno..." + exc.Message);
                }
            }

        }

        private void Service(TcpListener listener)
        {
            while (true)
            {

                Socket soc = null;
                try
                {
                    soc = listener.AcceptSocket();
                }
                catch (Exception exc)
                {
                    Util.Util.LogProceso("Fuera del entorno... 2" + exc.Message);
                }

                if (soc == null)
                {
                    continue;
                }

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


                                cardID = trama.Substring(InitialLenght, FinalLenght);
                                if (Cards.Exists(x => x.Id == cardID))
                                {
                                    System.Diagnostics.Debug.WriteLine("EXISTE " + cardID);
                                    Util.Util.LogProceso("EXISTE " + cardID);


                                    new PlotDA().InsertPlot(
                                        new PlotBE()
                                        {
                                            id = "",
                                            fecha = DateTime.Now,
                                            ip_antena = soc.RemoteEndPoint.ToString(),
                                            trama = cardID,
                                            puerto = ""
                                        });
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