using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LUIS.Intefaces;
namespace LUIS.Settings
{
    public class DocumentDBSettings:IDocumentDBSettings
    {
        public string EndpointUrl { get; set; }
        public string PrimaryKey { get; set; }
        public string database { get; set; }
        public string logCollection { get; set; }

        public string slotValuesCollection { get; set; }
    }
    
}
