using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using DoliMiddlewareApi.Dtos.Dolibarr;
using DoliMiddlewareApi.Exceptions;
using DoliMiddlewareApi.Services;
using DoliMiddlewareApi.Services.Clients;
using Moq;
using Moq.Protected;

namespace DoliMiddlewareApi.Tests.Services;

public class InvoiceServiceTests
{
    private readonly Mock<IDolibarrApiClient> _mockApiClient;
    private readonly InvoiceService _invoiceService;

    public InvoiceServiceTests()
    {
        // ARRANGE: Crear mock del cliente API
        _mockApiClient = new Mock<IDolibarrApiClient>();
        _invoiceService = new InvoiceService(_mockApiClient.Object);
    }

    // ===== TESTS POSITIVOS =====

    [Fact]
    public async Task GetInvoiceAsync_WithValidId_ReturnsInvoiceDetail()
    {
        // ARRANGE: Preparamos el mock para que devuelva datos
        var expectedResponse = new InvoiceDetailResponse
        {
            id = "1",
            @ref = "FAC-001",
            date = 1704067200,
            socid = "10",
            total_ttc = "100.00",
            remaintopay = "0.00",
            statut = "2"
        };

        _mockApiClient
            .Setup(client => client.GetResourceAsync<InvoiceDetailResponse>("invoices/1"))
            .ReturnsAsync(expectedResponse);

        // ACT: Ejecutamos el método bajo test
        var result = await _invoiceService.GetInvoiceAsync(1);

        // ASSERT: Verificamos que el mock fue llamado con el endpoint correcto
        _mockApiClient.Verify(
            client => client.GetResourceAsync<InvoiceDetailResponse>("invoices/1"),
            Times.Once
        );

        // Y que el resultado es el esperado
        Assert.Equal(1, result.Id);
        Assert.Equal("FAC-001", result.Number);
        Assert.Equal(100.00m, result.Total);
        Assert.Equal("paid", result.Status);
    }

    [Fact]
    public async Task GetInvoicesAsync_WithDefaultParams_ReturnsList()
    {
        // ARRANGE
        var expectedList = new List<InvoiceResponse>
        {
            new InvoiceResponse { id = "1", @ref = "FAC-001", statut = "0" },
            new InvoiceResponse { id = "2", @ref = "FAC-002", statut = "1" }
        };

        _mockApiClient
            .Setup(client => client.GetCollectionAsync<InvoiceResponse>("invoices?limit=50&page=0"))
            .ReturnsAsync(expectedList);

        // ACT
        var result = await _invoiceService.GetInvoicesAsync();

        // ASSERT
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal(2, result[1].Id);
    }

    [Fact]
    public async Task GetInvoicesAsync_WithParams_ReturnsCorrectList()
    {
        // ARRANGE
        var expectedList = new List<InvoiceResponse>
        {
            new InvoiceResponse { id = "3", @ref = "FAC-003", statut = "2" }
        };

        _mockApiClient
            .Setup(client => client.GetCollectionAsync<InvoiceResponse>("invoices?limit=10&page=1&status=2"))
            .ReturnsAsync(expectedList);

        // ACT: Pasamos página 2 (que se convierte a page=1 en el endpoint)
        var result = await _invoiceService.GetInvoicesAsync(limit: 10, page: 2, status: "2");

        // ASSERT
        Assert.Single(result);
        Assert.Equal(3, result[0].Id);
    }

    [Fact]
    public async Task CreateInvoiceAsync_WithValidDto_ReturnsNewId()
    {
        // ARRANGE
        var dto = new Dtos.command.CreateInvoiceDto
        {
            ClientId = 10,
            Date = DateTime.Now,
            Status = "unpaid",
            Reference = "TEST-001",
            Lines = new List<Dtos.command.CreateInvoiceLineDto>
            {
                new Dtos.command.CreateInvoiceLineDto
                {
                    Description = "Test Product",
                    Quantity = 2,
                    UnitPrice = 50.00m,
                    TaxRate = 21.00m
                }
            }
        };

        _mockApiClient
            .Setup(client => client.PostAsync("invoices", It.IsAny<object>()))
            .ReturnsAsync("42");

        // ACT
        var result = await _invoiceService.CreateInvoiceAsync(dto);

        // ASSERT
        Assert.Equal(42, result);
        _mockApiClient.Verify(client => client.PostAsync("invoices", It.IsAny<object>()), Times.Once);
    }

