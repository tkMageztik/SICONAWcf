using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMT.SICONA.BE
{
    public class PlotBE
    {
        public string id { get; set; }
        public DateTime fecha { set; get; }
        public string puerto { set; get; }
        public string ip_antena { set; get; }
        public string trama { set; get; }
        public bool procesado { get { return false; } }
    }
}
