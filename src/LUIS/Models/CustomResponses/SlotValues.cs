using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LUIS.Models.CustomResponses
{
    public class SlotValues
    {
        public string id { get; set; }
        public List<string> values { get; set; }

        public List<string> min { get; set; }
        public List<string> max { get; set; }
    }
}
