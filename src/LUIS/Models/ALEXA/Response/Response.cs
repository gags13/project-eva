using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.ComponentModel;

namespace LUIS.Models.ALEXA.Response
{
    public class Response
    {
        public OutputSpeech outputSpeech { get; set; }

        public Card card { get; set; }

        [DefaultValue(true)]
        public bool shouldEndSession { get; set; }
    }
}
