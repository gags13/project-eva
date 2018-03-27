using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LUIS.Intefaces
{
    public interface ITwilioSettings
    {
         string accountSid { get; set; }
         string authToken { get; set; }
         string from { get; set; }
    }
}
