using System.Text.Json.Serialization;

namespace Libify.Infraestructure.Services
{
  // --- Requests ---

  public record AsaasWebhookConfig(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("authToken")] string AuthToken,
    [property: JsonPropertyName("sendType")] string SendType = "SEQUENTIALLY",
    [property: JsonPropertyName("interrupted")] bool Interrupted = false,
    [property: JsonPropertyName("enabled")] bool Enabled = true,
    [property: JsonPropertyName("apiVersion")] int ApiVersion = 3,
    [property: JsonPropertyName("events")] string[]? Events = null);

  public record AsaasSubcontaRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("cpfCnpj")] string CpfCnpj,
    [property: JsonPropertyName("mobilePhone")] string? MobilePhone = null,
    [property: JsonPropertyName("companyType")] string? CompanyType = null,
    [property: JsonPropertyName("birthDate")] string? BirthDate = null,
    [property: JsonPropertyName("address")] string? Address = null,
    [property: JsonPropertyName("addressNumber")] string? AddressNumber = null,
    [property: JsonPropertyName("complement")] string? Complement = null,
    [property: JsonPropertyName("province")] string? Province = null,
    [property: JsonPropertyName("postalCode")] string? PostalCode = null,
    [property: JsonPropertyName("webhooks")] AsaasWebhookConfig[]? Webhooks = null);

  public record AsaasClienteRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("cpfCnpj")] string CpfCnpj,
    [property: JsonPropertyName("email")] string? Email = null,
    [property: JsonPropertyName("mobilePhone")] string? MobilePhone = null);

  public record AsaasCobrancaRequest(
    [property: JsonPropertyName("customer")] string Customer,
    [property: JsonPropertyName("billingType")] string BillingType,
    [property: JsonPropertyName("value")] decimal Value,
    [property: JsonPropertyName("dueDate")] string DueDate,
    [property: JsonPropertyName("description")] string? Description = null,
    [property: JsonPropertyName("installmentCount")] int? InstallmentCount = null,
    [property: JsonPropertyName("externalReference")] string? ExternalReference = null);

  public record AsaasAssinaturaRequest(
    [property: JsonPropertyName("customer")] string Customer,
    [property: JsonPropertyName("billingType")] string BillingType,
    [property: JsonPropertyName("value")] decimal Value,
    [property: JsonPropertyName("cycle")] string Cycle,
    [property: JsonPropertyName("nextDueDate")] string NextDueDate,
    [property: JsonPropertyName("description")] string? Description = null);

  public record AsaasLinkPagamentoRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("billingType")] string BillingType,
    [property: JsonPropertyName("chargeType")] string ChargeType,
    [property: JsonPropertyName("value")] decimal? Value = null);

  // --- Responses ---

  public record AsaasContaResponse(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("walletId")] string? WalletId,
    [property: JsonPropertyName("apiKey")] string? ApiKey,
    [property: JsonPropertyName("status")] string? Status);

  public record AsaasClienteResponse(
    [property: JsonPropertyName("id")] string? Id);

  public record AsaasCobrancaResponse(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("value")] decimal Value,
    [property: JsonPropertyName("dueDate")] string? DueDate,
    [property: JsonPropertyName("invoiceUrl")] string? InvoiceUrl,
    [property: JsonPropertyName("bankSlipUrl")] string? BankSlipUrl);

  public record AsaasAssinaturaResponse(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("value")] decimal Value,
    [property: JsonPropertyName("cycle")] string? Cycle);

  public record AsaasPixQrCodeResponse(
    [property: JsonPropertyName("encodedImage")] string? EncodedImage,
    [property: JsonPropertyName("payload")] string? Payload,
    [property: JsonPropertyName("expirationDate")] string? ExpirationDate);

  public record AsaasLinkPagamentoResponse(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("url")] string? Url);

  // --- Erros ---

  public record AsaasErrorItem(
    [property: JsonPropertyName("code")] string? Code,
    [property: JsonPropertyName("description")] string? Description);

  public record AsaasErrorResponse(
    [property: JsonPropertyName("errors")] AsaasErrorItem[]? Errors);
}
