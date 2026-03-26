using System.Configuration;
using System.Data.SqlClient;

namespace StudentAffairs
{
    public static class DB
    {
        public static SqlConnection GetConnection()
        {
            string connStr = ConfigurationManager.ConnectionStrings["StudentDB"].ConnectionString;
            return new SqlConnection(connStr);
        }
    }

}
