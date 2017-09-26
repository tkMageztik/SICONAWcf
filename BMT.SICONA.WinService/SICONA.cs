using BMT.SICONA.BE;
using BMT.SICONA.BL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace BMT.SICONA.WinService
{
    public partial class SICONA : ServiceBase
    {
        private System.Timers.Timer timer = null;

        private static List<CardsBE> Cards { set; get; }
        private static int InitialLenght { set; get; }
        private static int FinalLenght { set; get; }

        public SICONA()
        {
            InitializeComponent();
            double intervalo = Convert.ToDouble(ConfigurationManager.AppSettings["IntervaloTiempo"]);

            try
            {
                timer = new System.Timers.Timer(intervalo);
                timer.Elapsed += new ElapsedEventHandler(this.ServiceTimer_Tick);

                //testIBS();
                //RegistrarCoincidenciaCliente();
                //BLLog.FlatLog(" Se Ejecutó RegistrarCoincidenciaCliente()");
                //EnviarCorreosTransacciones();
                //BLLog.FlatLog(" Se Ejecutó EnviarCorreosTransacciones()");

                //BLLog.FlatLog(" Termina Inicialización de Servicio ");
            }
            catch (Exception ex)
            {
                //BLLog.FlatLog("  Excepción.WinServiceDE.Inicialización " + ex.Message);
            }
        }

        protected override void OnStart(string[] args)
        {
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Start();
        }

        protected override void OnStop()
        {
            timer.AutoReset = false;
            timer.Enabled = false;
        }

        public static void AppInitialize()
        {
            // This will get called on startup
#if DEBUG
            System.Diagnostics.Debug.WriteLine("Inicializando Servicio de Escucha de Antenas");
#endif
            Util.Util.LogProceso("Inicializando Servicio de Escucha de Antenas");

            Cards = new CardsBL().GetAllCards();
            new SICONA().StartListener();
            //new Initializer().StartListener_2();
        }

        private void ServiceTimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                timer.Enabled = true;
                this.timer.Start();
                //RegistrarCoincidenciaCliente();
                //BLLog.FlatLog(" Se Ejecutó RegistrarCoincidenciaCliente()");
                //EnviarCorreosTransacciones();
                //BLLog.FlatLog(" Se Ejecutó EnviarCorreosTransacciones()");
            }
            catch (Exception ex)
            {
                //BLLog.FlatLog(" Excepción ServiceTimer_Tick" + ex.Message.Trim());
            }
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


                                    new PlotBL().InsertPlot(
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
                    System.Diagnostics.Debug.WriteLine("ERROR: " + e.Message);
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
