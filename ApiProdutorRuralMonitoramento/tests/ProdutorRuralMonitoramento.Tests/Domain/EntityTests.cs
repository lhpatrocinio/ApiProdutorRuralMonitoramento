using FluentAssertions;
using ProdutorRuralMonitoramento.Domain.Entities;

namespace ProdutorRuralMonitoramento.Tests.Domain;

public class AlertaEntityTests
{
    [Fact]
    public void Construtor_DeveCriarAlertaComValoresCorretos()
    {
        var produtorId = Guid.NewGuid();
        var talhaoId = Guid.NewGuid();
        var regraId = Guid.NewGuid();
        var leituraId = Guid.NewGuid();

        var alerta = new Alerta(
            produtorId, talhaoId, regraId,
            TipoAlerta.Temperatura, Severidade.Critica,
            "Temperatura Alta", "Desc", 42m, leituraId);

        alerta.Id.Should().NotBeEmpty();
        alerta.ProdutorId.Should().Be(produtorId);
        alerta.TalhaoId.Should().Be(talhaoId);
        alerta.RegraAlertaId.Should().Be(regraId);
        alerta.LeituraId.Should().Be(leituraId);
        alerta.TipoAlerta.Should().Be(TipoAlerta.Temperatura);
        alerta.Severidade.Should().Be(Severidade.Critica);
        alerta.Titulo.Should().Be("Temperatura Alta");
        alerta.ValorLeitura.Should().Be(42m);
        alerta.Lido.Should().BeFalse();
        alerta.Resolvido.Should().BeFalse();
    }

    [Fact]
    public void MarcarComoLido_DeveAtualizarLidoELidoEm()
    {
        var alerta = new Alerta(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            TipoAlerta.Seca, Severidade.Alta, "Seca", null);

        alerta.MarcarComoLido();

        alerta.Lido.Should().BeTrue();
        alerta.LidoEm.Should().NotBeNull();
        alerta.LidoEm.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Resolver_DeveAtualizarResolvidoEObs()
    {
        var resolvidoPor = Guid.NewGuid();
        var alerta = new Alerta(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            TipoAlerta.Precipitacao, Severidade.Media, "Chuva", null);

        alerta.Resolver(resolvidoPor, "Irrigação ajustada");

        alerta.Resolvido.Should().BeTrue();
        alerta.ResolvidoEm.Should().NotBeNull();
        alerta.ResolvidoPor.Should().Be(resolvidoPor);
        alerta.Observacao.Should().Be("Irrigação ajustada");
    }

    [Fact]
    public void Resolver_SemObservacao_DeveAceitarNull()
    {
        var alerta = new Alerta(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            TipoAlerta.Geada, Severidade.Baixa, "Geada", null);

        alerta.Resolver(Guid.NewGuid());

        alerta.Resolvido.Should().BeTrue();
        alerta.Observacao.Should().BeNull();
    }
}

public class RegraAlertaEntityTests
{
    [Fact]
    public void Construtor_DeveCriarRegraComValoresCorretos()
    {
        var produtorId = Guid.NewGuid();
        var talhaoId = Guid.NewGuid();

        var regra = new RegraAlerta(produtorId, talhaoId, "Temp Alta", "Desc",
            CampoMonitorado.Temperatura, Operador.Maior, 35m,
            TipoAlerta.Temperatura, Severidade.Alta);

        regra.ProdutorId.Should().Be(produtorId);
        regra.TalhaoId.Should().Be(talhaoId);
        regra.Nome.Should().Be("Temp Alta");
        regra.Campo.Should().Be(CampoMonitorado.Temperatura);
        regra.Operador.Should().Be(Operador.Maior);
        regra.Valor.Should().Be(35m);
        regra.Ativo.Should().BeTrue();
    }

    [Fact]
    public void Atualizar_DeveAtualizarTodosOsCampos()
    {
        var regra = new RegraAlerta(Guid.NewGuid(), null, "Original", null,
            CampoMonitorado.Temperatura, Operador.Maior, 40m,
            TipoAlerta.Temperatura, Severidade.Alta);

        regra.Atualizar("Atualizada", "Nova desc",
            CampoMonitorado.UmidadeSolo, Operador.Menor, 20m,
            TipoAlerta.Seca, Severidade.Critica);

        regra.Nome.Should().Be("Atualizada");
        regra.Descricao.Should().Be("Nova desc");
        regra.Campo.Should().Be(CampoMonitorado.UmidadeSolo);
        regra.Operador.Should().Be(Operador.Menor);
        regra.Valor.Should().Be(20m);
        regra.TipoAlerta.Should().Be(TipoAlerta.Seca);
        regra.Severidade.Should().Be(Severidade.Critica);
    }

    [Fact]
    public void Ativar_DeveDefinirAtivoTrue()
    {
        var regra = new RegraAlerta(Guid.NewGuid(), null, "R", null,
            "temperatura", Operador.Maior, 40m, TipoAlerta.Temperatura, Severidade.Alta);
        regra.Desativar();

        regra.Ativar();

        regra.Ativo.Should().BeTrue();
    }

    [Fact]
    public void Desativar_DeveDefinirAtivoFalse()
    {
        var regra = new RegraAlerta(Guid.NewGuid(), null, "R", null,
            "temperatura", Operador.Maior, 40m, TipoAlerta.Temperatura, Severidade.Alta);

        regra.Desativar();

        regra.Ativo.Should().BeFalse();
    }
}

public class HistoricoStatusTalhaoEntityTests
{
    [Fact]
    public void Construtor_DeveCriarHistoricoComValoresCorretos()
    {
        var talhaoId = Guid.NewGuid();
        var leituraId = Guid.NewGuid();

        var historico = new HistoricoStatusTalhao(talhaoId, "Alerta - Solo Seco", "Desc", leituraId);

        historico.Id.Should().NotBeEmpty();
        historico.TalhaoId.Should().Be(talhaoId);
        historico.Status.Should().Be("Alerta - Solo Seco");
        historico.Descricao.Should().Be("Desc");
        historico.LeituraId.Should().Be(leituraId);
        historico.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
