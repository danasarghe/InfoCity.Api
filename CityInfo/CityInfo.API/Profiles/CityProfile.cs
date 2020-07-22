using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Profiles
{
    public class CityProfile :Profile
    {
        public CityProfile()
        {
            CreateMap<Entities.City, Model.CityWithoutPointOfInterestDto>();
            CreateMap<Entities.City, Model.CityDto>();
        }
    }
}
