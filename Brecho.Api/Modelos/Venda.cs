namespace Brecho.Api.Modelos;

public class Venda
{
    public int Id { get; private set; }
    public int PecaId { get; private set; }
    public Peca? Peca { get; private set; }
    public int UsuarioId { get; private set; }
    public Usuario? Usuario { get; private set; }
    public DateTime DataVenda { get; private set; }
    public string FormaPagamento { get; private set; } = string.Empty;
    public string? Observacoes { get; private set; }
    public string Situacao { get; private set; } = string.Empty;

    private Venda() { }

    public Venda(int pecaId, int usuarioId, DateTime dataVenda, string formaPagamento, string? observacoes = null)
    {
        if (pecaId <= 0)
            throw new ArgumentException("O identificador da peça deve ser maior que zero.", nameof(pecaId));

        if (usuarioId <= 0)
            throw new ArgumentException("O identificador do usuário deve ser maior que zero.", nameof(usuarioId));

        if (dataVenda.Kind != DateTimeKind.Utc)
            throw new ArgumentException("A data da venda deve estar em UTC (DateTimeKind.Utc).", nameof(dataVenda));

        if (string.IsNullOrWhiteSpace(formaPagamento))
            throw new ArgumentException("A forma de pagamento é obrigatória.", nameof(formaPagamento));

        PecaId = pecaId;
        UsuarioId = usuarioId;
        DataVenda = dataVenda;
        FormaPagamento = formaPagamento.Trim();
        Observacoes = string.IsNullOrWhiteSpace(observacoes) ? null : observacoes.Trim();
        Situacao = "Concluida";
    }

    public void Cancelar()
    {
        if (Situacao == "Cancelada")
            throw new InvalidOperationException("Esta venda já está cancelada.");

        Situacao = "Cancelada";
    }
}

