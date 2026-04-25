using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Services;

public class FacturapiFacturacionFiscalService : IFacturacionFiscalService
{
    private const string FacturapiBaseUrl = "https://www.facturapi.io";
    private readonly HttpClient _httpClient;
    private readonly IDbContextFactory<CrmDbContext> _dbFactory;
    private readonly IAppConfigRepository _appConfigRepository;

    public FacturapiFacturacionFiscalService(HttpClient httpClient, IDbContextFactory<CrmDbContext> dbFactory, IAppConfigRepository appConfigRepository)
    {
        _httpClient = httpClient;
        _dbFactory = dbFactory;
        _appConfigRepository = appConfigRepository;
    }

    public async Task<FacturacionFiscalResult> EmitirFacturaAsync(Guid facturaId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var factura = await db.Facturas
            .Include(f => f.Cliente)
            .Include(f => f.Pedido)
            .Include(f => f.NotaEntrega)
            .Include(f => f.Detalles)
            .Include(f => f.NotasEntregaRelacionadas)
                .ThenInclude(r => r.NotaEntrega)
            .FirstOrDefaultAsync(f => f.Id == facturaId, cancellationToken);

        if (factura == null)
        {
            return new FacturacionFiscalResult { Message = "No se encontró la factura a emitir." };
        }

        if (factura.TipoComprobante != FacturaTipoComprobante.Ingreso)
        {
            return await MarcarErrorAsync(db, factura, null, "La emisión inicial solo soporta facturas de ingreso.", cancellationToken);
        }

        if (factura.Estatus == FacturaEstatus.Timbrado && !string.IsNullOrWhiteSpace(factura.UuidFiscal))
        {
            return new FacturacionFiscalResult
            {
                Success = true,
                Message = "La factura ya se encuentra timbrada.",
                ExternalDocumentId = factura.ExternalDocumentId,
                UuidFiscal = factura.UuidFiscal,
                SerieFiscal = factura.SerieFiscal,
                FolioFiscal = factura.FolioFiscal,
                XmlUrl = factura.XmlUrl,
                PdfUrl = factura.PdfUrl,
                FechaTimbrado = factura.FechaTimbrado
            };
        }

        var configuracion = await CargarConfiguracionAsync();
        if (!configuracion.Habilitado)
        {
            return await MarcarErrorAsync(db, factura, null, "La facturación integrada está deshabilitada para la empresa activa.", cancellationToken);
        }

        if (!string.Equals(configuracion.Proveedor, "Facturapi", StringComparison.OrdinalIgnoreCase))
        {
            return await MarcarErrorAsync(db, factura, null, $"El proveedor configurado '{configuracion.Proveedor}' aún no está soportado por la integración actual.", cancellationToken);
        }

        var apiKey = configuracion.ModoPruebas ? configuracion.ApiKeyPruebas : configuracion.ApiKeyProduccion;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return await MarcarErrorAsync(db, factura, null, "Falta configurar la API key del proveedor fiscal.", cancellationToken);
        }

        var validacion = ValidarFactura(factura);
        if (!string.IsNullOrWhiteSpace(validacion))
        {
            return await MarcarErrorAsync(db, factura, null, validacion, cancellationToken);
        }

