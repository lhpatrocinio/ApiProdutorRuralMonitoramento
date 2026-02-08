using AutoMapper;
using ProdutorRuralMonitoramento.Application.DTOs.Response;
using ProdutorRuralMonitoramento.Domain.Entities;

namespace ProdutorRuralMonitoramento.Application.Mappings;

/// <summary>
/// Perfil de mapeamento AutoMapper
/// </summary>
public class MapperProfile : Profile
{
    public MapperProfile()
    {
        // Alerta Mappings
        CreateMap<Alerta, AlertaResponse>()
            .ForMember(dest => dest.TipoAlerta, opt => opt.MapFrom(src => src.TipoAlerta.ToString()))
            .ForMember(dest => dest.Severidade, opt => opt.MapFrom(src => src.Severidade.ToString()));

        CreateMap<Alerta, AlertaComRegraResponse>()
            .ForMember(dest => dest.TipoAlerta, opt => opt.MapFrom(src => src.TipoAlerta.ToString()))
            .ForMember(dest => dest.Severidade, opt => opt.MapFrom(src => src.Severidade.ToString()))
            .ForMember(dest => dest.RegraAlerta, opt => opt.MapFrom(src => src.RegraAlerta));

        // RegraAlerta Mappings
        CreateMap<RegraAlerta, RegraAlertaResponse>()
            .ForMember(dest => dest.Operador, opt => opt.MapFrom(src => src.Operador.ToString()))
            .ForMember(dest => dest.TipoAlerta, opt => opt.MapFrom(src => src.TipoAlerta.ToString()))
            .ForMember(dest => dest.Severidade, opt => opt.MapFrom(src => src.Severidade.ToString()));

        // HistoricoStatusTalhao Mappings
        CreateMap<HistoricoStatusTalhao, HistoricoStatusTalhaoResponse>();
    }
}
