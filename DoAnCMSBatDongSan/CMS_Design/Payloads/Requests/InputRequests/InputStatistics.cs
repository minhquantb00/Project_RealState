using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Design.Payloads.Requests.InputRequests
{
    public class InputStatistics
    {
        public int? TeamId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set;}
    }
}
