using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace LUIS.Models.ALEXA.Response
{
    public class Card
    {
        [DefaultValue("Simple")]
        public string type { get; set; }
        [DefaultValue("Done")]
        public string content { get; set; }
        [DefaultValue("Project EVA")]
        public string title { get; set; }
    }
}
