using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace ProdutorRuralMonitoramento.Api.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ProdutorRuralMonitoramentoController : ControllerBase
    {
    }
}
