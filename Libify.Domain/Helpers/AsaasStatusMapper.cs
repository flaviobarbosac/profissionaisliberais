using Libify.Domain.Enum;

namespace Libify.Domain.Helpers
{
    public static class AsaasStatusMapper
    {
        public static StatusCobranca? MapearPagamento(string? statusAsaas)
            => statusAsaas?.Trim().ToUpperInvariant() switch
            {
                "PENDING" => StatusCobranca.Pendente,
                "CONFIRMED" => StatusCobranca.Confirmada,
                "RECEIVED" => StatusCobranca.Recebida,
                "OVERDUE" => StatusCobranca.Vencida,
                "REFUNDED" => StatusCobranca.Estornada,
                "DELETED" => StatusCobranca.Cancelada,
                _ => null
            };

        public static string MapearFormaPagamento(FormaPagamento forma)
            => forma switch
            {
                FormaPagamento.Pix => "PIX",
                FormaPagamento.Boleto => "BOLETO",
                FormaPagamento.CartaoCredito => "CREDIT_CARD",
                _ => "UNDEFINED"
            };

        public static string MapearCicloAssinatura(TipoPlano tipo)
            => tipo switch
            {
                TipoPlano.Mensal => "MONTHLY",
                TipoPlano.Semestral => "SEMIANNUALLY",
                TipoPlano.Anual => "YEARLY",
                _ => throw new ArgumentOutOfRangeException(nameof(tipo), "Tipo de plano não suporta assinatura recorrente.")
            };
    }
}
