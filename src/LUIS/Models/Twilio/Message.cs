using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LUIS.Models.Twilio
{
    public class Message
    {
        public string msg { get; set; }
        public string from { get; set; }
        public string to { get; set; }
    }
}
