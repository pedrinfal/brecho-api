using System.Security.Claims;
using Brecho.Api.Dtos;
using Brecho.Api.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brecho.Api.Controllers;

[Route("api/pecas")]
public class PecasController : BaseApiController
{
    private readonly PecaServico _servico;

    public PecasController(PecaServico servico)
    {
        _servico = servico;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ListaPaginadaDto<PecaRespostaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ListaPaginadaDto<PecaRespostaDto>>> Listar(
        [FromQuery] string? busca = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 10)
    {
        var ehAdmin = User.Identity?.IsAuthenticated == true &&
                      User.FindFirstValue(ClaimTypes.Role) == "admin";

        var resposta = await _servico.ListarAsync(busca, pagina, tamanho, incluirInativas: ehAdmin);
        return Ok(resposta);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PecaRespostaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PecaRespostaDto>> ObterPorId(int id)
    {
        var resultado = await _servico.ObterPorIdAsync(id);
        if (!resultado.Sucesso || resultado.Valor is null)
            return TraduzirErro(resultado);

        return Ok(resultado.Valor);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(PecaRespostaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PecaRespostaDto>> Criar([FromBody] CriarPecaDto dto)
    {
        var resultado = await _servico.CriarAsync(dto);
        if (!resultado.Sucesso || resultado.Valor is null)
            return TraduzirErro(resultado);

        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Valor.Id }, resultado.Valor);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(PecaRespostaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PecaRespostaDto>> Atualizar(int id, [FromBody] AtualizarPecaDto dto)
    {
        var resultado = await _servico.AtualizarAsync(id, dto);
        if (!resultado.Sucesso || resultado.Valor is null)
            return TraduzirErro(resultado);

        return Ok(resultado.Valor);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Excluir(int id)
    {
        var resultado = await _servico.ExcluirAsync(id);
        if (!resultado.Sucesso)
            return TraduzirErro(resultado);

        return NoContent();
    }
}

