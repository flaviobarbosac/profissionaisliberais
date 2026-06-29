namespace Libify.Domain.Enum
{
    public enum CategoriaProfissional
    {
        Professor = 1,
        Arquiteto = 2,
        Advogado = 3,
        Eletricista = 4,
        Pintor = 5,
        Encanador = 6,
        Diarista = 7,
        Fotografo = 8,
        Consultor = 9,
        Outro = 99
    }

    public enum PropostaStatus
    {
        Rascunho = 1,
        Enviada = 2,
        Aceita = 3,
        Recusada = 4
    }

    public enum ContratoStatus
    {
        Pendente = 1,
        Assinado = 2,
        Cancelado = 3
    }

    public enum FormaPagamento
    {
        Pix = 1,
        Boleto = 2,
        CartaoCredito = 3,
        Todos = 4
    }

    public enum StatusCobranca
    {
        Pendente = 1,
        Confirmada = 2,
        Recebida = 3,
        Vencida = 4,
        Estornada = 5,
        Cancelada = 6
    }

    public enum TipoLancamento
    {
        Pagar = 1,
        Receber = 2
    }

    public enum StatusLancamento
    {
        Pendente = 1,
        Pago = 2,
        Vencido = 3,
        Cancelado = 4
    }

    public enum StatusNotaFiscal
    {
        Pendente = 1,
        Emitida = 2,
        Cancelada = 3
    }

    public enum TipoPlano
    {
        Gratuito = 1,
        Mensal = 2,
        Semestral = 3,
        Anual = 4
    }

    public enum StatusEvento
    {
        Agendado = 1,
        Concluido = 2,
        Cancelado = 3
    }

    public enum StatusContaAsaas
    {
        Pendente = 1,
        Aprovada = 2,
        Recusada = 3
    }
}
