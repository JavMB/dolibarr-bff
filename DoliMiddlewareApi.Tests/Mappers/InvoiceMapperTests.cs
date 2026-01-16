using DoliMiddlewareApi.Dtos.Dolibarr;
using DoliMiddlewareApi.Mappers;

namespace DoliMiddlewareApi.Tests.Mappers;

public class InvoiceMapperTests
{
    // ARRANGE → Preparar datos de entrada
    // ACT → Ejecutar método bajo test
    // ASSERT → Verificar resultado

    [Fact]
    public void MapToInvoiceDto_WithValidResponse_ReturnsCorrectDto()
    {
        // ARRANGE: Creamos un objeto response simulado
        var response = new InvoiceResponse
        {
            id = "123",
            @ref = "FAC-2024-001",
            date = 1704067200, // 01/01/2024 en Unix timestamp
            date_lim_reglement = 1706745600, // 01/02/2024
            socid = "456",
            total_ttc = "1234.56",
            remaintopay = "500.00",
            statut = "1" // unpaid
        };

        // ACT: Ejecutamos el método bajo test
        var result = InvoiceMapper.MapToInvoiceDto(response);

        // ASSERT: Verificamos el resultado
        Assert.Equal(123, result.Id);
        Assert.Equal("FAC-2024-001", result.Number);
        Assert.Equal(new DateTime(2024, 1, 1), result.Date);
        Assert.Equal(new DateTime(2024, 2, 1), result.ExpireDate);
        Assert.Equal(456, result.ClientId);
        Assert.Equal(1234.56m, result.Total);
        Assert.Equal(500.00m, result.RemainToPay);
        Assert.Equal("unpaid", result.Status);
    }

    [Fact]
    public void MapToInvoiceDto_WithNullValues_HandlesGracefully()
    {
        // ARRANGE: Probamos edge cases (casos límite)
        var response = new InvoiceResponse
        {
            id = "invalid",
            @ref = null,
            date = null,
            date_lim_reglement = null,
            socid = "not-a-number",
            total_ttc = "not-a-decimal",
            remaintopay = "",
            statut = "999" // status desconocido
        };

        // ACT
        var result = InvoiceMapper.MapToInvoiceDto(response);

        // ASSERT: Verificamos que se maneja gracefulmente
        Assert.Equal(0, result.Id); // fallback para parseo fallido
        Assert.Equal("SIN-REF", result.Number); // valor por defecto
        Assert.Null(result.Date); // null si no hay timestamp
        Assert.Null(result.ExpireDate);
        Assert.Equal(0, result.ClientId);
        Assert.Null(result.Total); // null si parseo falla
        Assert.Null(result.RemainToPay);
        Assert.Equal("unknown", result.Status); // status desconocido
    }

    [Theory]
    [InlineData("0", "draft")]
    [InlineData("1", "unpaid")]
    [InlineData("2", "paid")]
    [InlineData("3", "cancelled")]
    [InlineData("999", "unknown")]
    public void ConvertStatusToWord_WithDifferentStatuses_ReturnsCorrectWord(
        string statusCode,
        string expectedWord)
    {
        // ARRANGE
        var response = new InvoiceResponse
        {
            id = "1",
            statut = statusCode
        };

        // ACT
        var result = InvoiceMapper.MapToInvoiceDto(response);

        // ASSERT
        Assert.Equal(expectedWord, result.Status);
    }

    [Fact]
    public void MapToInvoiceDetailDto_WithLines_ReturnsDtoWithLines()
    {
        // ARRANGE: Creamos una respuesta compleja con líneas
        var response = new InvoiceDetailResponse
        {
            id = "1",
            @ref = "FAC-001",
            date = 1704067200,
            socid = "10",
            total_ttc = "100.00",
            remaintopay = "0.00",
            statut = "2", // paid
            Lines = new List<InvoiceLineResponse>
            {
                new InvoiceLineResponse
                {
                    id = "1",
                    description = "Product A",
                    qty = "2",
                    subprice = "50.00",
                    tva_tx = "21.00",
                    total_ttc = "121.00"
                }
            }
        };

        // ACT
        var result = InvoiceMapper.MapToInvoiceDetailDto(response);

        // ASSERT
        Assert.NotEmpty(result.Lines);
        Assert.Single(result.Lines);
        Assert.Equal("Product A", result.Lines[0].Description);
        Assert.Equal(2, result.Lines[0].Quantity);
        Assert.Equal(50.00m, result.Lines[0].UnitPrice);
        Assert.Equal(21.00m, result.Lines[0].TaxRate);
        Assert.Equal(121.00m, result.Lines[0].Total);
    }

    [Fact]
    public void MapToInvoiceLineDto_WithValidLine_ReturnsCorrectDto()
    {
        // ARRANGE
        var lineResponse = new InvoiceLineResponse
        {
            id = "42",
            desc = "Fallback description",
            description = "Main description",
            qty = "3",
            subprice = "99.99",
            tva_tx = "10.00",
            total_ttc = "329.97"
        };

        // ACT
        var result = InvoiceMapper.MapToInvoiceLineDto(lineResponse);

        // ASSERT
        Assert.Equal(42, result.Id);
        Assert.Equal("Main description", result.Description); // priority: description > desc
        Assert.Equal(3, result.Quantity);
        Assert.Equal(99.99m, result.UnitPrice);
        Assert.Equal(10.00m, result.TaxRate);
        Assert.Equal(329.97m, result.Total);
    }
}
