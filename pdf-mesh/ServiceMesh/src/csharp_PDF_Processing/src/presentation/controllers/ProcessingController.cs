using csharp_PDF_Processing.Src.Application.Dto;
using csharp_PDF_Processing.Src.Domain.Model;
using csharp_PDF_Processing.Src.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UglyToad.PdfPig;
using System.Text.Json;

namespace csharp_PDF_Processing.Src.Presentation.Controllers
{
    /// <summary>
    /// REST API controller for PDF processing jobs.
    /// Provides CRUD operations on processing jobs and a dedicated endpoint
    /// to extract text from a PDF and forward the result to Storage Service (C).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProcessingController(
        ProcessingDbContext db,
        IHttpClientFactory httpClientFactory) : ControllerBase
    {
        /// <summary>Returns all processing jobs.</summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProcessingJobDto>>> GetAll()
        {
            var jobs = await db.Jobs.ToListAsync();
            return Ok(jobs.Select(ToDto));
        }

        /// <summary>Returns a single job by ID, or 404 if not found.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProcessingJobDto>> GetById(int id)
        {
            var job = await db.Jobs.FindAsync(id);
            return job is null ? NotFound() : Ok(ToDto(job));
        }

        /// <summary>Creates a new processing job record and returns 201 with location header.</summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<ProcessingJobDto>> Create(CreateJobDto request)
        {
            var job = new ProcessingJob { FileName = request.FileName };
            db.Jobs.Add(job);
            await db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = job.Id }, ToDto(job));
        }

        /// <summary>Updates an existing job's filename. Returns 404 if not found.</summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, CreateJobDto request)
        {
            var job = await db.Jobs.FindAsync(id);
            if (job is null) return NotFound();
            job.FileName = request.FileName;
            await db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Deletes a job by ID, or 404 if not found.</summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var job = await db.Jobs.FindAsync(id);
            if (job is null) return NotFound();
            db.Jobs.Remove(job);
            await db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Accepts a PDF file, extracts text using PdfPig, converts to JSON,
        /// and forwards the result to Storage Service (C) via HTTP POST.
        /// </summary>
        [HttpPost("process")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProcessPdf(IFormFile file)
        {
            // [ApiController] + non-nullable IFormFile already rejects missing files at model binding.
            // These checks are defensive validation in case the attribute behavior changes.
            if (file is null || file.Length == 0)
                return BadRequest("No file uploaded.");

            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only PDF files are accepted.");

            var job = new ProcessingJob
            {
                FileName = file.FileName,
                Status = JobStatus.Processing
            };
            db.Jobs.Add(job);
            await db.SaveChangesAsync();

            try
            {
                // Extract text from PDF using PdfPig
                using var stream = file.OpenReadStream();
                using var pdf = PdfDocument.Open(stream);
                var pages = pdf.GetPages()
                    .Select(p => new { page = p.Number, text = p.Text })
                    .ToList();

                var jsonContent = JsonSerializer.Serialize(new
                {
                    fileName = file.FileName,
                    pageCount = pdf.NumberOfPages,
                    pages
                });

                // Forward extracted JSON to Storage Service (C)
                var storagePayload = new
                {
                    fileName = file.FileName,
                    jsonContent
                };
                var client = httpClientFactory.CreateClient("Storage");
                var response = await client.PostAsJsonAsync(
                    "api/documents", storagePayload);
                response.EnsureSuccessStatusCode();

                job.Status = JobStatus.Completed;
                await db.SaveChangesAsync();

                var stored = await response.Content.ReadFromJsonAsync<object>();
                return Ok(new { jobId = job.Id, status = JobStatus.Completed.ToString(), storedDocument = stored });
            }
            catch (Exception ex)
            {
                job.Status = JobStatus.Failed;
                await db.SaveChangesAsync();
                return StatusCode(500, new { jobId = job.Id, error = ex.Message });
            }
        }

        // Maps entity to response DTO
        private static ProcessingJobDto ToDto(ProcessingJob job) =>
            new(job.Id, job.FileName, job.Status, job.CreatedAt);
    }
}
