using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models.ALEXA.Request
{
    public class Session
    {
        public string sessionId { get; set; }
        public Application application { get; set; }
        public object attributes { get; set; }

        public User user { get; set; }

    }
}
