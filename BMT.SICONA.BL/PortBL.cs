﻿using BMT.SICONA.BE;
using BMT.SICONA.DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMT.SICONA.BL
{
    public class PortBL
    {
        public List<PortBE> GetAllPorts()
        {
            return new PortDA().GetAllPorts();
        }
    }
}
