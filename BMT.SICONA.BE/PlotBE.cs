using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMT.SICONA.BE
{
    public class PlotBE
    {
        public string id_trama { get; set; }
        public string cabecera { get; set; }
        public DateTime fecha { get; set; }
        public string puerto { get; set; }
        public string ip_antena { get; set; }
        public string trama { get; set; }
        public bool procesado { get { return false; } }
    }
}
