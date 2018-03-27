using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LUIS.Models.LUIS
{
    public class Message
    {
        public string name { get; set; }

        public string text { get; set; }

        public string thumbnail { get; set; }

        public long time { get; set; }

        public int sender { get; set; }

        public Dictionary<string, Dictionary<DateTimeOffset,double>> data { get; set; }

    }
}