    // ===== TESTS DE EXCEPCIONES =====

    [Fact]
    public async Task GetInvoiceAsync_WhenNotFound_ThrowsNotFoundException()
    {
        // ARRANGE: Simulamos que el API client lanza NotFoundException
        _mockApiClient
            .Setup(client => client.GetResourceAsync<InvoiceDetailResponse>("invoices/999"))
            .ThrowsAsync(new NotFoundException("Invoice not found"));

        // ACT + ASSERT: Verificamos que se propaga la excepción
        await Assert.ThrowsAsync<NotFoundException>(
            async () => await _invoiceService.GetInvoiceAsync(999)
        );
    }

    [Fact]
    public async Task AddInvoiceLineAsync_WhenInvoiceNotDraft_ThrowsForbiddenException()
    {
        // ARRANGE: Invoice no está en borrador
        var invoiceResponse = new InvoiceDetailResponse
        {
            id = "1",
            statut = "2" // paid, no draft (0)
        };

        _mockApiClient
            .Setup(client => client.GetResourceAsync<InvoiceDetailResponse>("invoices/1"))
            .ReturnsAsync(invoiceResponse);

        var lineDto = new Dtos.command.CreateInvoiceLineDto
        {
            Description = "Test",
            Quantity = 1,
            UnitPrice = 10.00m,
            TaxRate = 21.00m
        };

        // ACT + ASSERT
        await Assert.ThrowsAsync<ForbiddenException>(
            async () => await _invoiceService.AddInvoiceLineAsync(1, lineDto)
        );
    }

    [Fact]
    public async Task UpdateInvoiceAsync_WhenInvoiceNotDraft_ThrowsForbiddenException()
    {
        // ARRANGE
        var currentInvoice = new InvoiceDetailResponse
        {
            id = "1",
            statut = "1" // unpaid, no draft (0)
        };

        _mockApiClient
            .Setup(client => client.GetResourceAsync<InvoiceDetailResponse>("invoices/1"))
            .ReturnsAsync(currentInvoice);

        var updateDto = new Dtos.command.UpdateInvoiceDto
        {
            Number = "NEW-REF"
        };

        // ACT + ASSERT
        await Assert.ThrowsAsync<ForbiddenException>(
            async () => await _invoiceService.UpdateInvoiceAsync(1, updateDto)
        );
    }

    [Fact]
    public async Task UpdateInvoiceAsync_WithValidDraft_UpdatesCorrectly()
    {
        // ARRANGE
        var currentInvoice = new InvoiceDetailResponse
        {
            id = "1",
            @ref = "OLD-REF",
            statut = "0", // draft
            note_public = null,
            note_private = null
        };

        _mockApiClient
            .Setup(client => client.GetResourceAsync<InvoiceDetailResponse>("invoices/1"))
            .ReturnsAsync(currentInvoice);

        _mockApiClient
            .Setup(client => client.PutAsync("invoices/1", It.IsAny<object>()))
            .ReturnsAsync("OK");

        var updateDto = new Dtos.command.UpdateInvoiceDto
        {
            Number = "NEW-REF",
            NotePublic = "Public note",
            Status = "unpaid" // Cambia a status "1"
        };

        // ACT
        await _invoiceService.UpdateInvoiceAsync(1, updateDto);

        // ASSERT
        _mockApiClient.Verify(client => client.PutAsync("invoices/1", It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task AddInvoiceLineAsync_WithValidDraft_AddsLineSuccessfully()
    {
        // ARRANGE
        var invoiceResponse = new InvoiceDetailResponse
        {
            id = "1",
            statut = "0" // draft
        };

        _mockApiClient
            .Setup(client => client.GetResourceAsync<InvoiceDetailResponse>("invoices/1"))
            .ReturnsAsync(invoiceResponse);

        _mockApiClient
            .Setup(client => client.PostAsync("invoices/1/lines", It.IsAny<object>()))
            .ReturnsAsync("new-line-id");

        var lineDto = new Dtos.command.CreateInvoiceLineDto
        {
            Description = "Test Product",
            Quantity = 3,
            UnitPrice = 25.00m,
            TaxRate = 10.00m
        };

        // ACT
        var result = await _invoiceService.AddInvoiceLineAsync(1, lineDto);

        // ASSERT
        Assert.Equal("new-line-id", result);
        _mockApiClient.Verify(client => client.PostAsync("invoices/1/lines", It.IsAny<object>()), Times.Once);
    }
}
