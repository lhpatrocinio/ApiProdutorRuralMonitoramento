using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProdutorRuralMonitoramento.Application.DTOs.Request;
using ProdutorRuralMonitoramento.Application.DTOs.Response;
using ProdutorRuralMonitoramento.Application.Services.Interfaces;
using System.Security.Claims;

namespace ProdutorRuralMonitoramento.Api.Controllers.V1
{
    /// <summary>
    /// Controller para gerenciamento de Regras de Alerta
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class RegrasAlertaController : ControllerBase
    {
        private readonly IRegraAlertaService _regraAlertaService;
        private readonly IValidator<RegraAlertaCreateRequest> _createValidator;
        private readonly IValidator<RegraAlertaUpdateRequest> _updateValidator;
        private readonly ILogger<RegrasAlertaController> _logger;

        public RegrasAlertaController(
            IRegraAlertaService regraAlertaService,
            IValidator<RegraAlertaCreateRequest> createValidator,
            IValidator<RegraAlertaUpdateRequest> updateValidator,
            ILogger<RegrasAlertaController> logger)
        {
            _regraAlertaService = regraAlertaService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        /// <summary>
        /// Obtém todas as regras de alerta do produtor autenticado
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RegraAlertaResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<RegraAlertaResponse>>> GetAll()
        {
            var produtorId = GetProdutorId();
            var regras = await _regraAlertaService.GetAllByProdutorAsync(produtorId);
            return Ok(regras);
        }

        /// <summary>
        /// Obtém uma regra de alerta específica por ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(RegraAlertaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RegraAlertaResponse>> GetById(Guid id)
        {
            var regra = await _regraAlertaService.GetByIdAsync(id);
            if (regra == null)
                return NotFound(new { message = "Regra de alerta não encontrada" });
            
            return Ok(regra);
        }

        /// <summary>
        /// Obtém regras de alerta de um talhão específico
        /// </summary>
        [HttpGet("talhao/{talhaoId:guid}")]
        [ProducesResponseType(typeof(IEnumerable<RegraAlertaResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<RegraAlertaResponse>>> GetByTalhao(Guid talhaoId)
        {
            var regras = await _regraAlertaService.GetByTalhaoAsync(talhaoId);
            return Ok(regras);
        }

        /// <summary>
        /// Obtém regras de alerta ativas do produtor
        /// </summary>
        [HttpGet("ativas")]
        [ProducesResponseType(typeof(IEnumerable<RegraAlertaResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<RegraAlertaResponse>>> GetAtivas()
        {
            var produtorId = GetProdutorId();
            var regras = await _regraAlertaService.GetAtivasAsync(produtorId);
            return Ok(regras);
        }

        /// <summary>
        /// Cria uma nova regra de alerta
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(RegraAlertaResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RegraAlertaResponse>> Create([FromBody] RegraAlertaCreateRequest request)
        {
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var produtorId = GetProdutorId();
            var regra = await _regraAlertaService.CreateAsync(produtorId, request);
            
            return CreatedAtAction(nameof(GetById), new { id = regra.Id }, regra);
        }

        /// <summary>
        /// Atualiza uma regra de alerta existente
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(RegraAlertaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RegraAlertaResponse>> Update(Guid id, [FromBody] RegraAlertaUpdateRequest request)
        {
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            try
            {
                var regra = await _regraAlertaService.UpdateAsync(id, request);
                return Ok(regra);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Regra de alerta não encontrada" });
            }
        }

        /// <summary>
        /// Exclui uma regra de alerta
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _regraAlertaService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Regra de alerta não encontrada" });
            }
        }

        /// <summary>
        /// Ativa uma regra de alerta
        /// </summary>
        [HttpPatch("{id:guid}/ativar")]
        [ProducesResponseType(typeof(RegraAlertaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RegraAlertaResponse>> Ativar(Guid id)
        {
            try
            {
                var regra = await _regraAlertaService.AtivarAsync(id);
                return Ok(regra);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Regra de alerta não encontrada" });
            }
        }

        /// <summary>
        /// Desativa uma regra de alerta
        /// </summary>
        [HttpPatch("{id:guid}/desativar")]
        [ProducesResponseType(typeof(RegraAlertaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RegraAlertaResponse>> Desativar(Guid id)
        {
            try
            {
                var regra = await _regraAlertaService.DesativarAsync(id);
                return Ok(regra);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Regra de alerta não encontrada" });
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
