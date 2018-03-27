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
using LUIS.Handler;
using Models.YBOC;
using Models.ALEXA.Request;
using Microsoft.Azure.Documents.Client;
using LUIS.Settings;
using Microsoft.Azure.Documents;
using LUIS.Intefaces;
using LUIS.Models.AzureSQL;
using LUIS.Models.CustomResponses;
using LUIS.Models.ALEXA.Response;

namespace LUIS.Controllers
{
    [Route("api/[controller]")]
    public class AlexaController : Controller
    {
        IDocumentClient client;
        IDocumentDBSettings ddbSettings;
        IotHubHandler ih;
        AzureSqlHandler ass;
        public AlexaController(IDocumentClient client, IDocumentDBSettings ddbSettings, IotHubHandler ih, AzureSqlHandler ass)
        {
            this.client = client;
            this.ddbSettings = ddbSettings;
            this.ih = ih;
            this.ass = ass;
        }

        [HttpGet("{id}")]
        public async Task<string> Get(string id)
        {
            return "test" + id;
        }

        [HttpPost("iothub")]
        public async Task<ObjectResult> iothub([FromBody]SendToDevice send)
        {

            try
            {

                if (send.message.Parameters.Position == 0)
                    send.message.Parameters.Position = 1;

                await ih.SendCloudToDeviceMessageAsync(send.id, JsonConvert.SerializeObject(send.message));

                return new OkObjectResult(200);
            }
            catch (Exception e)
            {

                return new BadRequestObjectResult(e);
            }
        }

        [HttpPost]
        public async Task<dynamic> answer([FromBody]BuildRequest request)
        {
            //var log_await = log(request);sss
            Models.ALEXA.Response.AlexaFullResponse toReturn = new Models.ALEXA.Response.AlexaFullResponse().getDefaultResponse();


            switch (request.request.intent.name)
            {

                case "ComfortParameter":

                    try {
                        string bad = request.request.intent.slots.badFeeling.value;
                        if (bad == null)
                            toReturn = await ActionGetDataAndCreateRequest(request);
                        else
                            toReturn = await ActionBadFeeling(request);
                    }
                    catch(Exception e) { toReturn = await ActionGetDataAndCreateRequest(request); }
                    
                        
                    break;
                case "SetActuator":
                    toReturn = await ActionSetActuator(request);
                    break;
                case "presentation": toReturn = await ActionPresenter(request); break;

                case "ReadProperty": toReturn = await ActionGetData(request); break;
                case "Lights": string lightOnOf = await ActionGetLights(request);
                    toReturn = await ActionSetLights(request,lightOnOf);
                    break;
                //case "ReadProperty": toReturn = await ActionGetData(request); break;



                default:
                    toReturn.response.outputSpeech.text = "sorry didn't get you";
                    break;

            }
            //await log_await;
            return toReturn;

        }

        private async Task<AlexaFullResponse> ActionSetLights(BuildRequest request,string lightOnOff)
        {
            string value = request.request.intent.slots.lightParam.value;
            string toReturn = "okay";
            if (value == lightOnOff) {

                if (value == "on")
                {
                    toReturn = "I think it's already on , but I still placed a request";
                }
                else {
                    toReturn = "Well the lights seems to be off, but I did send the command just to be sure";
                }
            }
            else {
                toReturn = $"Well I turned the lights {value}";
            }
            string toSet = "1";
            if (value == "on")
                toSet = "0";
            await SetActuatorPosition(toSet,"cc");
            Models.ALEXA.Response.AlexaFullResponse response = new Models.ALEXA.Response.AlexaFullResponse().getDefaultResponse();
            response.response.card.content = toReturn;
            response.response.outputSpeech.text = toReturn;
            return response;
        }



        /* Private Methods */
        private async Task<Models.ALEXA.Response.AlexaFullResponse> ActionSetActuator(BuildRequest request)
        {


            int value = Convert.ToInt16(request.request.intent.slots.value.value);


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

                await SetActuatorPosition(value+"", "actuator");
            }

            Models.ALEXA.Response.AlexaFullResponse response = new Models.ALEXA.Response.AlexaFullResponse().getDefaultResponse();
            response.response.card.content = textResponse;
            response.response.outputSpeech.text = textResponse;
            return response;

        }

        private async Task<Models.ALEXA.Response.AlexaFullResponse> ActionPresenter(BuildRequest request)
        {



            string textResponse = $"";

            switch (request.request.intent.slots.slide.value)
            {

                case "Introduction":
                case "introduction":
                    textResponse = "I am Eva , I can enable interaction on existing systems with minimalistic changes. " +
                        "Being a bit technical , I am an interaction enabling service. "+"Currently I am compatible with the smart actuator";
                    break;

                default:
                    textResponse = "So hopefully you had a nice time, I am still work in progress . This is Eva Signing off";
                    break;
            }


            Models.ALEXA.Response.AlexaFullResponse response = new Models.ALEXA.Response.AlexaFullResponse().getDefaultResponse();
            response.response.card.content = textResponse;
            response.response.outputSpeech.text = textResponse;
            return response;

        }

