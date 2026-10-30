using Brecho.Api.Comum;
using Microsoft.AspNetCore.Mvc;

namespace Brecho.Api.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected ActionResult TraduzirErro<T>(Resultado<T> resultado)
    {
        return resultado.Erro switch
        {
            TipoErro.Validacao => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Erro de validação",
                detail: resultado.Mensagem),
            TipoErro.NaoAutorizado => Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Não autorizado",
                detail: resultado.Mensagem),
            TipoErro.Proibido => Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Acesso negado",
                detail: resultado.Mensagem),
            TipoErro.NaoEncontrado => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Recurso não encontrado",
                detail: resultado.Mensagem),
            TipoErro.Conflito => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflito de regra de negócio",
                detail: resultado.Mensagem),
            _ => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Requisição inválida",
                detail: resultado.Mensagem)
        };
    }
}

