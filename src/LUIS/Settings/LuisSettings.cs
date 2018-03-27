using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LUIS.Intefaces;
namespace LUIS.Settings
{
    public class LuisSettings :ILuisSettings
    {
        public string LuisEndpoint { get; set; }
    }
}
