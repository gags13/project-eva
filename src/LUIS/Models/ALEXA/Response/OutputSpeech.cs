using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace LUIS.Models.ALEXA.Response
{
    public class OutputSpeech
    {
        [DefaultValue("PlainText")]
        public string type { get; set; }

        [DefaultValue("Ok got it")]
        public string text { get; set; }
    }
}
