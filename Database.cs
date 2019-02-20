using System.Data.SqlServerCe;

namespace HastaTakip
{
    public  class Database
    {
        private static readonly string Veritabanıadresi = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        //private const string Veritabanıadresi = @"Data Source=C:\Users\goksem\Documents\Expression\Blend 4\Projects\HastaTakip\HastaTakip\bin\Debug" + "\\data.sdf;Max Database Size=4091";

        public static SqlCeConnection Connection = new SqlCeConnection(Veritabanıadresi);
        public  Veriler Veriler = new Veriler(Connection);
    }
}