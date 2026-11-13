using csharp_PDFHandler.Src.Application.Dto;
using csharp_PDFHandler.Src.Domain.Model;
using csharp_PDFHandler.Src.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace csharp_PDFHandler.Src.Presentation.Controllers
{
    /// <summary>
    /// REST API controller for PDF upload records.
    /// Provides CRUD operations and a dedicated endpoint to forward PDFs to Service B.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UploadsController(
        HandlerDbContext db,
        IHttpClientFactory httpClientFactory) : ControllerBase
    {
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        /// <summary>Returns all upload records.</summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UploadRecordDto>>> GetAll()
        {
            var records = await db.Uploads.ToListAsync();
            return Ok(records.Select(ToDto));
        }

        /// <summary>Returns a single upload record by ID, or 404 if not found.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UploadRecordDto>> GetById(int id)
        {
            var record = await db.Uploads.FindAsync(id);
            return record is null ? NotFound() : Ok(ToDto(record));
        }

        /// <summary>Creates a new upload record and returns 201 with location header.</summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<UploadRecordDto>> Create(CreateUploadDto request)
        {
            var record = new UploadRecord { FileName = request.FileName, FileSize = request.FileSize };
            db.Uploads.Add(record);
            await db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = record.Id }, ToDto(record));
        }

        /// <summary>Updates an existing upload record's filename and size. Returns 404 if not found.</summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, CreateUploadDto request)
        {
            var record = await db.Uploads.FindAsync(id);
            if (record is null) return NotFound();
            record.FileName = request.FileName;
            record.FileSize = request.FileSize;
            await db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Deletes an upload record by ID, or 404 if not found.</summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var record = await db.Uploads.FindAsync(id);
            if (record is null) return NotFound();
            db.Uploads.Remove(record);
            await db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Accepts a PDF file, validates it, saves an upload record,
        /// and forwards the file to Processing Service (B) via HTTP POST.
        /// </summary>
        [HttpPost("upload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            // [ApiController] + non-nullable IFormFile already rejects missing files at model binding.
            // These checks are defensive validation in case the attribute behavior changes.
            if (file is null || file.Length == 0)
                return BadRequest("No file uploaded.");

            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only PDF files are accepted.");

            if (file.Length > MaxFileSize)
                return BadRequest($"File exceeds maximum size of {MaxFileSize / 1024 / 1024}MB.");

            var record = new UploadRecord
            {
                FileName = file.FileName,
                FileSize = file.Length,
                Status = UploadStatus.Received
            };
            db.Uploads.Add(record);
            await db.SaveChangesAsync();

            try
            {
                // Forward PDF to Processing Service (B)
                using var content = new MultipartFormDataContent();
                using var stream = file.OpenReadStream();
                content.Add(new StreamContent(stream), "file", file.FileName);

                var client = httpClientFactory.CreateClient("Processing");
                var response = await client.PostAsync(
                    "api/processing/process", content);

                record.Status = UploadStatus.Forwarded;
                await db.SaveChangesAsync();

                var result = await response.Content.ReadFromJsonAsync<object>();
                return Ok(new { uploadId = record.Id, status = UploadStatus.Forwarded.ToString(), processingResult = result });
            }
            catch (Exception ex)
            {
                record.Status = UploadStatus.Failed;
                await db.SaveChangesAsync();
                return StatusCode(500, new { uploadId = record.Id, error = ex.Message });
            }
        }

        // Maps entity to response DTO
        private static UploadRecordDto ToDto(UploadRecord r) =>
            new(r.Id, r.FileName, r.FileSize, r.Status, r.UploadedAt);
    }
}
