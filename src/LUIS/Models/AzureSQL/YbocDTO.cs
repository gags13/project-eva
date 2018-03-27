using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LUIS.Models.AzureSQL
{
    public class YbocDTO
    {
        public int Id { get; set; }
        public string SetPosition { get; set; }
        public string FeedbackPos { get; set; }
        public DateTimeOffset EventEnqueuedUtcTime { get; set; }

        public string ConnectionDeviceId { get; set; }

        public List<YbocDTO> mapList(List<List<string>> allData) {
            List<YbocDTO> listYbocDTO = new List<YbocDTO>();
            foreach (var row in allData) {
                listYbocDTO.Add(new YbocDTO {
                    Id=Convert.ToInt32( row[0]),
                    SetPosition= row[1],
                    FeedbackPos= row[2],
                    EventEnqueuedUtcTime = Convert.ToDateTime(row[3]),
                    ConnectionDeviceId = row[4]

                });

            }
            return listYbocDTO;

        }
    }
}
