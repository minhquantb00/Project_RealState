using CMS_Design.Payloads.DTOs.DataResponseBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Design.Payloads.DTOs.DataResponseRole
{
    public class RoleDTO : BaseDTO
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
