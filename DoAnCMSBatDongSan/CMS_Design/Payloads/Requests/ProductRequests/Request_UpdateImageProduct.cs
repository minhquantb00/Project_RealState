using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Design.Payloads.Requests.ProductRequests
{
    public class Request_UpdateImageProduct
    {
        public int? ProductImgId { get; set; }
        [DataType(DataType.Upload)]
        public IFormFile? LinkImg { get; set; }
    }
}
