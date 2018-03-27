using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Twilio;
using Twilio.Types;
using Twilio.Rest.Api.V2010.Account;
using LUIS.Models.Twilio;
using LUIS.Settings;
using LUIS.Intefaces;
namespace LUIS.Controllers
{
    [Route("api/[controller]")]
    public class TwilioController : Controller
    {
        ITwilioSettings twilioSettings;
        public TwilioController(ITwilioSettings setting)
        {
            this.twilioSettings = setting;
        }
        [HttpPost("message")]
        public async Task PostMessage([FromBody]Message msg)
        {
            TwilioClient.Init(twilioSettings.accountSid, twilioSettings.authToken);
            var to = new PhoneNumber(msg.to);
            var message = MessageResource.Create(
            to,
            from: new PhoneNumber(twilioSettings.from),
            body: msg.msg);

        }
    }
}
