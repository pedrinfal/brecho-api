namespace Brecho.Api.Comum;

public enum TipoErro
{
    Validacao,
    NaoEncontrado,
    Proibido,
    Conflito,
    NaoAutorizado
}

public class Resultado<T>
{
    public bool Sucesso { get; }
    public T? Valor { get; }
    public TipoErro? Erro { get; }
    public string? Mensagem { get; }

    private Resultado(bool sucesso, T? valor, TipoErro? erro, string? mensagem)
    {
        Sucesso = sucesso;
        Valor = valor;
        Erro = erro;
        Mensagem = mensagem;
    }

    public static Resultado<T> Ok(T valor) => new(true, valor, null, null);
    public static Resultado<T> Falha(TipoErro erro, string mensagem) => new(false, default, erro, mensagem);
}

