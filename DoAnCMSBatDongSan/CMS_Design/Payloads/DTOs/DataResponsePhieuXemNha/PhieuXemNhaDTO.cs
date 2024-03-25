using CMS_Design.Entities;
using CMS_Design.Payloads.DTOs.DataResponseBase;
using CMS_Design.Payloads.DTOs.DataResponseProduct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Design.Payloads.DTOs.DataResponsePhieuXemNha
{
    public class PhieuXemNhaDTO : BaseDTO
    {
        public string CustumerName { get; set; }
        public string CustumerPhoneNumber { get; set; }
        public string CustumerId { get; set; }
        public string CustumerIdImg1 { get; set; }
        public string CustumerIdImg2 { get; set; }
        public string Desciption { get; set; }
        public int NhaId { get; set; }
        public bool BanThanhCong { get; set; }
        public DateTime CreateTime { get; set; }
        public ProductDTO Product { get; set; }
    }
}
