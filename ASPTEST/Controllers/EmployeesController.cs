using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASPTEST.Entities;
using ASPTEST.Models;
using ASPTEST.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace ASPTEST.Controllers
{
    [ApiController]
    [Route("api/companies/{companyId}/employees")]
    public class EmployeesController : ControllerBase
    {
        private readonly ICompanyRepository companyRepository;
        private readonly IMapper mapper;

        public EmployeesController(ICompanyRepository companyRepository, IMapper mapper)
        {
            this.companyRepository = companyRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployeesForCompany(Guid companyId)
        {
            if (! await companyRepository.CompanyExistsAsync(companyId))
            {
                return NotFound();
            }

            var employees = await companyRepository.GetEmployeesAsync(companyId);

            var employeeDtos = mapper.Map<IEnumerable<EmployeeDto>>(employees);

            return Ok(employeeDtos);
        }

        [HttpGet("{employeeId}", Name = nameof(GetEmployeeForCompany))]
        public async Task<ActionResult<EmployeeDto>> GetEmployeeForCompany(Guid companyId, Guid employeeId)
        {
            if (!await companyRepository.CompanyExistsAsync(companyId))
            {
                return NotFound();
            }

            var employee = await companyRepository.GetEmployeeAsync(companyId, employeeId);
            if (employee == null)
            {
                return NotFound();
            }

            var employeeDto = mapper.Map<EmployeeDto>(employee);
            return Ok(employeeDto);
        }

        [HttpPost]
        public async Task<ActionResult<EmployeeDto>> CreateEmployeeForCompany(Guid companyId, [FromBody]EmployeeAddDto employee)
        {
            if (!await companyRepository.CompanyExistsAsync(companyId))
            {
                return NotFound();
            }

            var entity = mapper.Map<Employee>(employee);
            companyRepository.AddEmployee(companyId, entity);
            await companyRepository.SaveAsync();

            var returnDto = mapper.Map<EmployeeDto>(entity);
            return CreatedAtRoute(nameof(GetEmployeeForCompany), new { companyId, employeeId = entity.Id }, returnDto);
        }
    }
}