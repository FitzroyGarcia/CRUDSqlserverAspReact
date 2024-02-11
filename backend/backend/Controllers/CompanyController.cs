using AutoMapper;
using backend.Core.Context;
using backend.Core.Dtos.Company;
using backend.Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private ApplicationDbContext _context { get; }

        private IMapper _mapper { get; }
        public CompanyController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        //CRUD

        //Create
        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> CreateCompany([FromBody] CompanyCreateDto dto)
        {
            var newCompany = _mapper.Map<Company>(dto);
            await _context.Companies.AddAsync(newCompany);
            await _context.SaveChangesAsync();

            return Ok("Company Created Successfully");
        }

        //Read
        [HttpGet]
        [Route("Get")]
        public async Task<ActionResult<IEnumerable<CompanyGetDto>>> GetCompanies()
        {
            //var companies = await _context.Companies.ToListAsync();
            var companies = await _context.Companies.OrderByDescending(q => q.CreatedAt).ToListAsync();
            var convertedCompanies = _mapper.Map<IEnumerable<CompanyGetDto>>(companies);

            return Ok(convertedCompanies);
        }

        //Read Company by ID
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<Company>> GetCompanyByID([FromRoute] long id)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(q => q.ID == id);

            if (company is null)
            {
                return NotFound("Company Not Found");
            }

            return Ok(company);
        }


        //Update
        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateCompany([FromRoute] long id, [FromBody] CompanyUpdateDto dto)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(q => q.ID == id);

            if (company is null)
            {
                return NotFound("Company Not Found");
            }

            company.Name = dto.Name;
            company.Size = dto.Size;
            company.IsActive = dto.IsActive;
            company.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok("Company Updated Successfully");
        }

        //Detele
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteCompany([FromRoute] long id) 
        {
            var company = await _context.Companies.FirstOrDefaultAsync(q => q.ID == id);

            if (company is null) 
            {
                return NotFound("Company Not Found");
            }

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();

            return Ok("Company Deleted Successfully");
        }
    }
}
