using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LUIS.Intefaces;
namespace LUIS.Settings
{
    public class AzureSqlSettings : IAzureSqlSettings
    {
        public string DataSource { get; set; }
        public string UserID { get ; set ; }
        public string Password { get ; set ; }
        public string InitialCatalog { get; set; }
    }
}
