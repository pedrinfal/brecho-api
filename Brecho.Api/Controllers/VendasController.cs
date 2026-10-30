using System.Security.Claims;
using Brecho.Api.Dtos;
using Brecho.Api.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brecho.Api.Controllers;

[Route("api/vendas")]
[Authorize]
public class VendasController : BaseApiController
{
    private readonly VendaServico _servico;

    public VendasController(VendaServico servico)
    {
        _servico = servico;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<VendaRespostaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<VendaRespostaDto>>> Listar()
    {
        var (usuarioId, papel) = ObterIdentidadeClaims();
        var lista = await _servico.ListarAsync(usuarioId, papel);
        return Ok(lista);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(VendaRespostaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VendaRespostaDto>> ObterPorId(int id)
    {
        var (usuarioId, papel) = ObterIdentidadeClaims();
        var resultado = await _servico.ObterPorIdAsync(id, usuarioId, papel);
        if (!resultado.Sucesso || resultado.Valor is null)
            return TraduzirErro(resultado);

        return Ok(resultado.Valor);
    }

    [HttpPost]
    [ProducesResponseType(typeof(VendaRespostaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VendaRespostaDto>> Criar([FromBody] CriarVendaDto dto)
    {
        var (usuarioId, _) = ObterIdentidadeClaims();
        var resultado = await _servico.CriarAsync(usuarioId, dto);
        if (!resultado.Sucesso || resultado.Valor is null)
            return TraduzirErro(resultado);

        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Valor.Id }, resultado.Valor);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(VendaRespostaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VendaRespostaDto>> Atualizar(int id, [FromBody] AtualizarVendaDto dto)
    {
        var (usuarioId, papel) = ObterIdentidadeClaims();
        var resultado = await _servico.AtualizarAsync(id, usuarioId, papel, dto);
        if (!resultado.Sucesso || resultado.Valor is null)
            return TraduzirErro(resultado);

        return Ok(resultado.Valor);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(int id)
    {
        var (usuarioId, papel) = ObterIdentidadeClaims();
        var resultado = await _servico.CancelarOuExcluirAsync(id, usuarioId, papel);
        if (!resultado.Sucesso)
            return TraduzirErro(resultado);

        return NoContent();
    }

    private (int UsuarioId, string Papel) ObterIdentidadeClaims()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var papel = User.FindFirstValue(ClaimTypes.Role) ?? "cliente";

        if (!int.TryParse(idStr, out var usuarioId))
            throw new InvalidOperationException("Identificador do usuário ausente no token JWT.");

        return (usuarioId, papel);
    }
}

