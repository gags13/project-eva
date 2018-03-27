using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models.ALEXA.Request
{
    public class BuildRequest
    {
        
        public Request request { get; set; }
        public Session session { get; set; }
        public string version { get; set; }

    }
}
