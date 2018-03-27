using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LUIS.Models.LUIS
{
    public class Response
    {
        public string query { get; set; }
        public Intent topScoringIntent { get; set; }
        public Intent[] intents { get; set; }
        public Entity[] entities { get; set; }
    }
}
