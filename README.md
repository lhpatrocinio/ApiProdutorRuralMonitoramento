# 🚨 AgroSolutions - API de Monitoramento

Microsserviço responsável pelo **motor de alertas inteligente** da plataforma AgroSolutions, processando leituras de sensores e gerando alertas automáticos com base em regras configuráveis.

## Visão Geral

| Item | Detalhe |
|------|---------|
| **Porta padrão** | 5003 |
| **Banco de dados** | AgroMonitoramento (SQL Server) |
| **Endpoints** | 23 |
| **Testes unitários** | 71 |
| **Autenticação** | JWT Bearer (obrigatório) |

## Responsabilidades

- Receber e processar leituras de sensores via RabbitMQ (`SensorDataReceivedEvent`)
- Avaliar leituras contra regras de monitoramento configuráveis (seca, temperatura, precipitação, geada)
- Gerar alertas com severidade automática (Baixa, Média, Alta, Crítica)
- Publicar evento `AlertCreatedEvent` para que a API de Cadastro atualize o status do talhão
- Gerenciar o ciclo de vida dos alertas (aberto → em atendimento → resolvido)

## Estrutura do Projeto (Clean Architecture)

```
ApiProdutorRuralMonitoramento/
├── ProdutorRuralMonitoramento.Domain/         # Entidades, regras de domínio, motor de alertas
├── ProdutorRuralMonitoramento.Application/    # Use cases, DTOs, handlers de eventos
├── ProdutorRuralMonitoramento.Infrastructure/ # EF Core, SQL Server, RabbitMQ Consumer/Publisher
├── ProdutorRuralMonitoramento.Api/            # Controllers, middlewares, Swagger
└── ProdutorRuralMonitoramento.Tests/          # Testes unitários (xUnit + Moq)
```

## Endpoints

### Alertas (`/api/v1/Alertas`)

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET`    | `/` | Lista todos os alertas |
| `GET`    | `/{id}` | Busca alerta por ID |
| `GET`    | `/talhao/{talhaoId}` | Alertas de um talhão |
| `GET`    | `/talhao/{talhaoId}/ativos` | Alertas ativos do talhão |
| `GET`    | `/severidade/{severidade}` | Alertas por severidade |
| `GET`    | `/periodo` | Alertas por período |
| `GET`    | `/estatisticas` | Estatísticas gerais de alertas |
| `POST`   | `/` | Cria alerta manualmente |
| `PATCH`  | `/{id}/resolver` | Marca alerta como resolvido |
| `PATCH`  | `/{id}/em-atendimento` | Marca alerta como em atendimento |
| `DELETE` | `/{id}` | Remove alerta |

### Regras de Monitoramento (`/api/v1/RegrasMonitoramento`)

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET`    | `/` | Lista todas as regras |
| `GET`    | `/{id}` | Busca regra por ID |
| `GET`    | `/talhao/{talhaoId}` | Regras de um talhão |
| `GET`    | `/tipo/{tipoSensor}` | Regras por tipo de sensor |
| `POST`   | `/` | Cria nova regra |
| `PUT`    | `/{id}` | Atualiza regra |
| `DELETE` | `/{id}` | Remove regra |
| `PATCH`  | `/{id}/ativar` | Ativa regra |
| `PATCH`  | `/{id}/desativar` | Desativa regra |
| `GET`    | `/exportar` | Exporta regras CSV |
| `POST`   | `/importar` | Importa regras CSV |
| `GET`    | `/default` | Lista regras padrão do sistema |

## Como Executar Localmente

### Pré-requisitos

- .NET 8 SDK
- SQL Server + RabbitMQ rodando (via Docker — ver [AgroSolutions-Infra](https://github.com/marceloms17/AgroSolutions-Infra))
- Token JWT obtido via [API de Autenticação](https://github.com/lhpatrocinio/ApiProdutorRuralAutenticacao)

### Executar

```powershell
git clone https://github.com/lhpatrocinio/ApiProdutorRuralMonitoramento.git
cd ApiProdutorRuralMonitoramento
dotnet restore
dotnet run --project ProdutorRuralMonitoramento.Api
```

Swagger disponível em: `http://localhost:5003/swagger`

### Executar Testes

```powershell
dotnet test
```

## Motor de Alertas

### Tipos de Regras

| Tipo | Sensor | Exemplo de Trigger |
|------|--------|--------------------|
| Seca | Umidade do solo | < 20% por 3 leituras consecutivas |
| Temperatura crítica | Temperatura | > 40°C ou < -2°C |
| Excesso hídrico | Precipitação | > 150mm em 24h |
| Geada | Temperatura | < 0°C |

### Severidade automática

| Severidade | Critério |
|------------|----------|
| **Baixa** | Valor fora do range normal |
| **Média** | Persistência por 2+ leituras |
| **Alta** | Persistência por 4+ leituras |
| **Crítica** | Valor extremo ou risco iminente |

## Mensageria RabbitMQ

| Direção | Exchange | Routing Key | Evento |
|---------|----------|-------------|--------|
| **Consome** | `agro.events` | `sensor.data.#` | `SensorDataReceivedEvent` → avalia regras |
| **Publica** | `agro.events` | `alert.created.{talhaoId}` | `AlertCreatedEvent` → atualiza status talhão |

- Dead Letter Queue: `agro.events.dlq`
- Retry: 3x com exponential backoff

## Dados de Seed

- 12 regras de monitoramento configuradas
- 24 alertas históricos (severidades variadas)

## Tecnologias

- .NET 8 / ASP.NET Core
- Entity Framework Core 8 + SQL Server
- RabbitMQ (MassTransit) — Consumer + Publisher
- JWT Bearer Authentication
- Polly (Resilience: Retry, Circuit Breaker, Bulkhead, Timeout)
- Swagger / OpenAPI
- xUnit + Moq + FluentAssertions
- GitHub Actions (CI/CD)