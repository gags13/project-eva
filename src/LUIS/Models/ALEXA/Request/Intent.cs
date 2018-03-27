using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models.ALEXA.Request
{
    public class Intent
    {
        public string name { get; set; }
        public Slots slots { get; set; }
    }
}
