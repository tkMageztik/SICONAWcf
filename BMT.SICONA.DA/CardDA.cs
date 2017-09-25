using BMT.SICONA.BE;
using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMT.SICONA.DA
{
    public class CardDA
    {

        public List<CardsBE> GetAllCards()
        {
            using (IDbConnection db = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["Postgres"].ConnectionString))
            {
                return db.Query<CardsBE>("Select * From Cards").ToList();
            }
        }

    }
}
