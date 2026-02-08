namespace ProdutorRuralMonitoramento.Domain.Entities;

/// <summary>
/// Tipos de alerta monitorados pelo sistema
/// </summary>
public enum TipoAlerta
{
    Seca = 1,
    Temperatura = 2,
    Precipitacao = 3,
    Geada = 4
}

/// <summary>
/// Níveis de severidade do alerta
/// </summary>
public enum Severidade
{
    Baixa = 1,
    Media = 2,
    Alta = 3,
    Critica = 4
}

/// <summary>
/// Operadores disponíveis para regras de alerta
/// </summary>
public enum Operador
{
    MenorQue,       // <
    MaiorQue,       // >
    MenorOuIgual,   // <=
    MaiorOuIgual,   // >=
    Igual           // ==
}

/// <summary>
/// Campos monitoráveis para alertas
/// </summary>
public static class CampoMonitorado
{
    public const string UmidadeSolo = "umidade_solo";
    public const string Temperatura = "temperatura";
    public const string Precipitacao = "precipitacao";
    public const string UmidadeAr = "umidade_ar";
    public const string VelocidadeVento = "velocidade_vento";
}
