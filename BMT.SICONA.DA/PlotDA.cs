using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMT.SICONA.BE;
using System.Data;
using System.Configuration;
using Npgsql;
using Dapper;

namespace BMT.SICONA.DA
{
    public class PlotDA
    {
        public int InsertPlot(PlotBE plotBE)
        {
            using (IDbConnection db = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["Postgres"].ConnectionString))
            {
                string sqlQuery = "INSERT INTO Trama values (@id,@fecha,@puerto,@ip_antena,@trama,@procesado) ";

                int rowsAffected = db.Execute(sqlQuery, plotBE);

                return rowsAffected;
            }
        }
    }
}