        private async Task<Models.ALEXA.Response.AlexaFullResponse> ActionGetDataAndCreateRequest(BuildRequest request)
        {

            StringBuilder sb = new StringBuilder();
            sb.Append(@"SELECT top 1 [Id]
                            ,[SetPosition]
                            ,[FeedbackPos]
                            ,[EventEnqueuedUtcTime]
                            ,[ConnectionDeviceId]");
            sb.Append("FROM [tblSample]");
            sb.Append("where [ConnectionDeviceId]= 'actuator'");
            sb.Append("order by [EventEnqueuedUtcTime] desc");

            List<List<string>> allResults = ass.SqlSelect(sb, 5);
            List<YbocDTO> allYbocData = new YbocDTO().mapList(allResults);
            int value = (int)Convert.ToDouble(allYbocData.OrderByDescending(x => x.EventEnqueuedUtcTime).FirstOrDefault().SetPosition);
            int prev = value;
            bool toIncrease = ToIncrease(request.request.intent.slots.param.value, "param");
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



            await SetActuatorPosition(value+"", "actuator");


            Models.ALEXA.Response.AlexaFullResponse response = new Models.ALEXA.Response.AlexaFullResponse().getDefaultResponse();

            if (((prev == 1 || prev == 0) && !toIncrease) || (prev == 100 && toIncrease))
            {
                // ddb removal code
                //List<SlotValues> res = FromDocDB("edgeResponses", ddbSettings.database, ddbSettings.slotValuesCollection);
                string textResponse = "";
                if (prev == 100)
                    textResponse = "Sorry I cannot move beyond hundred percentage";//RandomFromList(res.FirstOrDefault().max);
                else
                    textResponse = "The actuator is already closed";
                response.response.outputSpeech.text = textResponse;
                response.response.card.content = $"Setting to value {value}";
            }
            else
            {
                // ddb removal code
                //List<SlotValues> res = FromDocDB("midResponses", ddbSettings.database, ddbSettings.slotValuesCollection);
                string textResponse = "";
                if (toIncrease)
                    textResponse = "I increased the airflow a bit, let me know if you are feeling alright after sometime";//RandomFromList(res.FirstOrDefault().max);
                else
                    textResponse = "I decreased the airflow a bit, take a few minutes and let me know if you are comfortable";//RandomFromList(res.FirstOrDefault().min);
                response.response.outputSpeech.text = textResponse;
                response.response.card.content = $"Setting to value {value}";

            }


            return response;

        }

        private async Task<Models.ALEXA.Response.AlexaFullResponse> ActionGetData(BuildRequest request)
        {
            
            string deviceID = "bb";
            try
            {
                string getPosition = request.request.intent.slots.getTemperature.value;
                if (getPosition == null)
                    deviceID = "actuator";
            }
            catch {
                
                    deviceID = "actuator";//actuator
            }

            

            StringBuilder sb = new StringBuilder();
            sb.Append(@"SELECT top 1 [Id]
                            ,[SetPosition]
                            ,[FeedbackPos]
                            ,[EventEnqueuedUtcTime]
                            ,[ConnectionDeviceId]");
            sb.Append("FROM [tblSample]");
            sb.Append($"where [ConnectionDeviceId]= '{deviceID}'");
            sb.Append("order by [EventEnqueuedUtcTime] desc");

            List<List<string>> allResults = ass.SqlSelect(sb, 5);
            List<YbocDTO> allYbocData = new YbocDTO().mapList(allResults);
            double value = Convert.ToDouble(allYbocData.OrderByDescending(x => x.EventEnqueuedUtcTime).FirstOrDefault().SetPosition);




            Models.ALEXA.Response.AlexaFullResponse response = new Models.ALEXA.Response.AlexaFullResponse().getDefaultResponse();
            string textResponse = $"Currently the Damper is {value} percentage open";
            if(deviceID=="bb")
                textResponse = $"Currently the Temperature is around {value} degree centigrade";
            response.response.card.content = textResponse;
            response.response.outputSpeech.text = textResponse;
            return response;

        }
        private async Task<string> ActionGetLights(BuildRequest request)
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
        private async Task log(BuildRequest request)
        {
            await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri("projectEVA", "alexalog"), request);

        }

        private async Task SetActuatorPosition(string val,string deviceId= "actuator")
        {

            Message msg = new Message();
            Parameters param = new Parameters();
            param.Position = Convert.ToInt32(val);
            msg.Name = "SetAirResistance";
            msg.Parameters = param;
            SendToDevice send = new SendToDevice();
            send.id = deviceId;
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
            // ddb removal code
            //List<SlotValues> allSlotValues = FromDocDB(id, ddbSettings.database, ddbSettings.slotValuesCollection);
            //if (allSlotValues.FirstOrDefault().values.Contains(controlParam))
            //    return true;

            if (toIncrease.Contains(controlParam))
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

        private async Task<Models.ALEXA.Response.AlexaFullResponse> ActionBadFeeling(BuildRequest request)
        {


            Models.ALEXA.Response.AlexaFullResponse response = new Models.ALEXA.Response.AlexaFullResponse().getDefaultResponse();

            //ddb removal code
            //List<SlotValues> res = FromDocDB("badFeeling", ddbSettings.database, ddbSettings.slotValuesCollection);

            string textResponse = "";
            //ddb removal code
            //textResponse = RandomFromList(res.FirstOrDefault().values);
            textResponse = "Ok I get it but you need to be a bit more specific , do you want me to lower the temperature or increase it ?";

            response.response.outputSpeech.text = textResponse;
            response.response.card.content = textResponse;


            return response;

        }

    }
}
