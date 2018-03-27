using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LUIS.Intefaces
{
    public interface IAzureSqlSettings
    {
        string DataSource { get; set; }
        string UserID { get; set; }

        string Password { get; set; }
        string InitialCatalog { get; set; }
    }
}
