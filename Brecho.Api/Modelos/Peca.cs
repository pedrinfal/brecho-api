namespace Brecho.Api.Modelos;

public class Peca
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public string Tamanho { get; private set; } = string.Empty;
    public string Categoria { get; private set; } = string.Empty;
    public string Condicao { get; private set; } = string.Empty;
    public decimal Preco { get; private set; }
    public bool Vendida { get; private set; }
    public bool Ativa { get; private set; }
    public DateTime CriadaEm { get; private set; }

    public ICollection<Venda> Vendas { get; private set; } = new List<Venda>();

    private Peca() { }

    public Peca(
        string nome,
        string descricao,
        string tamanho,
        string categoria,
        string condicao,
        decimal preco,
        DateTime criadaEm,
        bool ativa = true)
    {
        ValidarDados(nome, descricao, tamanho, categoria, condicao, preco);

        if (criadaEm.Kind != DateTimeKind.Utc)
            throw new ArgumentException("A data de cadastro da peça deve estar em UTC (DateTimeKind.Utc).", nameof(criadaEm));

        Nome = nome.Trim();
        Descricao = descricao.Trim();
        Tamanho = tamanho.Trim().ToUpperInvariant();
        Categoria = categoria.Trim();
        Condicao = condicao.Trim();
        Preco = preco;
        Vendida = false;
        Ativa = ativa;
        CriadaEm = criadaEm;
    }

    public void Atualizar(
        string nome,
        string descricao,
        string tamanho,
        string categoria,
        string condicao,
        decimal preco,
        bool ativa)
    {
        ValidarDados(nome, descricao, tamanho, categoria, condicao, preco);

        Nome = nome.Trim();
        Descricao = descricao.Trim();
        Tamanho = tamanho.Trim().ToUpperInvariant();
        Categoria = categoria.Trim();
        Condicao = condicao.Trim();
        Preco = preco;
        Ativa = ativa;
    }

    public void MarcarComoVendida()
    {
        if (!Ativa)
            throw new InvalidOperationException("Não é possível vender uma peça inativa.");

        if (Vendida)
            throw new InvalidOperationException("Esta peça única já foi vendida (estoque de uma unidade).");

        Vendida = true;
    }

    public void LiberarParaVenda()
    {
        Vendida = false;
    }

    private static void ValidarDados(
        string nome,
        string descricao,
        string tamanho,
        string categoria,
        string condicao,
        decimal preco)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length < 3)
            throw new ArgumentException("O nome da peça deve ter pelo menos 3 caracteres.", nameof(nome));

        if (string.IsNullOrWhiteSpace(descricao) || descricao.Trim().Length < 5)
            throw new ArgumentException("A descrição da peça deve ter pelo menos 5 caracteres.", nameof(descricao));

        if (string.IsNullOrWhiteSpace(tamanho))
            throw new ArgumentException("O tamanho da peça é obrigatório.", nameof(tamanho));

        if (string.IsNullOrWhiteSpace(categoria))
            throw new ArgumentException("A categoria da peça é obrigatória.", nameof(categoria));

        if (string.IsNullOrWhiteSpace(condicao))
            throw new ArgumentException("A condição de conservação da peça é obrigatória.", nameof(condicao));

        if (preco <= 0)
            throw new ArgumentException("O preço da peça deve ser maior que zero.", nameof(preco));
    }
}

