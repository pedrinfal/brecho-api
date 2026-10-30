using Brecho.Api.Dtos;
using Brecho.Api.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Brecho.Api.Controllers;

[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly AuthServico _servico;

    public AuthController(AuthServico servico)
    {
        _servico = servico;
    }

    [HttpPost("registro")]
    [ProducesResponseType(typeof(AuthRespostaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthRespostaDto>> Registrar([FromBody] RegistroDto dto)
    {
        var resultado = await _servico.RegistrarAsync(dto);
        if (!resultado.Sucesso || resultado.Valor is null)
            return TraduzirErro(resultado);

        return Created($"/api/auth/usuarios/{resultado.Valor.Id}", resultado.Valor);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthRespostaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthRespostaDto>> Login([FromBody] LoginDto dto)
    {
        var resultado = await _servico.LoginAsync(dto);
        if (!resultado.Sucesso || resultado.Valor is null)
            return TraduzirErro(resultado);

        return Ok(resultado.Valor);
    }
}

