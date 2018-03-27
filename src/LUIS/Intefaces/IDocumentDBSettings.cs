using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LUIS.Intefaces
{
    public interface IDocumentDBSettings
    {
         string EndpointUrl { get; set; }
         string PrimaryKey { get; set; }
         string database { get; set; }
         string logCollection { get; set; }

         string slotValuesCollection { get; set; }
    }
}
