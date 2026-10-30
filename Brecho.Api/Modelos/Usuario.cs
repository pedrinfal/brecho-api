namespace Brecho.Api.Modelos;

public class Usuario
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public string Papel { get; private set; } = string.Empty;
    public DateTime CriadoEm { get; private set; }

    public ICollection<Venda> Vendas { get; private set; } = new List<Venda>();

    private Usuario() { }

    public Usuario(string nome, string email, string senhaHash, string papel, DateTime criadoEm)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length < 2)
            throw new ArgumentException("O nome do usuário deve ter pelo menos 2 caracteres.", nameof(nome));

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("O e-mail informado é inválido.", nameof(email));

        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new ArgumentException("O hash da senha é obrigatório.", nameof(senhaHash));

        if (papel != "admin" && papel != "cliente")
            throw new ArgumentException("O papel deve ser 'admin' ou 'cliente'.", nameof(papel));

        if (criadoEm.Kind != DateTimeKind.Utc)
            throw new ArgumentException("A data de criação deve estar em UTC (DateTimeKind.Utc).", nameof(criadoEm));

        Nome = nome.Trim();
        Email = email.Trim().ToLowerInvariant();
        SenhaHash = senhaHash;
        Papel = papel;
        CriadoEm = criadoEm;
    }
}

