using AutoMapper;
using backend.Core.Context;
using backend.Core.Dtos.Company;
using backend.Core.Dtos.Job;
using backend.Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private ApplicationDbContext _context { get; }

        private IMapper _mapper { get; }
        public JobController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        //CRUD

        //Create
        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> CreateJob([FromBody] JobCreateDto dto) 
        {
            var newJob = _mapper.Map<Job>(dto);
            await _context.Jobs.AddAsync(newJob);
            await _context.SaveChangesAsync();

            return Ok("Job Created Successfully");
        }

        //Read
        [HttpGet]
        [Route("Get")]
        public async Task<ActionResult<IEnumerable<JobGetDto>>> GetJob()
        {
            //var jobs = await _context.Jobs.ToListAsync();
            var jobs = await _context.Jobs.Include(job=>job.Company).OrderByDescending(q=>q.CreatedAt).ToListAsync();
            var convertedJobs = _mapper.Map<IEnumerable<JobGetDto>>(jobs);

            return Ok(convertedJobs);
        }

        //Read Company by ID
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<Job>> GetJobByID([FromRoute] long id)
        {
            var job = await _context.Jobs.FirstOrDefaultAsync(q => q.ID == id);

            if (job is null)
            {
                return NotFound("Company Not Found");
            }

            return Ok(job);
        }


        //Update

        //Detele
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteJob([FromRoute] long id)
        {
            var job = await _context.Jobs.FirstOrDefaultAsync(q => q.ID == id);

            if (job is null)
            {
                return NotFound("Job Not Found");
            }

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();

            return Ok("Job Deleted Successfully");
        }
    }
}
