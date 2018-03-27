using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LUIS.Intefaces;
namespace LUIS.Settings
{
    public class TwilioSettings: ITwilioSettings
    {
        public string accountSid { get; set; }
        public string authToken { get; set; }
        public string from { get; set; }
    }
}
