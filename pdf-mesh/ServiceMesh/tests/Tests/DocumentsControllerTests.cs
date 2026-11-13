using csharp_Storage.Src.Application.Dto;
using csharp_Storage.Src.Infrastructure.Data;
using csharp_Storage.Src.Presentation.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Tests
{
    /// <summary>
    /// Unit tests for DocumentsController (Service C).
    /// Uses EF Core InMemory database to isolate tests from real SQLite.
    /// </summary>
    public class DocumentsControllerTests
    {
        private static StorageDbContext CreateDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<StorageDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new StorageDbContext(options);
        }

        // Verify POST /api/documents creates a new document and returns 201 with correct DTO
        [Fact]
        public async Task Create_ReturnsCreatedResult_WithCorrectData()
        {
            // Arrange
            using var db = CreateDbContext(nameof(Create_ReturnsCreatedResult_WithCorrectData));
            var controller = new DocumentsController(db);
            var dto = new CreateDocumentDto("test.pdf", "{\"pages\":[]}");

            // Act
            var result = await controller.Create(dto);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returned = Assert.IsType<PdfDocumentDto>(created.Value);
            Assert.Equal("test.pdf", returned.FileName);
            Assert.Equal("{\"pages\":[]}", returned.JsonContent);
            Assert.Equal(1, returned.Id);
        }

        // Verify GET /api/documents/{id} returns 404 when the ID does not exist in the database
        [Fact]
        public async Task GetById_ReturnsNotFound_WhenDocumentDoesNotExist()
        {
            // Arrange
            using var db = CreateDbContext(nameof(GetById_ReturnsNotFound_WhenDocumentDoesNotExist));
            var controller = new DocumentsController(db);

            // Act
            var result = await controller.GetById(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        // Verify GET /api/documents/{id} returns 200 with matching data when the document exists
        [Fact]
        public async Task GetById_ReturnsDocument_WhenExists()
        {
            // Arrange
            using var db = CreateDbContext(nameof(GetById_ReturnsDocument_WhenExists));
            var controller = new DocumentsController(db);
            var createDto = new CreateDocumentDto("found.pdf", "{\"text\":\"hello\"}");
            await controller.Create(createDto);

            // Act
            var result = await controller.GetById(1);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var doc = Assert.IsType<PdfDocumentDto>(ok.Value);
            Assert.Equal("found.pdf", doc.FileName);
        }

        // Verify DELETE /api/documents/{id} removes the record and returns 204,
        // then confirm re-fetching the same ID returns 404
        [Fact]
        public async Task Delete_RemovesDocument_AndReturnsNoContent()
        {
            // Arrange
            using var db = CreateDbContext(nameof(Delete_RemovesDocument_AndReturnsNoContent));
            var controller = new DocumentsController(db);
            await controller.Create(new CreateDocumentDto("delete-me.pdf", "{}"));

            // Act
            var deleteResult = await controller.Delete(1);

            // Assert
            Assert.IsType<NoContentResult>(deleteResult);

            var getResult = await controller.GetById(1);
            Assert.IsType<NotFoundResult>(getResult.Result);
        }
    }
}
