using csharp_Storage.Src.Domain.Model;
using csharp_Storage.Src.Infrastructure.Data;
using csharp_Storage.Src.Application.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace csharp_Storage.Src.Presentation.Controllers
{
    /// <summary>
    /// REST API controller for managing stored PDF documents.
    /// Provides full CRUD operations on the Documents table.
    /// Called by Service B (PDF_Processing) to persist extracted JSON.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController(StorageDbContext db) : ControllerBase
    {
        /// <summary>Returns all stored documents.</summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PdfDocumentDto>>> GetAll()
        {
            var docs = await db.Documents.ToListAsync();
            return Ok(docs.Select(ToDto));
        }

        /// <summary>Returns a single document by ID, or 404 if not found.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PdfDocumentDto>> GetById(int id)
        {
            var doc = await db.Documents.FindAsync(id);
            return doc is null ? NotFound() : Ok(ToDto(doc));
        }

        /// <summary>Creates a new document from a DTO and returns 201 with location header.</summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<PdfDocumentDto>> Create(CreateDocumentDto request)
        {
            var doc = new PdfDocument
            {
                FileName = request.FileName,
                JsonContent = request.JsonContent
            };

            db.Documents.Add(doc);
            await db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = doc.Id }, ToDto(doc));
        }

        /// <summary>Updates an existing document's content. Returns 404 if not found.</summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, CreateDocumentDto request)
        {
            var doc = await db.Documents.FindAsync(id);
            if (doc is null) return NotFound();

            doc.FileName = request.FileName;
            doc.JsonContent = request.JsonContent;
            await db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Deletes a document by ID, or 404 if not found.</summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var doc = await db.Documents.FindAsync(id);
            if (doc is null) return NotFound();
            db.Documents.Remove(doc);
            await db.SaveChangesAsync();
            return NoContent();
        }

        // Maps entity to response DTO
        private static PdfDocumentDto ToDto(PdfDocument doc) =>
            new(doc.Id, doc.FileName, doc.JsonContent, doc.ProcessedAt);
    }
}
