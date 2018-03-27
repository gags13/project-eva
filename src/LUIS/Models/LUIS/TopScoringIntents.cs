using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LUIS.Models.LUIS
{
    public class TopScoringIntents
    {
        public string intent { get; set; }
        public double score { get; set; }
    }
}
