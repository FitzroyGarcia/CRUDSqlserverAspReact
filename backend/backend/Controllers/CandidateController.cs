using AutoMapper;
using backend.Core.Context;
using backend.Core.Dtos.Candidate;
using backend.Core.Dtos.Job;
using backend.Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateController : ControllerBase
    {
        private ApplicationDbContext _context { get; }

        private IMapper _mapper { get; }
        public CandidateController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        //CRUD

        //Create
        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> CreateCandidate([FromForm] CandidateCreateDto dto, IFormFile pdfFile)
        {
            //primero guardamos pdf en el servidor
            //Despues guardamos url en la entidad
            var fiveMegaByte = 5 * 1024 * 1024;
            var pdfMineType = "application/pdf";
            if (pdfFile.Length > fiveMegaByte || pdfFile.ContentType != pdfMineType) 
            {
                return BadRequest("File is not Valid");
            }

            var resumeUrl = Guid.NewGuid().ToString()+".pdf";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "documents", "pdfs", resumeUrl);

            using (var stream = new FileStream(filePath, FileMode.Create)) 
            {
                await pdfFile.CopyToAsync(stream);
            }

            var newCandidate = _mapper.Map<Candidate>(dto);
            newCandidate.ResumeUrl = resumeUrl;
            await _context.Candidates.AddAsync(newCandidate);
            await _context.SaveChangesAsync();

            return Ok("Candidate Created Successfully");
        }

        //Read
        [HttpGet]
        [Route("Get")]
        public async Task<ActionResult<IEnumerable<CandidateGetDto>>> GetCandidates()
        {
            //var jobs = await _context.Jobs.ToListAsync();
            var candidates = await _context.Candidates.Include(candidate => candidate.Job).OrderByDescending(q => q.CreatedAt).ToListAsync();
            var convertedCandidates = _mapper.Map<IEnumerable<CandidateGetDto>>(candidates);

            return Ok(convertedCandidates);
        }

        //Read (Download PDF)
        [HttpGet]
        [Route("download/{url}")]
        public IActionResult DownloadPdfFile(string url) 
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Documents", "pdfs", url);

            if (!System.IO.File.Exists(filePath)) 
            {
                return NotFound("File Not Fount");
            }

            var pdfBytes = System.IO.File.ReadAllBytes(filePath);
            var file = File(pdfBytes, "application/pdf", url);
            return file;
        }

        //Read Company by ID
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<Candidate>> GetCandidateByID([FromRoute] long id)
        {
            var candidate = await _context.Candidates.FirstOrDefaultAsync(q => q.ID == id);

            if (candidate is null)
            {
                return NotFound("Company Not Found");
            }

            return Ok(candidate);
        }

        //Update



        //Delete
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteCandidate([FromRoute] long id)
        {
            var candidate = await _context.Candidates.FirstOrDefaultAsync(q => q.ID == id);

            if (candidate is null)
            {
                return NotFound("Job Not Found");
            }

            var pdfFileName = candidate.ResumeUrl;

            _context.Candidates.Remove(candidate);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(pdfFileName))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Documents", "pdfs", pdfFileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            return Ok("Candidate Deleted Successfully");
        }
    }
}
