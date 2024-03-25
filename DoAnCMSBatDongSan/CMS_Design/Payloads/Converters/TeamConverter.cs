using CMS_Design.Entities;
using CMS_Design.Payloads.DTOs.DataResponseTeam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Design.Payloads.Converters
{
    public class TeamConverter
    {
        private readonly UserConverter _userConverter;
        public TeamConverter()
        {
            _userConverter = new UserConverter();
        }
        public TeamDTO EntityToDTO(Team team)
        {
            return new TeamDTO
            {
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Description = team.Description,
                Member = team.Users.Count(),
                Name = team.Name,
                Id = team.Id,
                Sologan = team.Sologan,
                StatusId = team.StatusId,
                Code = team.Code,
                TruongPhongId = team.TruongPhongId,
                Users = team.Users.Select(x => _userConverter.EntityToDTO(x))
            };
        }
    }
}
