using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Design.Payloads.DTOs.Statistics
{
    public class DataResponseStatistics
    {
        public int? teamId { get; set; }
        public string teamName { get; set; }
        public StatisticSalesAndProfit StatisticSalesAndProfit { get; set; }
    }
}
