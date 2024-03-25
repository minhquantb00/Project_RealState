using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Design.Payloads.DTOs.Statistics
{
    public class RaitoSalesAndProfit
    {
        public int? TeamId { get; set; }
        public string TeamName { get; set; }
        public double RatioSales { get; set; }
        public double RatioProfit { get; set; }
        public string TotalSalesOfCompany { get; set; }
        public string TotalProfitOfCompany { get; set; }
    }
}
