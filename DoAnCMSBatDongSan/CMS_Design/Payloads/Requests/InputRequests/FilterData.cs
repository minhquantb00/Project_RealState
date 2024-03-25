using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Design.Payloads.Requests.InputRequests
{
    public class FilterData
    {
        public string? Keyword { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? BuildStart { get; set; }
        public DateTime? BuildEnd { get; set; }
        public string? SortType { get; set; }
    }
}
