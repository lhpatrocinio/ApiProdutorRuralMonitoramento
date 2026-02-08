using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProdutorRuralMonitoramento.Application.DTOs.Request;
using ProdutorRuralMonitoramento.Application.DTOs.Response;
using ProdutorRuralMonitoramento.Application.Services.Interfaces;
using System.Security.Claims;

namespace ProdutorRuralMonitoramento.Api.Controllers.V1
{
    /// <summary>
    /// Controller para gerenciamento de Alertas
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class AlertasController : ControllerBase
    {
        private readonly IAlertaService _alertaService;
        private readonly ILogger<AlertasController> _logger;

        public AlertasController(IAlertaService alertaService, ILogger<AlertasController> logger)
        {
            _alertaService = alertaService;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint de teste - retorna status da API
        /// </summary>
        [HttpGet("status")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public ActionResult GetStatus()
        {
            return Ok(new { 
                status = "online", 
                api = "API Monitoramento",
                timestamp = DateTime.UtcNow,
                version = "1.0"
            });
        }

        /// <summary>
        /// Obtém todos os alertas do produtor autenticado
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AlertaResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AlertaResponse>>> GetAll()
        {
            var produtorId = GetProdutorId();
            var alertas = await _alertaService.GetAllByProdutorAsync(produtorId);
            return Ok(alertas);
        }

        /// <summary>
        /// Obtém um alerta específico por ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(AlertaComRegraResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AlertaComRegraResponse>> GetById(Guid id)
        {
            var alerta = await _alertaService.GetByIdAsync(id);
            if (alerta == null)
                return NotFound(new { message = "Alerta não encontrado" });
            
            return Ok(alerta);
        }

        /// <summary>
        /// Obtém alertas de um talhão específico
        /// </summary>
        [HttpGet("talhao/{talhaoId:guid}")]
        [ProducesResponseType(typeof(IEnumerable<AlertaResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AlertaResponse>>> GetByTalhao(Guid talhaoId)
        {
            var alertas = await _alertaService.GetByTalhaoAsync(talhaoId);
            return Ok(alertas);
        }

        /// <summary>
        /// Obtém alertas não lidos do produtor
        /// </summary>
        [HttpGet("nao-lidos")]
        [ProducesResponseType(typeof(IEnumerable<AlertaResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AlertaResponse>>> GetNaoLidos()
        {
            var produtorId = GetProdutorId();
            var alertas = await _alertaService.GetNaoLidosAsync(produtorId);
            return Ok(alertas);
        }

        /// <summary>
        /// Obtém alertas não resolvidos do produtor
        /// </summary>
        [HttpGet("nao-resolvidos")]
        [ProducesResponseType(typeof(IEnumerable<AlertaResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AlertaResponse>>> GetNaoResolvidos()
        {
            var produtorId = GetProdutorId();
            var alertas = await _alertaService.GetNaoResolvidosAsync(produtorId);
            return Ok(alertas);
        }

        /// <summary>
        /// Obtém alertas recentes do produtor
        /// </summary>
        [HttpGet("recentes")]
        [ProducesResponseType(typeof(IEnumerable<AlertaResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AlertaResponse>>> GetRecentes([FromQuery] int quantidade = 10)
        {
            var produtorId = GetProdutorId();
            var alertas = await _alertaService.GetRecentesAsync(produtorId, quantidade);
            return Ok(alertas);
        }

        /// <summary>
        /// Obtém alertas com filtros
        /// </summary>
        [HttpGet("filtrar")]
        [ProducesResponseType(typeof(IEnumerable<AlertaResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AlertaResponse>>> GetByFiltro([FromQuery] AlertaFiltroRequest filtro)
        {
            var produtorId = GetProdutorId();
            var alertas = await _alertaService.GetByFiltroAsync(produtorId, filtro);
            return Ok(alertas);
        }

        /// <summary>
        /// Obtém resumo dos alertas do produtor
        /// </summary>
        [HttpGet("resumo")]
        [ProducesResponseType(typeof(AlertaResumoResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<AlertaResumoResponse>> GetResumo()
        {
            var produtorId = GetProdutorId();
            var resumo = await _alertaService.GetResumoAsync(produtorId);
            return Ok(resumo);
        }

        /// <summary>
        /// Conta alertas não lidos do produtor
        /// </summary>
        [HttpGet("count/nao-lidos")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> CountNaoLidos()
        {
            var produtorId = GetProdutorId();
            var count = await _alertaService.CountNaoLidosAsync(produtorId);
            return Ok(count);
        }

        /// <summary>
        /// Conta alertas não resolvidos do produtor
        /// </summary>
        [HttpGet("count/nao-resolvidos")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> CountNaoResolvidos()
        {
            var produtorId = GetProdutorId();
            var count = await _alertaService.CountNaoResolvidosAsync(produtorId);
            return Ok(count);
        }

        /// <summary>
        /// Marca um alerta como lido
        /// </summary>
        [HttpPatch("{id:guid}/lido")]
        [ProducesResponseType(typeof(AlertaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AlertaResponse>> MarcarComoLido(Guid id)
        {
            try
            {
                var alerta = await _alertaService.MarcarComoLidoAsync(id);
                return Ok(alerta);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Alerta não encontrado" });
            }
        }

        /// <summary>
        /// Marca todos os alertas do produtor como lidos
        /// </summary>
        [HttpPatch("marcar-todos-lidos")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> MarcarTodosComoLidos()
        {
            var produtorId = GetProdutorId();
            await _alertaService.MarcarTodosComoLidosAsync(produtorId);
            return NoContent();
        }

        /// <summary>
        /// Resolve um alerta
        /// </summary>
        [HttpPatch("{id:guid}/resolver")]
        [ProducesResponseType(typeof(AlertaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AlertaResponse>> Resolver(Guid id, [FromBody] AlertaResolverRequest request)
        {
            try
            {
                var resolvidoPor = GetProdutorId();
                var alerta = await _alertaService.ResolverAsync(id, resolvidoPor, request);
                return Ok(alerta);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Alerta não encontrado" });
            }
        }

        private Guid GetProdutorId()
        {
            var produtorIdClaim = User.FindFirst("ProdutorId")?.Value 
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(produtorIdClaim) || !Guid.TryParse(produtorIdClaim, out var produtorId))
            {
                // Para desenvolvimento, usar um ID fixo
                return Guid.Parse("11111111-1111-1111-1111-111111111111");
            }
            
            return produtorId;
        }
    }
}
