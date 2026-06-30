namespace Libify.Infraestructure.Services.Interface
{
    /// <summary>
    /// Proxy de acesso à API do Asaas (white-label). A chave da subconta (apiKey do usuário)
    /// é passada por chamada para garantir o isolamento de dados entre os prestadores.
    /// </summary>
    public interface IAsaasClient
    {
        /// <summary>Cria uma subconta (KYC) na plataforma. Usa a API Key da plataforma.</summary>
        Task<AsaasContaResponse> CriarSubcontaAsync(AsaasSubcontaRequest dadosConta, CancellationToken cancellationToken = default);

        /// <summary>Cadastra um pagador (customer) na subconta.</summary>
        Task<AsaasClienteResponse> CriarClienteAsync(string apiKeySubconta, AsaasClienteRequest cliente, CancellationToken cancellationToken = default);

        /// <summary>Cria uma cobrança (PIX / Boleto / Cartão) na subconta.</summary>
        Task<AsaasCobrancaResponse> CriarCobrancaAsync(string apiKeySubconta, AsaasCobrancaRequest cobranca, CancellationToken cancellationToken = default);

        /// <summary>Obtém o QR Code PIX dinâmico de uma cobrança.</summary>
        Task<AsaasPixQrCodeResponse> ObterPixQrCodeAsync(string apiKeySubconta, string paymentId, CancellationToken cancellationToken = default);

        /// <summary>Cria um link de pagamento / página de checkout.</summary>
        Task<AsaasLinkPagamentoResponse> CriarLinkPagamentoAsync(string apiKeySubconta, AsaasLinkPagamentoRequest linkPagamento, CancellationToken cancellationToken = default);
    }
}
