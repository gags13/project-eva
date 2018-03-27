using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LUIS.Intefaces;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace LUIS.Handler
{
    public class AzureSqlHandler
    {
        IAzureSqlSettings settings;
        public AzureSqlHandler(IAzureSqlSettings settings)
        {
            this.settings = settings;
        }

        private SqlConnectionStringBuilder getBuilder() {

            return new SqlConnectionStringBuilder
            {
                DataSource = settings.DataSource,
                UserID=settings.UserID,
                Password=settings.Password,
                InitialCatalog=settings.InitialCatalog
            };
        }

        public List<List<string>> SqlSelect(StringBuilder sb,int columnCount) {

            List<List<string>> allResults = new List<List<string>>();
            SqlConnectionStringBuilder builder = getBuilder();
            string connectionString = builder.ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                using (SqlCommand command = new SqlCommand(sb.ToString(), connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            List<string> rowList = new List<string>();
                            for (int i = 0; i < columnCount; i++)
                                rowList.Add(reader.GetValue(i)+"");
                            allResults.Add(rowList);
                            
                        }
                    }

                }

                }
            return allResults;

            }
    }
}
