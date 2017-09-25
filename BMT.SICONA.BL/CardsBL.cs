using BMT.SICONA.BE;
using BMT.SICONA.DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMT.SICONA.BL
{
    public class CardsBL
    {
        public List<CardsBE> GetAllCards()
        {
            return new CardDA().GetAllCards();
        }
    }
}
