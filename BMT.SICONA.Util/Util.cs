using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMT.SICONA.Util
{
    public class Util
    {
        public static void LogProceso(string sMensaje)
        {
            string dirLog = ConfigurationManager.AppSettings["DirLog"].ToString().Trim();
            string ArcExt = ConfigurationManager.AppSettings["ArcExt"].ToString().Trim();

            DateTime fechaHora = DateTime.Now;
            string fileName = String.Format("{0:ddMMyyyy}", fechaHora);

            string path = dirLog + fileName + ArcExt;
            try
            {
                if (File.Exists(path))
                {
                    File.AppendAllText(path, DateTime.Now.ToString() + " | " + sMensaje.Trim() + " \r\n");
                }
                else
                {
                    using (var file = File.Create(path))
                    {
                        file.Close();
                    }
                    File.AppendAllText(path, DateTime.Now.ToString() + " | " + sMensaje.Trim() + " \r\n");

                }
            }
            catch { }

            //ADLog.LogProceso(" Se evaluaron los clientes correctamente.");
        }

        public static string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0'));
            return sb.ToString().ToUpper();

        }
    }
}
