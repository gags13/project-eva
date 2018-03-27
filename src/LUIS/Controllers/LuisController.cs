using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http;
using System.Net.Http;
using System.IO;
using System.Text;
using Newtonsoft.Json;
//using Twilio;
//using Twilio.Types;
//using Twilio.Rest.Api.V2010.Account;
//using LUIS.Models.Twilio;
using LUIS.Settings;
using Models.YBOC;
using LUIS.Intefaces;
using LUIS.Models.AzureSQL;
using LUIS.Handler;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using LUIS.Models.CustomResponses;
using Microsoft.AspNetCore.Cors;
namespace LUIS.Controllers
{
    [EnableCors("CorsPolicy")]
    [Route("api/[controller]")]
    public class LuisController : Controller
    {
        private HttpClient httpclient;
        private ILuisSettings luisSettings;
        IDocumentDBSettings ddbSettings;
        IotHubHandler ih;
        AzureSqlHandler ass;
        IDocumentClient client;

        public LuisController(ILuisSettings luisSettings, IDocumentClient client, IDocumentDBSettings ddbSettings, IotHubHandler ih, AzureSqlHandler ass)
        {
            httpclient = new HttpClient();
            this.luisSettings = luisSettings;
            this.client = client;
            this.ddbSettings = ddbSettings;
            this.ih = ih;
            this.ass = ass;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<string> Get(string id)
        {
            var response = await httpclient.GetAsync(luisSettings.LuisEndpoint + id);

            string resultJSON = await response.Content.ReadAsStringAsync();

            LUIS.Models.LUIS.Response res = JsonConvert.DeserializeObject<LUIS.Models.LUIS.Response>(resultJSON);


            return res.entities.FirstOrDefault().entity;
        }

        // GET api/values/5
        [HttpPost]
        public async Task<LUIS.Models.LUIS.Response> LuisRedirect([FromBody]LUIS.Models.LUIS.Message msg)
        {
            var response = await httpclient.GetAsync(luisSettings.LuisEndpoint + msg.text);

            string resultJSON = await response.Content.ReadAsStringAsync();

            LUIS.Models.LUIS.Response res = JsonConvert.DeserializeObject<LUIS.Models.LUIS.Response>(resultJSON);


            return res;
        }




        [HttpPost("messages")]
        public async Task<LUIS.Models.LUIS.Message> PostMessage([FromHeader]LUIS.Models.LUIS.Message msg)
        {

            try
            {
                //await log(msg);
                LUIS.Models.LUIS.Response res = await GetLuisData(msg.text);
                string topIntent = res.intents.OrderByDescending(x => x.score).FirstOrDefault().intent;
                LUIS.Models.LUIS.Message toReturn = new Models.LUIS.Message()
                {
                    name = "pr0jectEva",
                    text = "sorry , I did not inderstand you, if you are feeling hot, simply say I am feeeling hot or something",
                    time = DateTimeOffset.Now.Millisecond
                };
                toReturn.text = await actionSwitch(topIntent, res);
                toReturn.sender = 0;
                return toReturn;
            }
            catch (Exception e)
            {
                //await log(e);

                return new LUIS.Models.LUIS.Message
                {
                    name = "pr0jectEva",
                    text = $"Error : {e.Message}",
                    time = DateTimeOffset.Now.Millisecond,
                    sender = 0
                };
            }



        }

        [HttpPost("msg")]
        public async Task<LUIS.Models.LUIS.Message> PostMessageMEM([FromBody]LUIS.Models.LUIS.Message msg)
        {

            try
            {
                //await log(msg);
                LUIS.Models.LUIS.Response res = await GetLuisData(msg.text);
                string topIntent = res.intents.OrderByDescending(x => x.score).FirstOrDefault().intent;
                LUIS.Models.LUIS.Message toReturn = new Models.LUIS.Message()
                {
                    name = "pr0jectEva",
                    text = "sorry , I did not inderstand you, if you are feeling hot, simply say I am feeeling hot or something",
                    time = DateTimeOffset.Now.Millisecond
                };

                toReturn.text = await actionSwitch(topIntent, res);
                toReturn.sender = 0;
                toReturn.data = getSeries();
                return toReturn;
            }
            catch (Exception e)
            {
                //await log(e);

                return new LUIS.Models.LUIS.Message
                {
                    name = "pr0jectEva",
                    text = $"Error : {e.Message}",
                    time = DateTimeOffset.Now.Millisecond,
                    sender = 0
                };
            }



        }


        public async Task<LUIS.Models.LUIS.Response> GetLuisData(string text)
        {
            var response = await httpclient.GetAsync(luisSettings.LuisEndpoint + text);
            string resultJSON = await response.Content.ReadAsStringAsync();
            LUIS.Models.LUIS.Response res = JsonConvert.DeserializeObject<LUIS.Models.LUIS.Response>(resultJSON);
            return res;
        }
        private async Task<string> actionSwitch(string topIntent, LUIS.Models.LUIS.Response res)
        {
            string responseText = String.Empty;
            switch (topIntent)
            {

                case "ComfortParameter":
                    responseText = await GetDataAndSetActuator(res.entities.FirstOrDefault().entity);
                    break;

                case "GetPosition":
                    responseText = $"It's Currently at {await GetData()}";

                    break;

                case "Response":
                    responseText = "Hi , how can I help you, I can control the airflow";
                    break;

                case "SetActuator":
                    responseText = await SetActuator(Convert.ToInt32(res.entities.LastOrDefault().resolution.values.FirstOrDefault()));
                    break;

                case "getTemperature": responseText = await getTemp();
                    break;

                case "lights":
                    string lightcurrstatus = await GetLights();
                    responseText = await setLights(res.entities.FirstOrDefault().entity,lightcurrstatus);
                    break;

                default:
                    responseText = "sorry , I did not inderstand you, if you are feeling hot, simply say I am feeeling hot or something";
                    break;


            }
            return responseText;
        }

        private async Task<int> GetData()
        {

            StringBuilder sb = new StringBuilder();
            sb.Append(@"SELECT top 1 [Id]
                            ,[SetPosition]
                            ,[FeedbackPos]
                            ,[EventEnqueuedUtcTime]
                            ,[ConnectionDeviceId]");
            sb.Append("FROM [tblSample]");
            sb.Append("where [ConnectionDeviceId]= 'aa'");
            sb.Append("order by [EventEnqueuedUtcTime] desc");

            List<List<string>> allResults = ass.SqlSelect(sb, 5);
            List<YbocDTO> allYbocData = new YbocDTO().mapList(allResults);
            double value = Convert.ToDouble(allYbocData.OrderByDescending(x => x.EventEnqueuedUtcTime).FirstOrDefault().SetPosition);

            return (int)value;
        }

        private async Task<string> SetActuator(int value)
        {
            string textResponse = $"The position has been set to {value}";
            //var someshit = Newtonsoft.Json.JsonConvert.DeserializeObject<deviceExplorer.ALEXA.Models.BuildRequest>(request);
            if (value < 0 || value > 100)
            {
                textResponse = $"Sorry I cannot set the value to {value}, the value must lie between zero to hundred";
            }
            else
            {
                if (value == 0)
                    value = 1;

                await SetActuatorPosition(value+"");
            }
            return textResponse;
        }

        private async Task<string> GetDataAndSetActuator(string param)
        {
            int value = await GetData();
            int prev = value;
            bool toIncrease = ToIncrease(param, "param");
            if (toIncrease)
            {
                value = value + 5;
                if (value > 100)
                    value = 100;
            }
            else
            {
                value = value - 5;
                if (value < 1)
                    value = 1;
            }



            await SetActuatorPosition(value+"");
            string textResponse = $"Ok got it";

            Models.ALEXA.Response.AlexaFullResponse response = new Models.ALEXA.Response.AlexaFullResponse().getDefaultResponse();

            if (((prev == 1 || prev == 0) && !toIncrease) || (prev == 100 && toIncrease))
            {
                List<SlotValues> res = FromDocDB("edgeResponses", ddbSettings.database, ddbSettings.slotValuesCollection);

                if (prev == 100)
                    textResponse = RandomFromList(res.FirstOrDefault().max);
                else
                    textResponse = RandomFromList(res.FirstOrDefault().min);
                response.response.outputSpeech.text = textResponse;
                response.response.card.content = $"Setting to value {value}";
            }
            else
            {
                List<SlotValues> res = FromDocDB("midResponses", ddbSettings.database, ddbSettings.slotValuesCollection);

                if (toIncrease)
                    textResponse = RandomFromList(res.FirstOrDefault().max);
                else
                    textResponse = RandomFromList(res.FirstOrDefault().min);
                response.response.outputSpeech.text = textResponse;
                response.response.card.content = $"Setting to value {value}";

            }
            return textResponse;
        }




        private async Task SetActuatorPosition(string val,string device ="aa")
        {

            Message msg = new Message();
            Parameters param = new Parameters();
            //int value = val;
            param.Position = Convert.ToInt32(val);
            msg.Name = "SetAirResistance";
            msg.Parameters = param;
            SendToDevice send = new SendToDevice();
            send.id = device;
            send.message = msg;

            await ih.SendCloudToDeviceMessageAsync(send.id, JsonConvert.SerializeObject(send.message));

        }

        private bool ToIncrease(string controlParam, string id)
        {

            List<string> toIncrease = new List<string>
            { "decrease the temperature",
              "increase the air flow",
              "increase air flow",
              "increase airflow",
              "increase the airflow",
              "sweaty",
              "sticky",
              "hot",
              "decrease temperature",
              "enclosed",
              "cramped",
              "airless",
              "confined",
              "claustrophobic"
            };

            List<SlotValues> allSlotValues = FromDocDB(id, ddbSettings.database, ddbSettings.slotValuesCollection);
            if (allSlotValues.FirstOrDefault().values.Contains(controlParam))
                return true;
            return false;
        }

        private string RandomFromList(List<string> allResponses)
        {

            int size = allResponses.Count;
            Random random = new Random(DateTime.UtcNow.Millisecond);
            return allResponses[random.Next(0, size)];

        }

        private List<SlotValues> FromDocDB(string id, string database, string collection)
        {
            FeedOptions queryOptions = new FeedOptions
            {
                MaxItemCount = -1,
                EnableCrossPartitionQuery = true
            };

            string query = $"select * from c where c.id='{id}'";
            List<SlotValues> allSlotValues = client.CreateDocumentQuery<SlotValues>(UriFactory.CreateDocumentCollectionUri(database, collection), query, queryOptions).ToList();

            return allSlotValues;
        }
        //[HttpPost("message")]
        //public async Task PostMessage([FromBody]Message msg)
        //{

        //    TwilioClient.Init(accountSid, authToken);

        //    var to = new PhoneNumber(msg.to);
        //    var message = MessageResource.Create(
        //    to,
        //    from: new PhoneNumber(from),
        //    body: msg.msg);

        //}
        private async Task log(Object request)
        {
            await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri("projectEVA", "LuisLog"), request);

        }

        ///////////////////////
        private Dictionary<DateTimeOffset, double> getData()
        {
            Dictionary<DateTimeOffset, double> simData = new Dictionary<DateTimeOffset, double>();
            Random rnd = new Random(DateTime.Now.Millisecond);

            int total = rnd.Next(1, 10);
            int seed = 0;
            int prev = 0;
            DateTimeOffset dt = DateTimeOffset.UtcNow;
            double val = rnd.NextDouble();
            for (int i = 1; i <= total; i++)
            {
                seed = rnd.Next(1, 23);
                val = rnd.Next(1, 100);
                simData.Add(dt.AddHours(-1 * (seed + prev)), val * -1);
                prev += seed;
            }

            return simData;
        }

        private Dictionary<string, Dictionary<DateTimeOffset, double>> getSeries()
        {
            Dictionary<string, Dictionary<DateTimeOffset, double>> series = new Dictionary<string, Dictionary<DateTimeOffset, double>>();

            Random rnd = new Random(DateTime.Now.Second);

            int total = rnd.Next(1, 10);

            for (int i = 1; i <= total; i++) {
                int num = rnd.Next();
                series.Add($"AIALA:{num}:{total}", getData());

            }

            return series;
        }

        private async Task<string> getTemp() {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"SELECT top 1 [Id]
                            ,[SetPosition]
                            ,[FeedbackPos]
                            ,[EventEnqueuedUtcTime]
                            ,[ConnectionDeviceId]");
            sb.Append("FROM [tblSample]");
            sb.Append($"where [ConnectionDeviceId]= 'bb'");
            sb.Append("order by [EventEnqueuedUtcTime] desc");

            List<List<string>> allResults = ass.SqlSelect(sb, 5);
            List<YbocDTO> allYbocData = new YbocDTO().mapList(allResults);
            double value = Convert.ToDouble(allYbocData.OrderByDescending(x => x.EventEnqueuedUtcTime).FirstOrDefault().FeedbackPos);
            string textResponse = $"Currently the Temperature is around {value} degree centigrade";
            return textResponse;
        }

        private async Task<string> setLights(string value, string lightOnOff) {
            
            string toReturn = "okay";
            if (value == lightOnOff)
            {

                if (value == "on")
                {
                    toReturn = "I think it's already on , but I still placed a request";
                }
                else
                {
                    toReturn = "Well the lights seems to be off, but I did send the command just to be sure";
                }
            }
            else
            {
                toReturn = $"Well I turned the lights {value}";
            }

            string toSet = "1";
            if (value == "on")
                toSet = "0";

            await SetActuatorPosition(toSet,"cc");
            return toReturn;
        }

        private async Task<string> GetLights()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"SELECT top 1 [Id]
                            ,[SetPosition]
                            ,[FeedbackPos]
                            ,[EventEnqueuedUtcTime]
                            ,[ConnectionDeviceId]");
            sb.Append("FROM [tblSample]");
            sb.Append("where [ConnectionDeviceId]= 'cc'");
            sb.Append("order by [EventEnqueuedUtcTime] desc");

            List<List<string>> allResults = ass.SqlSelect(sb, 5);
            List<YbocDTO> allYbocData = new YbocDTO().mapList(allResults);
            double value = Convert.ToDouble(allYbocData.OrderByDescending(x => x.EventEnqueuedUtcTime).FirstOrDefault().SetPosition);




            //Models.ALEXA.Response.AlexaFullResponse response = new Models.ALEXA.Response.AlexaFullResponse().getDefaultResponse();
            string lightOnOff = "on";
            if (value == 1)
                lightOnOff = "off";
            //response.response.card.content = $"The current state of the lights is {lightOnOff}";
            //response.response.outputSpeech.text = $"Currently , the lights are {lightOnOff}";


            return lightOnOff;
        }
    }
}
