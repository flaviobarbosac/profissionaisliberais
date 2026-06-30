using AutoMapper;
using Libify.API.Dto;
using Libify.Domain.Model;

namespace Libify.API.Configuration
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Usuario, UsuarioDto>().ReverseMap();
            CreateMap<Cliente, ClienteDto>().ReverseMap();
            CreateMap<Servico, ServicoDto>().ReverseMap();
            CreateMap<Proposta, PropostaDto>().ReverseMap();
            CreateMap<PropostaItem, PropostaItemDto>().ReverseMap();
            CreateMap<Contrato, ContratoDto>().ReverseMap();
            CreateMap<Cobranca, CobrancaDto>().ReverseMap();
            CreateMap<LancamentoFinanceiro, LancamentoFinanceiroDto>().ReverseMap();
            CreateMap<NotaFiscal, NotaFiscalDto>().ReverseMap();
            CreateMap<Evento, EventoDto>().ReverseMap();
            CreateMap<Tarefa, TarefaDto>().ReverseMap();
            CreateMap<Post, PostDto>().ReverseMap();
            CreateMap<Plano, PlanoDto>().ReverseMap();
            CreateMap<Dispositivo, DispositivoDto>();
        }
    }
}
