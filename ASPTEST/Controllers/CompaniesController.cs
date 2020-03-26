using ASPTEST.DtoParameters;
using ASPTEST.Entities;
using ASPTEST.Models;
using ASPTEST.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASPTEST.Controllers
{
    [ApiController]
    [Route("api/companies")]
    public class CompaniesController: ControllerBase
    {
        private readonly ICompanyRepository companyRepository;
        private readonly IMapper mapper;

        public CompaniesController(ICompanyRepository companyRepository, IMapper mapper)
        {
            this.companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper)); ;
        }

        [HttpGet]
        // 1、默认情况会生成Task<IActionResult>的返回类型，这是一个泛型，最好将返回类型确定，这样可以帮助更好的阅读代码
        // 2、默认使用HttpGet方法，并且直接使用class的Route，.net core会对参数进行自动推断，以确定使用什么样的方式获取参数（[FromBody] / [FromQuery] / ...）
        //    这里使用一个参数q，通常是一个查询参数，将参数传入，根据是否为空，进行不同数据结果的返回。
        // 3、使用mapper（AutoMapper）类，可以映射数据库模型和用户使用模型之间的关系，具体实现在Profiles中，同时注册了服务，即可使用。
        //public async Task<ActionResult<IEnumerable<CompanyDto>>> GetCompanies(string q)
        //{
        //    var companies = await companyRepository.GetCompaniesAsync(q);

        //    var companyDtos = mapper.Map<IEnumerable<CompanyDto>>(companies);

        //    return Ok(companyDtos);
        //}


        // 更新参数的使用，使用一个参数类来传参。同时，因为这里传入的是一个复杂的类，和上面的单独传入的q有区别，系统会默认认为这应该是从[FromBody]传入的，
        // 会导致查询条件出错并且body中找不到相应参数。这时，需要显式的给出[FromQuery]告诉系统这时一个查询参数即可。
        public async Task<ActionResult<IEnumerable<CompanyDto>>> GetCompanies([FromQuery]CompanyDtoParameters companyDtoParameters)
        {
            var companies = await companyRepository.GetCompaniesAsync(companyDtoParameters);

            var companyDtos = mapper.Map<IEnumerable<CompanyDto>>(companies);

            return Ok(companyDtos);
        }

        [HttpGet("{companyId}", Name = nameof(GetCompany))]
        // 使用缓存，时长120秒
        [ResponseCache(Duration = 120)]
        public async Task<ActionResult<CompanyDto>> GetCompany(Guid companyId)
        {
            var company = await companyRepository.GetCompanyAsync(companyId);
            if (company == null)
            {
                return NotFound();
            }
            return Ok(mapper.Map<CompanyDto>(company));
        }

        [HttpPost]
        // 在Post中，参数通常都是FromBody的，所以可以省略不写，也可以显式的给出。
        // 同时，.net core框架也给我们本身提供了检验机制，在使用参数前，它已经检查了参数的有效性，如果为null，它会直接返回400，不会执行内部代码，所以不需要再手动判断参数是否为空。
        // 通常情况下，创建、更新、查询的Dto不一样，所以最好的方案就是为每一个操作创建一个对应的Dto，这样便于以后的维护和扩展。
        public async Task<ActionResult<CompanyDto>> CreateCompany([FromBody]CompanyAddDto company)
        {
            var entity = mapper.Map<Company>(company);
            companyRepository.AddCompany(entity);
            // AddCompany操作之后，系统其实并没有将数据插入到数据库，需要手动调用Save方法，这里调用我们封装的Save异步方法保存数据。
            await companyRepository.SaveAsync();

            // 保存之后，将数据以Dto模型返回给前端
            var returnDto = mapper.Map<CompanyDto>(entity);
            // 因为是创建，所以应该返回201状态码
            return CreatedAtRoute(nameof(GetCompany), new { companyId = returnDto.Id }, returnDto);
        }
    }
}
