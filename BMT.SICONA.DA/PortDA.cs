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
    public class PortDA
    {
        public List<PortBE> GetAllPorts()
        {
            using (IDbConnection db = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["Postgres"].ConnectionString))
            {
                return db.Query<PortBE>("Select * From Puertos").ToList();
            }
        }
    }
}
