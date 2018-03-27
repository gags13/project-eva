using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace LUIS.Models.ALEXA.Response
{
    public class AlexaFullResponse
    {
        [DefaultValue("1.0")]
        public string version { get; set; }
        
        public object sessionAttributes { get; set; }

        public Response response { get; set; }

        public AlexaFullResponse getDefaultResponse() {
            AlexaFullResponse toReturn = new AlexaFullResponse();
            toReturn.version = "1.0";
            toReturn.sessionAttributes = new object();
            toReturn.response = new Response {
                shouldEndSession=true,
                card= new Card {
                    type = "Simple",
                    title = "Project EVA",
                    content = "Done"
                },
                outputSpeech = new OutputSpeech {
                    type = "PlainText",
                    text = "OK got it"

                }

            };
            return toReturn;
        }
    }
}
