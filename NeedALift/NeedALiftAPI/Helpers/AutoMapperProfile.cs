using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NeedALiftAPI.Models;

namespace NeedALiftAPI.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Users, UsersDTO>();
            CreateMap<UsersDTO, Users>();
        }
    }
}