        var payload = ConstruirPayload(factura);
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(FacturapiBaseUrl), "/v2/invoices"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiKey}:")));
        request.Content = JsonContent.Create(payload, options: new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var (errorCode, errorMessage) = ParseError(rawResponse, response.ReasonPhrase);
                return await MarcarErrorAsync(db, factura, errorCode, errorMessage, cancellationToken);
            }

            using var document = JsonDocument.Parse(rawResponse);
            var root = document.RootElement;

            factura.ExternalDocumentId = GetString(root, "id") ?? factura.ExternalDocumentId;
            factura.UuidFiscal = GetString(root, "uuid") ?? GetString(root, "uuid_fiscal") ?? factura.UuidFiscal;
            factura.SerieFiscal = GetString(root, "series") ?? GetString(root, "serie") ?? factura.SerieFiscal;
            factura.FolioFiscal = GetString(root, "folio_number") ?? GetString(root, "folio") ?? factura.FolioFiscal;
            factura.XmlUrl = GetString(root, "xml_url") ?? factura.XmlUrl;
            factura.PdfUrl = GetString(root, "pdf_url") ?? factura.PdfUrl;
            factura.Cancelable = GetBool(root, "cancellable") ?? GetBool(root, "cancelable") ?? factura.Cancelable;
            factura.CancellationStatus = GetString(root, "cancellation_status") ?? factura.CancellationStatus;
            factura.FechaTimbrado = GetDateTime(root, "stamp.date") ?? GetDateTime(root, "date") ?? DateTime.UtcNow;
            factura.Estatus = FacturaEstatus.Timbrado;
            factura.ErrorCode = null;
            factura.ErrorMessage = null;
            factura.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync(cancellationToken);

            return new FacturacionFiscalResult
            {
                Success = true,
                Message = "Factura emitida correctamente en el proveedor fiscal.",
                ExternalDocumentId = factura.ExternalDocumentId,
                UuidFiscal = factura.UuidFiscal,
                SerieFiscal = factura.SerieFiscal,
                FolioFiscal = factura.FolioFiscal,
                XmlUrl = factura.XmlUrl,
                PdfUrl = factura.PdfUrl,
                FechaTimbrado = factura.FechaTimbrado
            };
        }
        catch (Exception ex)
        {
            return await MarcarErrorAsync(db, factura, null, ex.InnerException?.Message ?? ex.Message, cancellationToken);
        }
    }

    private async Task<FacturacionConfiguracion> CargarConfiguracionAsync()
    {
        return new FacturacionConfiguracion
        {
            Habilitado = await ObtenerBoolAsync(ClavesConfiguracionFacturacion.Habilitado, false),
            Proveedor = await ObtenerTextoAsync(ClavesConfiguracionFacturacion.Proveedor, "Facturapi"),
            ModoPruebas = await ObtenerBoolAsync(ClavesConfiguracionFacturacion.ModoPruebas, true),
            ApiKeyPruebas = await ObtenerTextoAsync(ClavesConfiguracionFacturacion.ApiKeyPruebas, string.Empty),
            ApiKeyProduccion = await ObtenerTextoAsync(ClavesConfiguracionFacturacion.ApiKeyProduccion, string.Empty)
        };
    }

    private async Task<string> ObtenerTextoAsync(string clave, string valorDefault)
    {
        var valor = await _appConfigRepository.GetValueAsync(clave);
        return string.IsNullOrWhiteSpace(valor) ? valorDefault : valor.Trim();
    }

    private async Task<bool> ObtenerBoolAsync(string clave, bool valorDefault)
    {
        var valor = await _appConfigRepository.GetValueAsync(clave);
        return bool.TryParse(valor, out var resultado) ? resultado : valorDefault;
    }

    private static string? ValidarFactura(Factura factura)
    {
        if (factura.Cliente == null)
            return "La factura no tiene cliente asociado.";

        if (string.IsNullOrWhiteSpace(factura.Cliente.RazonSocial))
            return "El cliente no tiene razón social fiscal.";

        if (string.IsNullOrWhiteSpace(factura.Cliente.RfcCif))
            return "El cliente no tiene RFC configurado.";

        if (string.IsNullOrWhiteSpace(factura.Cliente.RegimenFiscalReceptor))
            return "El cliente no tiene régimen fiscal configurado.";

        if (string.IsNullOrWhiteSpace(factura.Cliente.DomicilioFiscalCp) && string.IsNullOrWhiteSpace(factura.Cliente.CodigoPostal))
            return "El cliente no tiene código postal fiscal configurado.";

        if (string.IsNullOrWhiteSpace(factura.FormaPagoSat))
            return "La factura no tiene forma de pago SAT.";

        if ((factura.Subtotal <= 0m && factura.Total <= 0m) || factura.Total < 0m)
            return "La factura debe tener importes válidos para emitirse.";

        return null;
    }

    private static object ConstruirPayload(Factura factura)
    {
        var notas = factura.NotasEntregaRelacionadas
            .Where(r => r.IsActive)
            .Select(r => r.NotaEntrega.NumeroNota)
            .Distinct()
            .ToList();
        var zip = factura.Cliente.DomicilioFiscalCp ?? factura.Cliente.CodigoPostal;
        var customer = new Dictionary<string, object?>
        {
            ["legal_name"] = factura.Cliente.RazonSocial,
            ["tax_id"] = factura.Cliente.RfcCif,
            ["tax_system"] = factura.Cliente.RegimenFiscalReceptor,
            ["email"] = factura.Cliente.EmailFacturacion ?? factura.Cliente.Email,
            ["address"] = new Dictionary<string, object?>
            {
                ["zip"] = zip,
                ["country"] = "MEX"
            }
        };

        if (!string.IsNullOrWhiteSpace(factura.UsoCfdi))
        {
            customer["use"] = factura.UsoCfdi;
        }

        var amount = factura.Subtotal > 0m ? factura.Subtotal : factura.Total;
        var description = factura.Pedido != null
            ? notas.Count > 1
                ? $"Notas {string.Join(", ", notas)} / Pedido {factura.Pedido.NumeroPedido}"
                : factura.NotaEntrega != null
                    ? $"Nota {factura.NotaEntrega.NumeroNota} / Pedido {factura.Pedido.NumeroPedido}"
                    : $"Pedido {factura.Pedido.NumeroPedido}"
            : string.IsNullOrWhiteSpace(factura.Observaciones)
                ? $"Factura {factura.FolioInterno}"
                : factura.Observaciones!;

        var items = factura.Detalles.Any()
            ? factura.Detalles
                .Where(d => d.IsActive)
                .Select(d => new Dictionary<string, object?>
                {
                    ["quantity"] = d.Cantidad,
                    ["discount"] = d.Descuento > 0 ? d.Descuento : null,
                    ["product"] = new Dictionary<string, object?>
                    {
                        ["description"] = d.Descripcion,
                        ["product_key"] = string.IsNullOrWhiteSpace(d.ClaveProductoServicioSat) ? "01010101" : d.ClaveProductoServicioSat,
                        ["price"] = d.ValorUnitario,
                        ["unit_key"] = string.IsNullOrWhiteSpace(d.ClaveUnidadSat) ? "H87" : d.ClaveUnidadSat,
                        ["unit_name"] = string.IsNullOrWhiteSpace(d.Unidad) ? "Pieza" : d.Unidad
                    }
                })
                .ToArray()
            : new[]
            {
                new Dictionary<string, object?>
                {
                    ["quantity"] = 1,
                    ["product"] = new Dictionary<string, object?>
                    {
                        ["description"] = description,
                        ["product_key"] = "01010101",
                        ["price"] = amount
                    }
                }
            };

        var payload = new Dictionary<string, object?>
        {
            ["customer"] = customer,
            ["items"] = items,
            ["payment_form"] = factura.FormaPagoSat,
            ["payment_method"] = factura.MetodoPagoSat,
            ["date"] = factura.FechaEmision
        };

        if (!string.IsNullOrWhiteSpace(factura.UsoCfdi))
        {
            payload["use"] = factura.UsoCfdi;
        }

        return payload;
    }

    private static (string? ErrorCode, string ErrorMessage) ParseError(string rawResponse, string? fallback)
    {
        if (string.IsNullOrWhiteSpace(rawResponse))
            return (null, fallback ?? "El proveedor fiscal devolvió un error sin detalle.");

        try
        {
            using var document = JsonDocument.Parse(rawResponse);
            var root = document.RootElement;
            var code = GetString(root, "code") ?? GetString(root, "error.code");
            var message = GetString(root, "message")
                ?? GetString(root, "error.message")
                ?? GetString(root, "details")
                ?? fallback
                ?? "El proveedor fiscal devolvió un error al emitir la factura.";
            return (code, message);
        }
        catch
        {
            return (null, rawResponse);
        }
    }

    private static async Task<FacturacionFiscalResult> MarcarErrorAsync(CrmDbContext db, Factura factura, string? errorCode, string errorMessage, CancellationToken cancellationToken)
    {
        factura.Estatus = FacturaEstatus.ErrorTimbrado;
        factura.ErrorCode = errorCode;
        factura.ErrorMessage = errorMessage;
        factura.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return new FacturacionFiscalResult
        {
            Success = false,
            Message = errorMessage,
            ErrorCode = errorCode
        };
    }

    private static string? GetString(JsonElement element, string path)
    {
        if (!TryGetElement(element, path, out var found))
            return null;

        return found.ValueKind switch
        {
            JsonValueKind.String => found.GetString(),
            JsonValueKind.Number => found.ToString(),
            JsonValueKind.True => bool.TrueString,
            JsonValueKind.False => bool.FalseString,
            _ => null
        };
    }

    private static bool? GetBool(JsonElement element, string path)
    {
        if (!TryGetElement(element, path, out var found))
            return null;

        return found.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String when bool.TryParse(found.GetString(), out var result) => result,
            _ => null
        };
    }

    private static DateTime? GetDateTime(JsonElement element, string path)
    {
        if (!TryGetElement(element, path, out var found))
            return null;

        if (found.ValueKind == JsonValueKind.String && DateTime.TryParse(found.GetString(), out var result))
            return result;

        return null;
    }

    private static bool TryGetElement(JsonElement element, string path, out JsonElement found)
    {
        found = element;
        foreach (var part in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            if (found.ValueKind != JsonValueKind.Object || !TryGetPropertyCaseInsensitive(found, part, out found))
                return false;
        }

        return true;
    }

    private static bool TryGetPropertyCaseInsensitive(JsonElement element, string propertyName, out JsonElement found)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                found = property.Value;
                return true;
            }
        }

        found = default;
        return false;
    }
}
