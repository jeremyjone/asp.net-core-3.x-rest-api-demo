using ASPTEST.Entities;
using ASPTEST.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASPTEST.Profiles
{
    public class CompanyProfile: Profile
    {
        public CompanyProfile()
        {
            // 创建映射关系
            CreateMap<Company, CompanyDto>()
                .ForMember(
                    dest => dest.CompanyName,
                    opt => opt.MapFrom(src => src.Name));

            CreateMap<CompanyAddDto, Company>();
        }
    }
}
