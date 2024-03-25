using CMS_Design.Payloads.DTOs.DataResponseBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Design.Payloads.DTOs.DataResponseUser
{
    public class UserDTO : BaseDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public int? RoleId { get; set; }
        public string RoleName { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}
