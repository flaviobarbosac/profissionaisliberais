using Newtonsoft.Json;

namespace Libify.Infraestructure.Services
{
    // Requests (camelCase conforme contrato Asaas)

    public record AsaasSubcontaRequest(
        [property: JsonProperty("name")] string Name,
        [property: JsonProperty("email")] string Email,
        [property: JsonProperty("cpfCnpj")] string CpfCnpj,
        [property: JsonProperty("mobilePhone")] string? MobilePhone = null);

    public record AsaasClienteRequest(
        [property: JsonProperty("name")] string Name,
        [property: JsonProperty("cpfCnpj")] string CpfCnpj,
        [property: JsonProperty("email")] string? Email = null,
        [property: JsonProperty("mobilePhone")] string? MobilePhone = null);

    public record AsaasCobrancaRequest(
        [property: JsonProperty("customer")] string Customer,
        [property: JsonProperty("billingType")] string BillingType,
        [property: JsonProperty("value")] decimal Value,
        [property: JsonProperty("dueDate")] string DueDate,
        [property: JsonProperty("description")] string? Description = null,
        [property: JsonProperty("installmentCount")] int? InstallmentCount = null);

    public record AsaasLinkPagamentoRequest(
        [property: JsonProperty("name")] string Name,
        [property: JsonProperty("billingType")] string BillingType,
        [property: JsonProperty("chargeType")] string ChargeType,
        [property: JsonProperty("value")] decimal? Value = null);

    // Responses

    public record AsaasContaResponse(
        [property: JsonProperty("id")] string? Id,
        [property: JsonProperty("walletId")] string? WalletId,
        [property: JsonProperty("apiKey")] string? ApiKey);

    public record AsaasClienteResponse(
        [property: JsonProperty("id")] string? Id);

    public record AsaasCobrancaResponse(
        [property: JsonProperty("id")] string? Id,
        [property: JsonProperty("status")] string? Status,
        [property: JsonProperty("value")] decimal Value,
        [property: JsonProperty("dueDate")] string? DueDate,
        [property: JsonProperty("invoiceUrl")] string? InvoiceUrl,
        [property: JsonProperty("bankSlipUrl")] string? BankSlipUrl);

    public record AsaasPixQrCodeResponse(
        [property: JsonProperty("encodedImage")] string? EncodedImage,
        [property: JsonProperty("payload")] string? Payload,
        [property: JsonProperty("expirationDate")] string? ExpirationDate);

    public record AsaasLinkPagamentoResponse(
        [property: JsonProperty("id")] string? Id,
        [property: JsonProperty("url")] string? Url);
}
